using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Assets;

public sealed class AssetsService(IApplicationDbContext db, IPriceProvider priceProvider) : IAssetsService
{
    public async Task<List<AssetDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Assets.OrderBy(a => a.Ticker).Select(ToAssetDtoExpression).ToListAsync(cancellationToken);

    public async Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAssetAsync(id, cancellationToken));

    public async Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        var asset = new Asset
        {
            Ticker = request.Ticker,
            Name = request.Name,
            Class = request.Class,
            Currency = request.Currency,
            Exchange = request.Exchange,
        };

        db.Assets.Add(asset);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(asset);
    }

    public async Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default)
    {
        var asset = await FindAssetAsync(id, cancellationToken);
        asset.Ticker = request.Ticker;
        asset.Name = request.Name;
        asset.Class = request.Class;
        asset.Currency = request.Currency;
        asset.Exchange = request.Exchange;

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(asset);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await FindAssetAsync(id, cancellationToken);
        var hasEvents = await db.AssetEvents.AnyAsync(e => e.AssetId == id, cancellationToken);
        if (hasEvents)
        {
            throw new ValidationException($"Asset '{asset.Ticker}' has recorded events and cannot be deleted.");
        }

        db.Assets.Remove(asset);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AssetEventDto>> GetEventsAsync(Guid? assetId, CancellationToken cancellationToken = default)
    {
        var query = db.AssetEvents.AsQueryable();
        if (assetId is { } id)
        {
            query = query.Where(e => e.AssetId == id);
        }

        return await query.OrderByDescending(e => e.Date).Select(ToEventDtoExpression).ToListAsync(cancellationToken);
    }

    public async Task<AssetEventDto> CreateEventAsync(CreateAssetEventRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureReferencesExistAsync(request.AssetId, request.AccountId, cancellationToken);

        var assetEvent = new AssetEvent
        {
            AssetId = request.AssetId,
            AccountId = request.AccountId,
            Type = request.Type,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Fees = request.Fees,
            Date = request.Date,
            Notes = request.Notes,
        };

        db.AssetEvents.Add(assetEvent);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(assetEvent);
    }

    public async Task<AssetEventDto> UpdateEventAsync(Guid id, UpdateAssetEventRequest request, CancellationToken cancellationToken = default)
    {
        var assetEvent = await FindEventAsync(id, cancellationToken);
        await EnsureReferencesExistAsync(request.AssetId, request.AccountId, cancellationToken);

        assetEvent.AssetId = request.AssetId;
        assetEvent.AccountId = request.AccountId;
        assetEvent.Type = request.Type;
        assetEvent.Quantity = request.Quantity;
        assetEvent.UnitPrice = request.UnitPrice;
        assetEvent.Fees = request.Fees;
        assetEvent.Date = request.Date;
        assetEvent.Notes = request.Notes;

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(assetEvent);
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assetEvent = await FindEventAsync(id, cancellationToken);
        db.AssetEvents.Remove(assetEvent);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AssetPositionDto>> GetPositionsAsync(DateOnly asOf, CancellationToken cancellationToken = default)
    {
        var assets = await db.Assets.ToListAsync(cancellationToken);
        var events = await db.AssetEvents.Where(e => e.Date <= asOf).ToListAsync(cancellationToken);
        var prices = await priceProvider.GetLatestPricesAsync(assets.Select(a => a.Id).ToList(), asOf, cancellationToken);

        return assets
            .Select(asset => BuildPositionDto(asset, events.Where(e => e.AssetId == asset.Id), prices.GetValueOrDefault(asset.Id)))
            .Where(p => p.Quantity != 0 || p.RealizedPnL != 0 || p.IncomeReceived != 0)
            .ToList();
    }

    public async Task<AssetPositionDto> GetPositionAsync(Guid assetId, DateOnly asOf, CancellationToken cancellationToken = default)
    {
        var asset = await FindAssetAsync(assetId, cancellationToken);
        var events = await db.AssetEvents.Where(e => e.AssetId == assetId && e.Date <= asOf).ToListAsync(cancellationToken);
        var price = await priceProvider.GetPriceAsync(assetId, asOf, cancellationToken);
        return BuildPositionDto(asset, events, price);
    }

    public async Task<List<AssetPriceSnapshotDto>> GetPriceSnapshotsAsync(Guid assetId, CancellationToken cancellationToken = default)
        => await db.AssetPriceSnapshots
            .Where(s => s.AssetId == assetId)
            .OrderByDescending(s => s.Date)
            .Select(s => new AssetPriceSnapshotDto(s.Id, s.AssetId, s.Date, s.Price))
            .ToListAsync(cancellationToken);

    public async Task<AssetPriceSnapshotDto> UpsertPriceSnapshotAsync(UpsertAssetPriceSnapshotRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Assets.AnyAsync(a => a.Id == request.AssetId, cancellationToken))
        {
            throw new NotFoundException(nameof(Asset), request.AssetId);
        }

        var snapshot = await db.AssetPriceSnapshots.FirstOrDefaultAsync(
            s => s.AssetId == request.AssetId && s.Date == request.Date, cancellationToken);

        if (snapshot is null)
        {
            snapshot = new AssetPriceSnapshot { AssetId = request.AssetId, Date = request.Date, Price = request.Price };
            db.AssetPriceSnapshots.Add(snapshot);
        }
        else
        {
            snapshot.Price = request.Price;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new AssetPriceSnapshotDto(snapshot.Id, snapshot.AssetId, snapshot.Date, snapshot.Price);
    }

    private static AssetPositionDto BuildPositionDto(Asset asset, IEnumerable<AssetEvent> events, decimal? marketPrice)
    {
        var position = PortfolioCalculator.ComputePosition(asset.Id, events);
        return new AssetPositionDto(
            asset.Id, asset.Ticker, asset.Name, asset.Class, asset.Currency,
            position.Quantity, position.AverageCost, position.TotalInvested, position.RealizedPnL, position.IncomeReceived,
            marketPrice,
            marketPrice is { } price ? position.MarketValue(price) : null,
            marketPrice is { } p ? position.UnrealizedPnL(p) : null);
    }

    private async Task EnsureReferencesExistAsync(Guid assetId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!await db.Assets.AnyAsync(a => a.Id == assetId, cancellationToken))
        {
            throw new NotFoundException(nameof(Asset), assetId);
        }

        if (!await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), accountId);
        }
    }

    private async Task<Asset> FindAssetAsync(Guid id, CancellationToken cancellationToken)
        => await db.Assets.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(Asset), id);

    private async Task<AssetEvent> FindEventAsync(Guid id, CancellationToken cancellationToken)
        => await db.AssetEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(AssetEvent), id);

    private static readonly System.Linq.Expressions.Expression<Func<Asset, AssetDto>> ToAssetDtoExpression = a =>
        new AssetDto(a.Id, a.Ticker, a.Name, a.Class, a.Currency, a.Exchange);

    private static AssetDto ToDto(Asset a) => new(a.Id, a.Ticker, a.Name, a.Class, a.Currency, a.Exchange);

    private static readonly System.Linq.Expressions.Expression<Func<AssetEvent, AssetEventDto>> ToEventDtoExpression = e =>
        new AssetEventDto(e.Id, e.AssetId, e.AccountId, e.Type, e.Quantity, e.UnitPrice, e.Fees, e.Date, e.Notes);

    private static AssetEventDto ToDto(AssetEvent e) => new(e.Id, e.AssetId, e.AccountId, e.Type, e.Quantity, e.UnitPrice, e.Fees, e.Date, e.Notes);
}
