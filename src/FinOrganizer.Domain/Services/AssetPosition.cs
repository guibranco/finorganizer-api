namespace FinOrganizer.Domain.Services;

public sealed record AssetPosition(
    Guid AssetId,
    decimal Quantity,
    decimal AverageCost,
    decimal TotalInvested,
    decimal RealizedPnL,
    decimal IncomeReceived)
{
    public decimal MarketValue(decimal marketPrice) => Quantity * marketPrice;

    public decimal UnrealizedPnL(decimal marketPrice) => Quantity * (marketPrice - AverageCost);
}
