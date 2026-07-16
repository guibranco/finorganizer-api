namespace FinOrganizer.Application.Common.Interfaces;

/// <summary>
/// Resolves market prices for assets. The default implementation reads manually entered
/// <c>AssetPriceSnapshot</c> rows; a future implementation can call an external market data API
/// without changing any consumer of this interface.
/// </summary>
public interface IPriceProvider
{
    /// <summary>Latest known price at or before <paramref name="asOf"/>, or null if none is recorded.</summary>
    Task<decimal?> GetPriceAsync(Guid assetId, DateOnly asOf, CancellationToken cancellationToken = default);

    /// <summary>Latest known price per asset, for the given set of assets, at or before <paramref name="asOf"/>.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetLatestPricesAsync(
        IReadOnlyCollection<Guid> assetIds, DateOnly asOf, CancellationToken cancellationToken = default);
}
