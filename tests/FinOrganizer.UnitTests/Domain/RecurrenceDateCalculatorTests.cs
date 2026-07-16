using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;

namespace FinOrganizer.UnitTests.Domain;

public class RecurrenceDateCalculatorTests
{
    private static RecurrenceRule Rule(
        RecurrenceFrequency frequency, int interval = 1, int? dayOfMonth = null, DayOfWeek? dayOfWeek = null, DateOnly? startDate = null) => new()
    {
        Name = "Test", AccountId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), Currency = "USD",
        Frequency = frequency, Interval = interval, DayOfMonth = dayOfMonth, DayOfWeek = dayOfWeek,
        StartDate = startDate ?? new DateOnly(2026, 1, 1), NextDueDate = startDate ?? new DateOnly(2026, 1, 1),
    };

    [Fact]
    public void Weekly_NoDayOfWeek_AddsSevenDaysTimesInterval()
    {
        var rule = Rule(RecurrenceFrequency.Weekly, interval: 2);
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2026, 7, 1));

        Assert.Equal(new DateOnly(2026, 7, 15), next); // 2026-07-01 + 14 days
    }

    [Fact]
    public void Weekly_WithDayOfWeek_NudgesForwardToThatWeekday()
    {
        // 2026-07-01 is a Wednesday; +7 days lands on Wednesday 07-08, then nudges to Friday 07-10.
        var rule = Rule(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfWeek.Friday);
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2026, 7, 1));

        Assert.Equal(new DateOnly(2026, 7, 10), next);
        Assert.Equal(DayOfWeek.Friday, next.DayOfWeek);
    }

    [Fact]
    public void Monthly_ClampsDayOfMonthToShorterFollowingMonth()
    {
        // Jan 31 + 1 month with DayOfMonth=31 should clamp to Feb 28 in a non-leap year.
        var rule = Rule(RecurrenceFrequency.Monthly, dayOfMonth: 31);
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2026, 1, 31));

        Assert.Equal(new DateOnly(2026, 2, 28), next);
    }

    [Fact]
    public void Monthly_WithInterval_SkipsAheadByThatManyMonths()
    {
        var rule = Rule(RecurrenceFrequency.Monthly, interval: 3, dayOfMonth: 15);
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2026, 1, 15));

        Assert.Equal(new DateOnly(2026, 4, 15), next);
    }

    [Fact]
    public void Yearly_ClampsLeapDayToFeb28InNonLeapYear()
    {
        var rule = Rule(RecurrenceFrequency.Yearly, startDate: new DateOnly(2028, 2, 29));
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2028, 2, 29));

        Assert.Equal(new DateOnly(2029, 2, 28), next);
    }

    [Fact]
    public void Custom_AddsIntervalDays()
    {
        var rule = Rule(RecurrenceFrequency.Custom, interval: 10);
        var next = RecurrenceDateCalculator.GetNextDueDate(rule, new DateOnly(2026, 1, 1));

        Assert.Equal(new DateOnly(2026, 1, 11), next);
    }
}
