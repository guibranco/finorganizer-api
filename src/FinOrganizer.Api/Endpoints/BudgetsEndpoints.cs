using FinOrganizer.Api.Common;
using FinOrganizer.Application.Budgets;

namespace FinOrganizer.Api.Endpoints;

public static class BudgetsEndpoints
{
    public static void MapBudgetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/budgets").WithTags("Budgets");

        group.MapGet("/", async (DateOnly? month, IBudgetsService service, CancellationToken ct)
                => Results.Ok(await service.GetAllAsync(month, ct)))
            .Produces<List<BudgetDto>>();

        group.MapGet("/{id:guid}", async (Guid id, IBudgetsService service, CancellationToken ct)
                => Results.Ok(await service.GetByIdAsync(id, ct)))
            .Produces<BudgetDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateBudgetRequest request, IBudgetsService service, CancellationToken ct) =>
            {
                var budget = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/budgets/{budget.Id}", budget);
            })
            .WithRequestValidation<CreateBudgetRequest>()
            .Produces<BudgetDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateBudgetRequest request, IBudgetsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateBudgetRequest>()
            .Produces<BudgetDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IBudgetsService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/vs-actual", async (DateOnly month, IBudgetsService service, CancellationToken ct)
                => Results.Ok(await service.GetBudgetVsActualAsync(month, ct)))
            .Produces<List<BudgetVsActualDto>>();
    }
}
