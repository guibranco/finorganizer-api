using FinOrganizer.Api.Common;
using FinOrganizer.Application.Assets;
using FinOrganizer.Application.Common.Interfaces;

namespace FinOrganizer.Api.Endpoints;

public static class AssetsEndpoints
{
    public static void MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/assets").WithTags("Assets");

        group.MapGet("/", async (IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.GetAllAsync(ct)))
            .Produces<List<AssetDto>>();

        group.MapGet("/{id:guid}", async (Guid id, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.GetByIdAsync(id, ct)))
            .Produces<AssetDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAssetRequest request, IAssetsService service, CancellationToken ct) =>
            {
                var asset = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/assets/{asset.Id}", asset);
            })
            .WithRequestValidation<CreateAssetRequest>()
            .Produces<AssetDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateAssetRequest request, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateAssetRequest>()
            .Produces<AssetDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IAssetsService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/positions", async (DateOnly? asOf, IAssetsService service, IDateTimeProvider clock, CancellationToken ct)
                => Results.Ok(await service.GetPositionsAsync(asOf ?? clock.Today, ct)))
            .Produces<List<AssetPositionDto>>();

        group.MapGet("/{id:guid}/position", async (Guid id, DateOnly? asOf, IAssetsService service, IDateTimeProvider clock, CancellationToken ct)
                => Results.Ok(await service.GetPositionAsync(id, asOf ?? clock.Today, ct)))
            .Produces<AssetPositionDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/prices", async (Guid id, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.GetPriceSnapshotsAsync(id, ct)))
            .Produces<List<AssetPriceSnapshotDto>>();

        group.MapPost("/prices", async (UpsertAssetPriceSnapshotRequest request, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.UpsertPriceSnapshotAsync(request, ct)))
            .WithRequestValidation<UpsertAssetPriceSnapshotRequest>()
            .Produces<AssetPriceSnapshotDto>()
            .ProducesValidationProblem();

        var events = app.MapGroup("/api/v1/asset-events").WithTags("Assets");

        events.MapGet("/", async (Guid? assetId, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.GetEventsAsync(assetId, ct)))
            .Produces<List<AssetEventDto>>();

        events.MapPost("/", async (CreateAssetEventRequest request, IAssetsService service, CancellationToken ct) =>
            {
                var assetEvent = await service.CreateEventAsync(request, ct);
                return Results.Created($"/api/v1/asset-events/{assetEvent.Id}", assetEvent);
            })
            .WithRequestValidation<CreateAssetEventRequest>()
            .Produces<AssetEventDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        events.MapPut("/{id:guid}", async (Guid id, UpdateAssetEventRequest request, IAssetsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateEventAsync(id, request, ct)))
            .WithRequestValidation<UpdateAssetEventRequest>()
            .Produces<AssetEventDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        events.MapDelete("/{id:guid}", async (Guid id, IAssetsService service, CancellationToken ct) =>
            {
                await service.DeleteEventAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
