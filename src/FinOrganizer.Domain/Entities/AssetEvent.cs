using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class AssetEvent : Entity
{
    public Guid AssetId { get; set; }

    /// <summary>The broker account this event settled in.</summary>
    public Guid AccountId { get; set; }

    public AssetEventType Type { get; set; }

    /// <summary>Shares/units involved. Not meaningful for income events (Dividend/Distribution/Interest).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Price per unit for Buy/Sell, or split ratio numerator for Split. Total cash amount for income events.</summary>
    public decimal UnitPrice { get; set; }

    public decimal Fees { get; set; }

    public DateOnly Date { get; set; }

    public string? Notes { get; set; }
}
