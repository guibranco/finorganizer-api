using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Dashboard;

public sealed record NetWorthPointDto(DateOnly Month, decimal AccountsBalance, decimal PortfolioValue, decimal NetWorth);

public sealed record AllocationByAssetClassDto(AssetClass Class, decimal MarketValue, decimal Percent);

public sealed record AllocationByAccountDto(Guid AccountId, string AccountName, decimal Balance, decimal Percent);

public sealed record AllocationDto(
    IReadOnlyList<AllocationByAssetClassDto> ByAssetClass,
    IReadOnlyList<AllocationByAccountDto> ByAccount);

public sealed record IncomeVsExpenseMonthDto(DateOnly Month, decimal Income, decimal Expense, decimal Net);

public sealed record PassiveIncomeMonthDto(DateOnly Month, decimal Amount);

public sealed record PassiveIncomeSummaryDto(IReadOnlyList<PassiveIncomeMonthDto> Months, decimal TrailingTwelveMonthTotal);

public sealed record TopExpenseCategoryDto(Guid CategoryId, string CategoryName, decimal Amount, decimal Percent);
