using FinOrganizer.Domain.Common;

namespace FinOrganizer.Domain.Entities;

public class SavingsGoal : Entity
{
    public required string Name { get; set; }

    public decimal TargetAmount { get; set; }

    public DateOnly TargetDate { get; set; }

    /// <summary>When set, current progress tracks this account's balance instead of manual contributions.</summary>
    public Guid? LinkedAccountId { get; set; }
}
