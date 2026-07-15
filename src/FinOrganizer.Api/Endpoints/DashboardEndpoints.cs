using FinOrganizer.Application.Dashboard;
using FinOrganizer.Application.Projection;

namespace FinOrganizer.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard").WithTags("Dashboard");

        group.MapGet("/net-worth", async (int? months, IDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetNetWorthOverTimeAsync(months ?? 12, ct)));

        group.MapGet("/allocation", async (IDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetAllocationAsync(ct)));

        group.MapGet("/income-vs-expense", async (int? months, IDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetIncomeVsExpenseAsync(months ?? 12, ct)));

        group.MapGet("/passive-income", async (int? months, IDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetPassiveIncomeAsync(months ?? 12, ct)));

        group.MapGet("/top-expense-categories", async (
                DateOnly from, DateOnly to, int? top, IDashboardService service, CancellationToken ct)
            => Results.Ok(await service.GetTopExpenseCategoriesAsync(from, to, top ?? 10, ct)));

        group.MapGet("/projection", async (int? monthsAhead, IProjectionService service, CancellationToken ct)
            => Results.Ok(await service.ProjectAsync(monthsAhead ?? 12, ct)));
    }
}
