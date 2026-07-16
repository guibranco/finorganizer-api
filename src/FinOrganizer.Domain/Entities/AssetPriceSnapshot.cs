using FinOrganizer.Domain.Common;

namespace FinOrganizer.Domain.Entities;

public class AssetPriceSnapshot : Entity
{
    public Guid AssetId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Price { get; set; }
}
