using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Application.Recurrence;

namespace FinOrganizer.Api.BackgroundServices;

/// <summary>Materializes due recurrence occurrences on startup and once every 24 hours thereafter.</summary>
public sealed class RecurrencePostingBackgroundService(
    IServiceScopeFactory scopeFactory, ILogger<RecurrencePostingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var postingService = scope.ServiceProvider.GetRequiredService<IRecurrencePostingService>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var materialized = await postingService.PostDueOccurrencesAsync(dateTimeProvider.Today, cancellationToken);
            logger.LogInformation("Recurrence posting run materialized {Count} occurrence(s).", materialized);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Recurrence posting run failed.");
        }
    }
}
