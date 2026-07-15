using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Assets;

public sealed record AssetDto(Guid Id, string Ticker, string Name, AssetClass Class, string Currency, string? Exchange);

public sealed record CreateAssetRequest(string Ticker, string Name, AssetClass Class, string Currency, string? Exchange);

public sealed record UpdateAssetRequest(string Ticker, string Name, AssetClass Class, string Currency, string? Exchange);

public sealed record AssetEventDto(
    Guid Id,
    Guid AssetId,
    Guid AccountId,
    AssetEventType Type,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    DateOnly Date,
    string? Notes);

public sealed record CreateAssetEventRequest(
    Guid AssetId,
    Guid AccountId,
    AssetEventType Type,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    DateOnly Date,
    string? Notes);

public sealed record UpdateAssetEventRequest(
    Guid AssetId,
    Guid AccountId,
    AssetEventType Type,
    decimal Quantity,
    decimal UnitPrice,
    decimal Fees,
    DateOnly Date,
    string? Notes);

public sealed record AssetPositionDto(
    Guid AssetId,
    string Ticker,
    string Name,
    AssetClass Class,
    string Currency,
    decimal Quantity,
    decimal AverageCost,
    decimal TotalInvested,
    decimal RealizedPnL,
    decimal IncomeReceived,
    decimal? MarketPrice,
    decimal? MarketValue,
    decimal? UnrealizedPnL);

public sealed record AssetPriceSnapshotDto(Guid Id, Guid AssetId, DateOnly Date, decimal Price);

public sealed record UpsertAssetPriceSnapshotRequest(Guid AssetId, DateOnly Date, decimal Price);
