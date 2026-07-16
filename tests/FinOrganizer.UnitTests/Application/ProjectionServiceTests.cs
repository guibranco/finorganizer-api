using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Application.Projection;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.UnitTests.TestHelpers;
using NSubstitute;

namespace FinOrganizer.UnitTests.Application;

public class ProjectionServiceTests
{
    private static readonly DateOnly Today = new(2026, 7, 15);

    private static IDateTimeProvider FixedClock()
    {
        var clock = Substitute.For<IDateTimeProvider>();
        clock.Today.Returns(Today);
        return clock;
    }

    private static IPriceProvider NoAssetsPriceProvider()
    {
        var provider = Substitute.For<IPriceProvider>();
        provider.GetLatestPricesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, decimal>());
        return provider;
    }

    [Fact]
    public async Task ProjectAsync_MonthlyIncomeRule_AppliesOnceEachMonthItFallsInAndAccumulatesNetWorth()
    {
        using var db = new SqliteInMemoryDatabase();

        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 1000m };
        var category = new Category { Name = "Salary", Kind = CategoryKind.Income };
        db.Context.Accounts.Add(account);
        db.Context.Categories.Add(category);
        db.Context.RecurrenceRules.Add(new RecurrenceRule
        {
            Name = "Paycheck",
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Income,
            Amount = 100m,
            Currency = "USD",
            Frequency = RecurrenceFrequency.Monthly,
            DayOfMonth = 15,
            Interval = 1,
            StartDate = new DateOnly(2026, 6, 15),
            NextDueDate = new DateOnly(2026, 8, 15), // first occurrence inside the projection window
            AutoPost = true,
            IsActive = true,
        });
        await db.Context.SaveChangesAsync();

        var service = new ProjectionService(db.Context, NoAssetsPriceProvider(), FixedClock());
        var result = await service.ProjectAsync(monthsAhead: 2);

        Assert.Equal(2, result.Count);

        var august = result[0];
        Assert.Equal(new DateOnly(2026, 8, 1), august.Month);
        Assert.Equal(100m, august.RecurringIncome);
        Assert.Equal(0m, august.RecurringExpense);
        Assert.Equal(1100m, august.CumulativeNetWorth);
        Assert.Equal(1100m, Assert.Single(august.AccountBalances).ProjectedBalance);

        var september = result[1];
        Assert.Equal(new DateOnly(2026, 9, 1), september.Month);
        Assert.Equal(100m, september.RecurringIncome);
        Assert.Equal(1200m, september.CumulativeNetWorth);
        Assert.Equal(1200m, Assert.Single(september.AccountBalances).ProjectedBalance);
    }

    [Fact]
    public async Task ProjectAsync_ExpenseRule_ReducesAccountBalanceAndNetWorth()
    {
        using var db = new SqliteInMemoryDatabase();

        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 500m };
        var category = new Category { Name = "Rent", Kind = CategoryKind.Expense };
        db.Context.Accounts.Add(account);
        db.Context.Categories.Add(category);
        db.Context.RecurrenceRules.Add(new RecurrenceRule
        {
            Name = "Rent",
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            Amount = 200m,
            Currency = "USD",
            Frequency = RecurrenceFrequency.Monthly,
            DayOfMonth = 1,
            Interval = 1,
            StartDate = new DateOnly(2026, 8, 1),
            NextDueDate = new DateOnly(2026, 8, 1),
            AutoPost = true,
            IsActive = true,
        });
        await db.Context.SaveChangesAsync();

        var service = new ProjectionService(db.Context, NoAssetsPriceProvider(), FixedClock());
        var result = await service.ProjectAsync(monthsAhead: 1);

        var august = Assert.Single(result);
        Assert.Equal(200m, august.RecurringExpense);
        Assert.Equal(-200m, august.Net);
        Assert.Equal(300m, august.CumulativeNetWorth);
    }

    [Fact]
    public async Task ProjectAsync_RuleEndingBeforeTargetMonth_StopsBeingApplied()
    {
        using var db = new SqliteInMemoryDatabase();

        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 0m };
        var category = new Category { Name = "Freelance", Kind = CategoryKind.Income };
        db.Context.Accounts.Add(account);
        db.Context.Categories.Add(category);
        db.Context.RecurrenceRules.Add(new RecurrenceRule
        {
            Name = "Short contract",
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Income,
            Amount = 50m,
            Currency = "USD",
            Frequency = RecurrenceFrequency.Monthly,
            DayOfMonth = 1,
            Interval = 1,
            StartDate = new DateOnly(2026, 8, 1),
            NextDueDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 1),
            AutoPost = true,
            IsActive = true,
        });
        await db.Context.SaveChangesAsync();

        var service = new ProjectionService(db.Context, NoAssetsPriceProvider(), FixedClock());
        var result = await service.ProjectAsync(monthsAhead: 2);

        Assert.Equal(50m, result[0].RecurringIncome); // August: within EndDate
        Assert.Equal(0m, result[1].RecurringIncome);  // September: rule has ended
    }
}
