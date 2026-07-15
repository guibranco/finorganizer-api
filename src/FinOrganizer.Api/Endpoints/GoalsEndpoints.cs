using FinOrganizer.Api.Common;
using FinOrganizer.Application.Goals;

namespace FinOrganizer.Api.Endpoints;

public static class GoalsEndpoints
{
    public static void MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/savings-goals").WithTags("Goals");

        group.MapGet("/", async (ISavingsGoalsService service, CancellationToken ct)
            => Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, ISavingsGoalsService service, CancellationToken ct)
            => Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (CreateSavingsGoalRequest request, ISavingsGoalsService service, CancellationToken ct) =>
            {
                var goal = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/savings-goals/{goal.Id}", goal);
            })
            .WithRequestValidation<CreateSavingsGoalRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateSavingsGoalRequest request, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateSavingsGoalRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, ISavingsGoalsService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:guid}/contributions", async (Guid id, ISavingsGoalsService service, CancellationToken ct)
            => Results.Ok(await service.GetContributionsAsync(id, ct)));

        group.MapPost("/{id:guid}/contributions", async (Guid id, AddSavingsGoalContributionRequest request, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.AddContributionAsync(id, request, ct)))
            .WithRequestValidation<AddSavingsGoalContributionRequest>();
    }
}
