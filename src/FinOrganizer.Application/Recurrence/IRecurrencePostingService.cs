namespace FinOrganizer.Application.Recurrence;

/// <summary>Materializes due <see cref="Domain.Entities.RecurrenceRule"/> occurrences: auto-posts or queues for confirmation.</summary>
public interface IRecurrencePostingService
{
    /// <returns>Number of occurrences newly materialized (posted or queued).</returns>
    Task<int> PostDueOccurrencesAsync(DateOnly asOf, CancellationToken cancellationToken = default);
}
