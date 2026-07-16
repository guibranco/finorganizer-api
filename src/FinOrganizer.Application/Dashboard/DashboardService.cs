using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Dashboard;

public sealed class DashboardService(IApplicationDbContext db, IPriceProvider priceProvider, IDateTimeProvider dateTimeProvider) : IDashboardService
{
    public async Task<List<NetWorthPointDto>> GetNetWorthOverTimeAsync(int months, CancellationToken cancellationToken = default)
    {
        var monthEnds = GetMonthEndsAscending(months, dateTimeProvider.Today);

        var accounts = await db.Accounts.ToListAsync(cancellationToken);
        var allTransactions = await db.Transactions.ToListAsync(cancellationToken);
        var assets = await db.Assets.ToListAsync(cancellationToken);
        var allEvents = await db.AssetEvents.ToListAsync(cancellationToken);
        var assetIds = assets.Select(a => a.Id).ToList();

        var result = new List<NetWorthPointDto>(monthEnds.Count);

        foreach (var monthEnd in monthEnds)
        {
            var relevantTransactions = allTransactions.Where(t => t.Date <= monthEnd).ToList();
            var accountsBalance = accounts.Sum(a => AccountBalanceCalculator.ComputeBalance(
                a, relevantTransactions.Where(t => t.AccountId == a.Id || t.CounterpartyAccountId == a.Id)));

            var relevantEvents = allEvents.Where(e => e.Date <= monthEnd).ToList();
            var prices = await priceProvider.GetLatestPricesAsync(assetIds, monthEnd, cancellationToken);
            var portfolioValue = assets.Sum(asset =>
            {
                var position = PortfolioCalculator.ComputePosition(asset.Id, relevantEvents.Where(e => e.AssetId == asset.Id));
                return prices.TryGetValue(asset.Id, out var price) ? position.MarketValue(price) : 0m;
            });

            result.Add(new NetWorthPointDto(monthEnd, accountsBalance, portfolioValue, accountsBalance + portfolioValue));
        }

        return result;
    }

    public async Task<AllocationDto> GetAllocationAsync(CancellationToken cancellationToken = default)
    {
        var today = dateTimeProvider.Today;

        var assets = await db.Assets.ToListAsync(cancellationToken);
        var events = await db.AssetEvents.ToListAsync(cancellationToken);
        var prices = await priceProvider.GetLatestPricesAsync(assets.Select(a => a.Id).ToList(), today, cancellationToken);

        var byAssetClass = assets
            .Select(a => (a.Class, Value: prices.TryGetValue(a.Id, out var p)
                ? PortfolioCalculator.ComputePosition(a.Id, events.Where(e => e.AssetId == a.Id)).MarketValue(p)
                : 0m))
            .GroupBy(x => x.Class)
            .Select(g => (Class: g.Key, Value: g.Sum(x => x.Value)))
            .Where(x => x.Value != 0)
            .ToList();

        var totalAssetValue = byAssetClass.Sum(x => x.Value);
        var byAssetClassDtos = byAssetClass
            .Select(x => new AllocationByAssetClassDto(x.Class, x.Value, Percent(x.Value, totalAssetValue)))
            .OrderByDescending(x => x.MarketValue)
            .ToList();

        var accounts = await db.Accounts.Where(a => !a.IsArchived).ToListAsync(cancellationToken);
        var transactions = await db.Transactions.ToListAsync(cancellationToken);
        var accountBalances = accounts
            .Select(a => (Account: a, Balance: AccountBalanceCalculator.ComputeBalance(
                a, transactions.Where(t => t.AccountId == a.Id || t.CounterpartyAccountId == a.Id))))
            .ToList();

        var totalAccountBalance = accountBalances.Sum(x => x.Balance);
        var byAccountDtos = accountBalances
            .Select(x => new AllocationByAccountDto(x.Account.Id, x.Account.Name, x.Balance, Percent(x.Balance, totalAccountBalance)))
            .OrderByDescending(x => x.Balance)
            .ToList();

        return new AllocationDto(byAssetClassDtos, byAccountDtos);
    }

