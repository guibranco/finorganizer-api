namespace FinOrganizer.Application.Assets;

public interface IAssetsService
{
    Task<List<AssetDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default);

    Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<AssetEventDto>> GetEventsAsync(Guid? assetId, CancellationToken cancellationToken = default);

    Task<AssetEventDto> CreateEventAsync(CreateAssetEventRequest request, CancellationToken cancellationToken = default);

    Task<AssetEventDto> UpdateEventAsync(Guid id, UpdateAssetEventRequest request, CancellationToken cancellationToken = default);

    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<AssetPositionDto>> GetPositionsAsync(DateOnly asOf, CancellationToken cancellationToken = default);

    Task<AssetPositionDto> GetPositionAsync(Guid assetId, DateOnly asOf, CancellationToken cancellationToken = default);

    Task<List<AssetPriceSnapshotDto>> GetPriceSnapshotsAsync(Guid assetId, CancellationToken cancellationToken = default);

    Task<AssetPriceSnapshotDto> UpsertPriceSnapshotAsync(UpsertAssetPriceSnapshotRequest request, CancellationToken cancellationToken = default);
}
