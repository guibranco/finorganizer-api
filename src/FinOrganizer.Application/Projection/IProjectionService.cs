namespace FinOrganizer.Application.Projection;

public interface IProjectionService
{
    /// <summary>Projects cash flow, per-account balances, and net worth for each of the next N months from recurrence rules.</summary>
    Task<List<ProjectionMonthDto>> ProjectAsync(int monthsAhead, CancellationToken cancellationToken = default);
}
