using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Services;

/// <summary>Computes the next due date for a <see cref="RecurrenceRule"/> strictly after a given date.</summary>
public static class RecurrenceDateCalculator
{
    public static DateOnly GetNextDueDate(RecurrenceRule rule, DateOnly after) => rule.Frequency switch
    {
        RecurrenceFrequency.Weekly => NextWeekly(rule, after),
        RecurrenceFrequency.Monthly => NextMonthly(rule, after),
        RecurrenceFrequency.Yearly => NextYearly(rule, after),
        RecurrenceFrequency.Custom => after.AddDays(Math.Max(1, rule.Interval)),
        _ => throw new ArgumentOutOfRangeException(nameof(rule), rule.Frequency, "Unsupported recurrence frequency."),
    };

    private static DateOnly NextWeekly(RecurrenceRule rule, DateOnly after)
    {
        var step = Math.Max(1, rule.Interval) * 7;
        var next = after.AddDays(step);

        if (rule.DayOfWeek is { } dayOfWeek)
        {
            for (var i = 0; i < 7 && next.DayOfWeek != dayOfWeek; i++)
            {
                next = next.AddDays(1);
            }
        }

        return next;
    }

    private static DateOnly NextMonthly(RecurrenceRule rule, DateOnly after)
    {
        var step = Math.Max(1, rule.Interval);
        var targetMonth = after.AddMonths(step);
        var day = Math.Min(rule.DayOfMonth ?? after.Day, DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));
        return new DateOnly(targetMonth.Year, targetMonth.Month, day);
    }

    private static DateOnly NextYearly(RecurrenceRule rule, DateOnly after)
    {
        var step = Math.Max(1, rule.Interval);
        var candidate = after;

        do
        {
            var year = candidate.Year + step;
            var day = Math.Min(rule.StartDate.Day, DateTime.DaysInMonth(year, rule.StartDate.Month));
            candidate = new DateOnly(year, rule.StartDate.Month, day);
        }
        while (candidate <= after);

        return candidate;
    }
}
