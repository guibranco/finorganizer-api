using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Application.Common.Models;
using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Transactions;

public sealed class TransactionsService(IApplicationDbContext db) : ITransactionsService
{
    public async Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionFilter filter, CancellationToken cancellationToken = default)
    {
        var query = db.Transactions.AsQueryable();

        if (filter.AccountId is { } accountId)
        {
            query = query.Where(t => t.AccountId == accountId || t.CounterpartyAccountId == accountId);
        }

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (filter.Type is { } type)
        {
            query = query.Where(t => t.Type == type);
        }

        if (filter.DateFrom is { } dateFrom)
        {
            query = query.Where(t => t.Date >= dateFrom);
        }

        if (filter.DateTo is { } dateTo)
        {
            query = query.Where(t => t.Date <= dateTo);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query = query.Where(t => t.Description != null && EF.Functions.Like(t.Description, $"%{filter.SearchText}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);

        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDtoExpression)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>(items, totalCount, page, pageSize);
    }

    public async Task<TransactionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAsync(id, cancellationToken));

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAccountsExistAsync(request.AccountId, request.CounterpartyAccountId, cancellationToken);

        var transaction = new Transaction
        {
            AccountId = request.AccountId,
            Type = request.Type,
            Amount = request.Amount,
            Currency = request.Currency,
            Date = request.Date,
            Description = request.Description,
            CategoryId = request.CategoryId,
            CounterpartyAccountId = request.CounterpartyAccountId,
            Tags = request.Tags?.ToList() ?? [],
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    public async Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await FindAsync(id, cancellationToken);
        await EnsureAccountsExistAsync(request.AccountId, request.CounterpartyAccountId, cancellationToken);

        transaction.AccountId = request.AccountId;
        transaction.Type = request.Type;
        transaction.Amount = request.Amount;
        transaction.Currency = request.Currency;
        transaction.Date = request.Date;
        transaction.Description = request.Description;
        transaction.CategoryId = request.CategoryId;
        transaction.CounterpartyAccountId = request.CounterpartyAccountId;
        transaction.Tags = request.Tags?.ToList() ?? [];

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await FindAsync(id, cancellationToken);
        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TransactionImportSummary> ImportCsvAsync(ImportTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Accounts.AnyAsync(a => a.Id == request.AccountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var parseResult = CsvTransactionParser.Parse(
            request.CsvContent, request.Mapping, request.HasHeaderRow, request.Delimiter, request.DateFormat, request.TagsSeparator);

        var categories = await db.Categories.ToListAsync(cancellationToken);
        var imported = 0;

        foreach (var row in parseResult.Rows.Where(r => r.Transaction is not null))
        {
            var parsed = row.Transaction!;
            var categoryId = parsed.CategoryName is null
                ? null
                : categories.FirstOrDefault(c => string.Equals(c.Name, parsed.CategoryName, StringComparison.OrdinalIgnoreCase))?.Id;

            db.Transactions.Add(new Transaction
            {
                AccountId = request.AccountId,
                Type = parsed.Type,
                Amount = parsed.Amount,
                Currency = request.Currency,
                Date = parsed.Date,
                Description = parsed.Description,
                CategoryId = categoryId,
                Tags = parsed.Tags.ToList(),
            });
            imported++;
        }

        if (imported > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        var errors = parseResult.Rows.Where(r => r.Error is not null).ToList();
        return new TransactionImportSummary(imported, errors);
    }

    private async Task EnsureAccountsExistAsync(Guid accountId, Guid? counterpartyAccountId, CancellationToken cancellationToken)
    {
        if (!await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        if (counterpartyAccountId is { } counterpartyId && !await db.Accounts.AnyAsync(a => a.Id == counterpartyId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), counterpartyId);
        }
    }

    private async Task<Transaction> FindAsync(Guid id, CancellationToken cancellationToken)
        => await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(Transaction), id);

    private static readonly System.Linq.Expressions.Expression<Func<Transaction, TransactionDto>> ToDtoExpression = t =>
        new TransactionDto(t.Id, t.AccountId, t.Type, t.Amount, t.Currency, t.Date, t.Description,
            t.CategoryId, t.CounterpartyAccountId, t.RecurrenceId, t.Tags);

    private static TransactionDto ToDto(Transaction t) => new(
        t.Id, t.AccountId, t.Type, t.Amount, t.Currency, t.Date, t.Description,
        t.CategoryId, t.CounterpartyAccountId, t.RecurrenceId, t.Tags);
}