    public async Task<List<IncomeVsExpenseMonthDto>> GetIncomeVsExpenseAsync(int months, CancellationToken cancellationToken = default)
    {
        var monthStarts = GetMonthStartsAscending(months, dateTimeProvider.Today);
        var transactions = await db.Transactions
            .Where(t => t.Date >= monthStarts[0] && (t.Type == TransactionType.Income || t.Type == TransactionType.Expense))
            .ToListAsync(cancellationToken);

        return monthStarts.Select(monthStart =>
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthTransactions = transactions.Where(t => t.Date >= monthStart && t.Date <= monthEnd).ToList();
            var income = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return new IncomeVsExpenseMonthDto(monthStart, income, expense, income - expense);
        }).ToList();
    }

    public async Task<PassiveIncomeSummaryDto> GetPassiveIncomeAsync(int months, CancellationToken cancellationToken = default)
    {
        var monthStarts = GetMonthStartsAscending(months, dateTimeProvider.Today);
        var earliestStart = monthStarts[0];

        var passiveCategoryIds = await db.Categories.Where(c => c.IsPassive).Select(c => c.Id).ToListAsync(cancellationToken);
        var passiveTransactions = await db.Transactions
            .Where(t => t.Date >= earliestStart && t.Type == TransactionType.Income
                        && t.CategoryId != null && passiveCategoryIds.Contains(t.CategoryId.Value))
            .ToListAsync(cancellationToken);

        var incomeEvents = await db.AssetEvents
            .Where(e => e.Date >= earliestStart
                        && (e.Type == AssetEventType.Dividend || e.Type == AssetEventType.Distribution || e.Type == AssetEventType.Interest))
            .ToListAsync(cancellationToken);

        var monthly = monthStarts.Select(monthStart =>
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var txnTotal = passiveTransactions.Where(t => t.Date >= monthStart && t.Date <= monthEnd).Sum(t => t.Amount);
            var eventTotal = incomeEvents.Where(e => e.Date >= monthStart && e.Date <= monthEnd).Sum(e => e.UnitPrice);
            return new PassiveIncomeMonthDto(monthStart, txnTotal + eventTotal);
        }).ToList();

        var trailingTwelveMonthTotal = monthly.TakeLast(12).Sum(m => m.Amount);
        return new PassiveIncomeSummaryDto(monthly, trailingTwelveMonthTotal);
    }

    public async Task<List<TopExpenseCategoryDto>> GetTopExpenseCategoriesAsync(DateOnly from, DateOnly to, int top, CancellationToken cancellationToken = default)
    {
        var transactions = await db.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= from && t.Date <= to && t.CategoryId != null)
            .ToListAsync(cancellationToken);

        var categories = await db.Categories.ToDictionaryAsync(c => c.Id, cancellationToken);
        var total = transactions.Sum(t => t.Amount);

        return transactions
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new TopExpenseCategoryDto(
                g.Key, categories.GetValueOrDefault(g.Key)?.Name ?? "(unknown)", g.Sum(t => t.Amount), Percent(g.Sum(t => t.Amount), total)))
            .OrderByDescending(x => x.Amount)
            .Take(top)
            .ToList();
    }

    private static decimal Percent(decimal part, decimal whole) => whole == 0 ? 0 : Math.Round(part / whole * 100, 2);

    private static List<DateOnly> GetMonthStartsAscending(int months, DateOnly today)
    {
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
        return Enumerable.Range(0, Math.Max(1, months))
            .Select(i => currentMonthStart.AddMonths(-(Math.Max(1, months) - 1 - i)))
            .ToList();
    }

    private static List<DateOnly> GetMonthEndsAscending(int months, DateOnly today)
        => GetMonthStartsAscending(months, today)
            .Select(start => start.AddMonths(1).AddDays(-1) is var end && end > today ? today : end)
            .ToList();
}
