using FinOrganizer.Api.Common;
using FinOrganizer.Application.Goals;

namespace FinOrganizer.Api.Endpoints;

public static class GoalsEndpoints
{
    public static void MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/savings-goals").WithTags("Goals");

        group.MapGet("/", async (ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.GetAllAsync(ct)))
            .Produces<List<SavingsGoalDto>>();

        group.MapGet("/{id:guid}", async (Guid id, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.GetByIdAsync(id, ct)))
            .Produces<SavingsGoalDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateSavingsGoalRequest request, ISavingsGoalsService service, CancellationToken ct) =>
            {
                var goal = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/savings-goals/{goal.Id}", goal);
            })
            .WithRequestValidation<CreateSavingsGoalRequest>()
            .Produces<SavingsGoalDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateSavingsGoalRequest request, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateSavingsGoalRequest>()
            .Produces<SavingsGoalDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ISavingsGoalsService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/contributions", async (Guid id, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.GetContributionsAsync(id, ct)))
            .Produces<List<SavingsGoalContributionDto>>();

        group.MapPost("/{id:guid}/contributions", async (Guid id, AddSavingsGoalContributionRequest request, ISavingsGoalsService service, CancellationToken ct)
                => Results.Ok(await service.AddContributionAsync(id, request, ct)))
            .WithRequestValidation<AddSavingsGoalContributionRequest>()
            .Produces<SavingsGoalContributionDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
