namespace FinOrganizer.Application.Budgets;

public sealed record BudgetDto(Guid Id, Guid CategoryId, DateOnly Month, decimal LimitAmount);

public sealed record CreateBudgetRequest(Guid CategoryId, DateOnly Month, decimal LimitAmount);

public sealed record UpdateBudgetRequest(decimal LimitAmount);

public sealed record BudgetVsActualDto(
    Guid CategoryId,
    string CategoryName,
    DateOnly Month,
    decimal LimitAmount,
    decimal ActualAmount,
    decimal Remaining,
    decimal PercentUsed);
