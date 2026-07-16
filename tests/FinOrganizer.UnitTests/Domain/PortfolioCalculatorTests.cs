using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;

namespace FinOrganizer.UnitTests.Domain;

public class PortfolioCalculatorTests
{
    private static readonly Guid AssetId = Guid.NewGuid();

    private static AssetEvent Buy(decimal qty, decimal price, decimal fees, DateOnly date) => new()
    {
        AssetId = AssetId, Type = AssetEventType.Buy, Quantity = qty, UnitPrice = price, Fees = fees, Date = date,
    };

    private static AssetEvent Sell(decimal qty, decimal price, decimal fees, DateOnly date) => new()
    {
        AssetId = AssetId, Type = AssetEventType.Sell, Quantity = qty, UnitPrice = price, Fees = fees, Date = date,
    };

    private static AssetEvent Dividend(decimal amount, DateOnly date) => new()
    {
        AssetId = AssetId, Type = AssetEventType.Dividend, UnitPrice = amount, Date = date,
    };

    private static AssetEvent Split(decimal ratio, DateOnly date) => new()
    {
        AssetId = AssetId, Type = AssetEventType.Split, UnitPrice = ratio, Date = date,
    };

    [Fact]
    public void ComputePosition_TwoBuys_WeightsAverageCostByQuantity()
    {
        var events = new[]
        {
            Buy(10, 100, 10, new DateOnly(2026, 1, 1)), // cost basis 1010 / 10 = 101
            Buy(10, 110, 0, new DateOnly(2026, 2, 1)),  // (1010 + 1100) / 20 = 105.5
        };

        var position = PortfolioCalculator.ComputePosition(AssetId, events);

        Assert.Equal(20, position.Quantity);
        Assert.Equal(105.5m, position.AverageCost);
        Assert.Equal(2110m, position.TotalInvested);
        Assert.Equal(0, position.RealizedPnL);
    }

    [Fact]
    public void ComputePosition_SellAfterBuys_RealizesGainAtAverageCostAndKeepsAverageCostForRemainder()
    {
        var events = new[]
        {
            Buy(10, 100, 10, new DateOnly(2026, 1, 1)),
            Buy(10, 110, 0, new DateOnly(2026, 2, 1)),   // avg cost now 105.5, qty 20
            Sell(5, 130, 5, new DateOnly(2026, 3, 1)),   // realized = 5*(130-105.5) - 5 = 117.5
        };

        var position = PortfolioCalculator.ComputePosition(AssetId, events);

        Assert.Equal(15, position.Quantity);
        Assert.Equal(105.5m, position.AverageCost);
        Assert.Equal(117.5m, position.RealizedPnL);
        Assert.Equal(1582.5m, position.TotalInvested);
    }

    [Fact]
    public void ComputePosition_SellingEverything_ResetsAverageCostToZero()
    {
        var events = new[]
        {
            Buy(10, 100, 0, new DateOnly(2026, 1, 1)),
            Sell(10, 150, 0, new DateOnly(2026, 2, 1)),
        };

        var position = PortfolioCalculator.ComputePosition(AssetId, events);

        Assert.Equal(0, position.Quantity);
        Assert.Equal(0, position.AverageCost);
        Assert.Equal(500m, position.RealizedPnL);
    }

    [Fact]
    public void ComputePosition_DividendEvents_AccumulateIncomeWithoutAffectingQuantityOrCost()
    {
        var events = new[]
        {
            Buy(10, 100, 0, new DateOnly(2026, 1, 1)),
            Dividend(25, new DateOnly(2026, 2, 1)),
            Dividend(30, new DateOnly(2026, 3, 1)),
        };

        var position = PortfolioCalculator.ComputePosition(AssetId, events);

        Assert.Equal(10, position.Quantity);
        Assert.Equal(100, position.AverageCost);
        Assert.Equal(55m, position.IncomeReceived);
    }

    [Fact]
    public void ComputePosition_Split_ScalesQuantityUpAndAverageCostDownProportionally()
    {
        var events = new[]
        {
            Buy(10, 100, 0, new DateOnly(2026, 1, 1)), // qty 10, avg cost 100
            Split(2, new DateOnly(2026, 2, 1)),         // 2-for-1 split
        };

        var position = PortfolioCalculator.ComputePosition(AssetId, events);

        Assert.Equal(20, position.Quantity);
        Assert.Equal(50, position.AverageCost);
        Assert.Equal(1000m, position.TotalInvested); // cost basis preserved
    }

    [Fact]
    public void UnrealizedPnLAndMarketValue_UseCurrentQuantityAndAverageCost()
    {
        var position = PortfolioCalculator.ComputePosition(AssetId, [Buy(10, 100, 0, new DateOnly(2026, 1, 1))]);

        Assert.Equal(1500m, position.MarketValue(150));
        Assert.Equal(500m, position.UnrealizedPnL(150));
    }
}
