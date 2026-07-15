using FinOrganizer.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Infrastructure.Services;

/// <summary>
/// Resolves prices from manually entered <c>AssetPriceSnapshot</c> rows. Swap for an external
/// market-data implementation later without touching any consumer of <see cref="IPriceProvider"/>.
/// </summary>
public sealed class ManualPriceProvider(IApplicationDbContext db) : IPriceProvider
{
    public async Task<decimal?> GetPriceAsync(Guid assetId, DateOnly asOf, CancellationToken cancellationToken = default)
        => await db.AssetPriceSnapshots
            .Where(s => s.AssetId == assetId && s.Date <= asOf)
            .OrderByDescending(s => s.Date)
            .Select(s => (decimal?)s.Price)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetLatestPricesAsync(
        IReadOnlyCollection<Guid> assetIds, DateOnly asOf, CancellationToken cancellationToken = default)
    {
        if (assetIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var snapshots = await db.AssetPriceSnapshots
            .Where(s => assetIds.Contains(s.AssetId) && s.Date <= asOf)
            .ToListAsync(cancellationToken);

        return snapshots
            .GroupBy(s => s.AssetId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.Date).First().Price);
    }
}
