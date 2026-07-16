namespace FinOrganizer.Application.Projection;

public sealed record ProjectionAccountBalanceDto(Guid AccountId, string AccountName, decimal ProjectedBalance);

public sealed record ProjectionMonthDto(
    DateOnly Month,
    decimal RecurringIncome,
    decimal RecurringExpense,
    decimal Net,
    decimal CumulativeNetWorth,
    IReadOnlyList<ProjectionAccountBalanceDto> AccountBalances);
