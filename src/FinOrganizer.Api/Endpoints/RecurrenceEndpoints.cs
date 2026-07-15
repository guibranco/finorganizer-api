using FinOrganizer.Api.Common;
using FinOrganizer.Application.Recurrence;

namespace FinOrganizer.Api.Endpoints;

public static class RecurrenceEndpoints
{
    public static void MapRecurrenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recurrence-rules").WithTags("Recurrence");

        group.MapGet("/", async (IRecurrenceRulesService service, CancellationToken ct)
            => Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IRecurrenceRulesService service, CancellationToken ct)
            => Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (CreateRecurrenceRuleRequest request, IRecurrenceRulesService service, CancellationToken ct) =>
            {
                var rule = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/recurrence-rules/{rule.Id}", rule);
            })
            .WithRequestValidation<CreateRecurrenceRuleRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateRecurrenceRuleRequest request, IRecurrenceRulesService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateRecurrenceRuleRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, IRecurrenceRulesService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/pending-occurrences", async (IRecurrenceRulesService service, CancellationToken ct)
            => Results.Ok(await service.GetPendingOccurrencesAsync(ct)));

        group.MapPost("/occurrences/{occurrenceId:guid}/confirm", async (Guid occurrenceId, IRecurrenceRulesService service, CancellationToken ct)
            => Results.Ok(await service.ConfirmOccurrenceAsync(occurrenceId, ct)));

        group.MapPost("/occurrences/{occurrenceId:guid}/skip", async (Guid occurrenceId, IRecurrenceRulesService service, CancellationToken ct)
            => Results.Ok(await service.SkipOccurrenceAsync(occurrenceId, ct)));
    }
}
