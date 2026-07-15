using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Services;

/// <summary>
/// Computes a held position (weighted-average cost, realized P/L, income received) for one asset
/// from its chronological events. Positions are never persisted — always derived on read.
/// </summary>
public static class PortfolioCalculator
{
    public static AssetPosition ComputePosition(Guid assetId, IEnumerable<AssetEvent> events)
    {
        var quantity = 0m;
        var averageCost = 0m;
        var realizedPnL = 0m;
        var incomeReceived = 0m;

        foreach (var e in events.OrderBy(e => e.Date).ThenBy(e => e.Id))
        {
            switch (e.Type)
            {
                case AssetEventType.Buy:
                    var totalCost = (quantity * averageCost) + (e.Quantity * e.UnitPrice) + e.Fees;
                    quantity += e.Quantity;
                    averageCost = quantity == 0m ? 0m : totalCost / quantity;
                    break;

                case AssetEventType.Sell:
                    var sellQuantity = Math.Min(e.Quantity, quantity);
                    realizedPnL += (sellQuantity * (e.UnitPrice - averageCost)) - e.Fees;
                    quantity -= sellQuantity;
                    if (quantity == 0m)
                    {
                        averageCost = 0m;
                    }

                    break;

                case AssetEventType.Split:
                    if (e.UnitPrice > 0m)
                    {
                        quantity *= e.UnitPrice;
                        averageCost /= e.UnitPrice;
                    }

                    break;

                case AssetEventType.Dividend:
                case AssetEventType.Distribution:
                case AssetEventType.Interest:
                    incomeReceived += e.UnitPrice;
                    break;
            }
        }

        var totalInvested = quantity * averageCost;
        return new AssetPosition(assetId, quantity, averageCost, totalInvested, realizedPnL, incomeReceived);
    }
}
