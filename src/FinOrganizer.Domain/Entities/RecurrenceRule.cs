using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class RecurrenceRule : Entity
{
    public required string Name { get; set; }

    public Guid AccountId { get; set; }

    public Guid CategoryId { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>Used for Monthly/Yearly frequencies.</summary>
    public int? DayOfMonth { get; set; }

    /// <summary>Used for Weekly frequency.</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>Step size in units of <see cref="Frequency"/> for Custom frequency (e.g. every 2 weeks).</summary>
    public int Interval { get; set; } = 1;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly NextDueDate { get; set; }

    public bool AutoPost { get; set; }

    public bool IsActive { get; set; } = true;
}
