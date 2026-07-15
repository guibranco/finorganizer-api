using FinOrganizer.Application.Budgets;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.UnitTests.TestHelpers;

namespace FinOrganizer.UnitTests.Application;

public class BudgetsServiceTests
{
    [Fact]
    public async Task GetBudgetVsActualAsync_SumsExpensesInMonthForCategoryAndComputesRemainderAndPercent()
    {
        using var db = new SqliteInMemoryDatabase();

        var category = new Category { Name = "Groceries", Kind = CategoryKind.Expense };
        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 0 };
        db.Context.Categories.Add(category);
        db.Context.Accounts.Add(account);
        db.Context.Budgets.Add(new Budget { CategoryId = category.Id, Month = new DateOnly(2026, 3, 1), LimitAmount = 500m });

        db.Context.Transactions.AddRange(
            new Transaction { AccountId = account.Id, Type = TransactionType.Expense, Amount = 120m, Currency = "USD", Date = new DateOnly(2026, 3, 5), CategoryId = category.Id },
            new Transaction { AccountId = account.Id, Type = TransactionType.Expense, Amount = 180m, Currency = "USD", Date = new DateOnly(2026, 3, 20), CategoryId = category.Id },
            // Outside the budgeted month - must not count.
            new Transaction { AccountId = account.Id, Type = TransactionType.Expense, Amount = 999m, Currency = "USD", Date = new DateOnly(2026, 4, 1), CategoryId = category.Id },
            // Income in the same category/month - must not count as spend.
            new Transaction { AccountId = account.Id, Type = TransactionType.Income, Amount = 50m, Currency = "USD", Date = new DateOnly(2026, 3, 10), CategoryId = category.Id });
        await db.Context.SaveChangesAsync();

        var service = new BudgetsService(db.Context);
        var result = await service.GetBudgetVsActualAsync(new DateOnly(2026, 3, 15));

        var line = Assert.Single(result);
        Assert.Equal(category.Id, line.CategoryId);
        Assert.Equal(500m, line.LimitAmount);
        Assert.Equal(300m, line.ActualAmount);
        Assert.Equal(200m, line.Remaining);
        Assert.Equal(60m, line.PercentUsed);
    }

    [Fact]
    public async Task GetBudgetVsActualAsync_NoSpendYet_ReturnsZeroActualAndFullLimitRemaining()
    {
        using var db = new SqliteInMemoryDatabase();

        var category = new Category { Name = "Travel", Kind = CategoryKind.Expense };
        db.Context.Categories.Add(category);
        db.Context.Budgets.Add(new Budget { CategoryId = category.Id, Month = new DateOnly(2026, 5, 1), LimitAmount = 300m });
        await db.Context.SaveChangesAsync();

        var service = new BudgetsService(db.Context);
        var result = await service.GetBudgetVsActualAsync(new DateOnly(2026, 5, 1));

        var line = Assert.Single(result);
        Assert.Equal(0m, line.ActualAmount);
        Assert.Equal(300m, line.Remaining);
        Assert.Equal(0m, line.PercentUsed);
    }
}
