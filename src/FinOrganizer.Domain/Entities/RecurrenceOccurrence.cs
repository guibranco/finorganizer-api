using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

/// <summary>
/// One materialized due date for a <see cref="RecurrenceRule"/>. Auto-posted rules are recorded here
/// as an idempotency guard; manual rules sit as PendingConfirmation until the UI confirms or skips them.
/// </summary>
public class RecurrenceOccurrence : Entity
{
    public Guid RecurrenceRuleId { get; set; }

    public DateOnly DueDate { get; set; }

    /// <summary>Snapshot of the rule's amount/currency at materialization time, so later rule edits don't retroactively change pending items.</summary>
    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public RecurrenceOccurrenceStatus Status { get; set; } = RecurrenceOccurrenceStatus.PendingConfirmation;

    public Guid? PostedTransactionId { get; set; }
}
