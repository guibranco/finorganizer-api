using FinOrganizer.Domain.Common;

namespace FinOrganizer.Domain.Entities;

public class Budget : Entity
{
    public Guid CategoryId { get; set; }

    /// <summary>Always the first day of the budgeted month.</summary>
    public DateOnly Month { get; set; }

    public decimal LimitAmount { get; set; }
}
