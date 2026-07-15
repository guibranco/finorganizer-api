using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Accounts;

public sealed class AccountsService(IApplicationDbContext db) : IAccountsService
{
    public async Task<List<AccountDto>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default)
    {
        var accounts = await db.Accounts
            .Where(a => includeArchived || !a.IsArchived)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var accountIds = accounts.Select(a => a.Id).ToHashSet();
        var transactions = await db.Transactions
            .Where(t => accountIds.Contains(t.AccountId) || (t.CounterpartyAccountId != null && accountIds.Contains(t.CounterpartyAccountId.Value)))
            .ToListAsync(cancellationToken);

        return accounts.Select(a => ToDto(a, transactions)).ToList();
    }

    public async Task<AccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(id, cancellationToken);
        var transactions = await GetAccountTransactionsAsync(id, cancellationToken);
        return ToDto(account, transactions);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            Name = request.Name,
            Type = request.Type,
            Currency = request.Currency,
            InitialBalance = request.InitialBalance,
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(account, []);
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(id, cancellationToken);
        account.Name = request.Name;
        account.Type = request.Type;
        account.Currency = request.Currency;
        account.IsArchived = request.IsArchived;

        await db.SaveChangesAsync(cancellationToken);
        var transactions = await GetAccountTransactionsAsync(id, cancellationToken);
        return ToDto(account, transactions);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await FindAsync(id, cancellationToken);
        var hasTransactions = await db.Transactions.AnyAsync(
            t => t.AccountId == id || t.CounterpartyAccountId == id, cancellationToken);

        if (hasTransactions)
        {
            throw new ValidationException(
                $"Account '{account.Name}' has transactions and cannot be deleted. Archive it instead.");
        }

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Account> FindAsync(Guid id, CancellationToken cancellationToken)
        => await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(Account), id);

    private async Task<List<Transaction>> GetAccountTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        => await db.Transactions
            .Where(t => t.AccountId == accountId || t.CounterpartyAccountId == accountId)
            .ToListAsync(cancellationToken);

    private static AccountDto ToDto(Account account, IEnumerable<Transaction> allTransactions)
    {
        var relevant = allTransactions.Where(t => t.AccountId == account.Id || t.CounterpartyAccountId == account.Id);
        var balance = AccountBalanceCalculator.ComputeBalance(account, relevant);
        return new AccountDto(
            account.Id, account.Name, account.Type, account.Currency,
            account.InitialBalance, balance, account.IsArchived, account.CreatedAtUtc);
    }
}
