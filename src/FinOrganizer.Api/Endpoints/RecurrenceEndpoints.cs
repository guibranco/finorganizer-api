using FinOrganizer.Api.Common;
using FinOrganizer.Application.Recurrence;

namespace FinOrganizer.Api.Endpoints;

public static class RecurrenceEndpoints
{
    public static void MapRecurrenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recurrence-rules").WithTags("Recurrence");

        group.MapGet("/", async (IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.GetAllAsync(ct)))
            .Produces<List<RecurrenceRuleDto>>();

        group.MapGet("/{id:guid}", async (Guid id, IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.GetByIdAsync(id, ct)))
            .Produces<RecurrenceRuleDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRecurrenceRuleRequest request, IRecurrenceRulesService service, CancellationToken ct) =>
            {
                var rule = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/recurrence-rules/{rule.Id}", rule);
            })
            .WithRequestValidation<CreateRecurrenceRuleRequest>()
            .Produces<RecurrenceRuleDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateRecurrenceRuleRequest request, IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateRecurrenceRuleRequest>()
            .Produces<RecurrenceRuleDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IRecurrenceRulesService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/pending-occurrences", async (IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.GetPendingOccurrencesAsync(ct)))
            .Produces<List<RecurrenceOccurrenceDto>>();

        group.MapPost("/occurrences/{occurrenceId:guid}/confirm", async (Guid occurrenceId, IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.ConfirmOccurrenceAsync(occurrenceId, ct)))
            .Produces<RecurrenceOccurrenceDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/occurrences/{occurrenceId:guid}/skip", async (Guid occurrenceId, IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.SkipOccurrenceAsync(occurrenceId, ct)))
            .Produces<RecurrenceOccurrenceDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
