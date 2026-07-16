using FinOrganizer.Domain.Common;

namespace FinOrganizer.Domain.Entities;

/// <summary>A manual contribution toward a goal that has no <see cref="SavingsGoal.LinkedAccountId"/>.</summary>
public class SavingsGoalContribution : Entity
{
    public Guid SavingsGoalId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string? Note { get; set; }
}
