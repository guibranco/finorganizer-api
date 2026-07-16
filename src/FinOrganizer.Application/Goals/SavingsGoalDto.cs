namespace FinOrganizer.Application.Goals;

public sealed record SavingsGoalDto(
    Guid Id,
    string Name,
    decimal TargetAmount,
    DateOnly TargetDate,
    Guid? LinkedAccountId,
    decimal CurrentAmount,
    decimal ProgressPercent);

public sealed record CreateSavingsGoalRequest(string Name, decimal TargetAmount, DateOnly TargetDate, Guid? LinkedAccountId);

public sealed record UpdateSavingsGoalRequest(string Name, decimal TargetAmount, DateOnly TargetDate, Guid? LinkedAccountId);

public sealed record SavingsGoalContributionDto(Guid Id, Guid SavingsGoalId, decimal Amount, DateOnly Date, string? Note);

public sealed record AddSavingsGoalContributionRequest(decimal Amount, DateOnly Date, string? Note);
