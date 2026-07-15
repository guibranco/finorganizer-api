using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Projection;

public sealed class ProjectionService(IApplicationDbContext db, IPriceProvider priceProvider, IDateTimeProvider dateTimeProvider) : IProjectionService
{
    public async Task<List<ProjectionMonthDto>> ProjectAsync(int monthsAhead, CancellationToken cancellationToken = default)
    {
        var today = dateTimeProvider.Today;

        var accounts = await db.Accounts.Where(a => !a.IsArchived).ToListAsync(cancellationToken);
        var transactions = await db.Transactions.ToListAsync(cancellationToken);
        var rules = await db.RecurrenceRules.Where(r => r.IsActive).ToListAsync(cancellationToken);

        var runningBalances = accounts.ToDictionary(
            a => a.Id,
            a => AccountBalanceCalculator.ComputeBalance(a, transactions.Where(t => t.AccountId == a.Id || t.CounterpartyAccountId == a.Id)));

        var assets = await db.Assets.ToListAsync(cancellationToken);
        var events = await db.AssetEvents.ToListAsync(cancellationToken);
        var prices = await priceProvider.GetLatestPricesAsync(assets.Select(a => a.Id).ToList(), today, cancellationToken);
        var portfolioValue = assets.Sum(asset =>
        {
            var position = PortfolioCalculator.ComputePosition(asset.Id, events.Where(e => e.AssetId == asset.Id));
            return prices.TryGetValue(asset.Id, out var price) ? position.MarketValue(price) : 0m;
        });

        // Each rule's cursor advances independently of its persisted NextDueDate, starting from it.
        var simulatedNextDue = rules.ToDictionary(r => r.Id, r => r.NextDueDate);

        var cumulativeNetWorth = runningBalances.Values.Sum() + portfolioValue;
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var result = new List<ProjectionMonthDto>(Math.Max(0, monthsAhead));

        for (var i = 1; i <= monthsAhead; i++)
        {
            var targetMonthStart = monthStart.AddMonths(i);
            var targetMonthEnd = targetMonthStart.AddMonths(1).AddDays(-1);
            var monthIncome = 0m;
            var monthExpense = 0m;

            foreach (var rule in rules)
            {
                var due = simulatedNextDue[rule.Id];

                while (due <= targetMonthEnd && (rule.EndDate is null || due <= rule.EndDate))
                {
                    if (due >= targetMonthStart)
                    {
                        switch (rule.Type)
                        {
                            case TransactionType.Income:
                                monthIncome += rule.Amount;
                                runningBalances[rule.AccountId] = runningBalances.GetValueOrDefault(rule.AccountId) + rule.Amount;
                                break;
                            case TransactionType.Expense:
                                monthExpense += rule.Amount;
                                runningBalances[rule.AccountId] = runningBalances.GetValueOrDefault(rule.AccountId) - rule.Amount;
                                break;
                        }
                    }

                    due = RecurrenceDateCalculator.GetNextDueDate(rule, due);
                }

                simulatedNextDue[rule.Id] = due;
            }

            var net = monthIncome - monthExpense;
            cumulativeNetWorth += net;

            var accountBalances = accounts
                .Select(a => new ProjectionAccountBalanceDto(a.Id, a.Name, runningBalances.GetValueOrDefault(a.Id)))
                .ToList();

            result.Add(new ProjectionMonthDto(targetMonthStart, monthIncome, monthExpense, net, cumulativeNetWorth, accountBalances));
        }

        return result;
    }
}
