using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class Account : Entity
{
    public required string Name { get; set; }

    public AccountType Type { get; set; }

    public required string Currency { get; set; }

    public decimal InitialBalance { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
