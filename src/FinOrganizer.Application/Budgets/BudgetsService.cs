using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Budgets;

public sealed class BudgetsService(IApplicationDbContext db) : IBudgetsService
{
    public async Task<List<BudgetDto>> GetAllAsync(DateOnly? month, CancellationToken cancellationToken = default)
    {
        var query = db.Budgets.AsQueryable();
        if (month is { } m)
        {
            var normalized = FirstOfMonth(m);
            query = query.Where(b => b.Month == normalized);
        }

        return await query.OrderBy(b => b.Month).Select(ToDtoExpression).ToListAsync(cancellationToken);
    }

    public async Task<BudgetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAsync(id, cancellationToken));

    public async Task<BudgetDto> CreateAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var month = FirstOfMonth(request.Month);

        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        if (await db.Budgets.AnyAsync(b => b.CategoryId == request.CategoryId && b.Month == month, cancellationToken))
        {
            throw new ValidationException("A budget for this category and month already exists.");
        }

        var budget = new Budget { CategoryId = request.CategoryId, Month = month, LimitAmount = request.LimitAmount };
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(budget);
    }

    public async Task<BudgetDto> UpdateAsync(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var budget = await FindAsync(id, cancellationToken);
        budget.LimitAmount = request.LimitAmount;
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(budget);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var budget = await FindAsync(id, cancellationToken);
        db.Budgets.Remove(budget);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<BudgetVsActualDto>> GetBudgetVsActualAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        var normalized = FirstOfMonth(month);
        var monthEnd = normalized.AddMonths(1).AddDays(-1);

        var budgets = await db.Budgets.Where(b => b.Month == normalized).ToListAsync(cancellationToken);
        var categoryIds = budgets.Select(b => b.CategoryId).ToList();
        var categories = await db.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var actuals = await db.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= normalized && t.Date <= monthEnd
                        && t.CategoryId != null && categoryIds.Contains(t.CategoryId.Value))
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total, cancellationToken);

        return budgets.Select(b =>
        {
            var actual = actuals.GetValueOrDefault(b.CategoryId);
            var categoryName = categories.GetValueOrDefault(b.CategoryId)?.Name ?? "(unknown)";
            var percentUsed = b.LimitAmount == 0 ? 0 : Math.Round(actual / b.LimitAmount * 100, 2);
            return new BudgetVsActualDto(b.CategoryId, categoryName, normalized, b.LimitAmount, actual, b.LimitAmount - actual, percentUsed);
        }).ToList();
    }

    private static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    private async Task<Budget> FindAsync(Guid id, CancellationToken cancellationToken)
        => await db.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(Budget), id);

    private static readonly System.Linq.Expressions.Expression<Func<Budget, BudgetDto>> ToDtoExpression = b =>
        new BudgetDto(b.Id, b.CategoryId, b.Month, b.LimitAmount);

    private static BudgetDto ToDto(Budget b) => new(b.Id, b.CategoryId, b.Month, b.LimitAmount);
}
