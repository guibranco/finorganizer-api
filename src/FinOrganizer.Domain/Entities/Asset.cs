using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class Asset : Entity
{
    public required string Ticker { get; set; }

    public required string Name { get; set; }

    public AssetClass Class { get; set; }

    public required string Currency { get; set; }

    public string? Exchange { get; set; }
}
