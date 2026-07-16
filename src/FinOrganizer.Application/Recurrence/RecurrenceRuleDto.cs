using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Recurrence;

public sealed record RecurrenceRuleDto(
    Guid Id,
    string Name,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    RecurrenceFrequency Frequency,
    int? DayOfMonth,
    DayOfWeek? DayOfWeek,
    int Interval,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly NextDueDate,
    bool AutoPost,
    bool IsActive);

public sealed record CreateRecurrenceRuleRequest(
    string Name,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    RecurrenceFrequency Frequency,
    int? DayOfMonth,
    DayOfWeek? DayOfWeek,
    int Interval,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool AutoPost);

public sealed record UpdateRecurrenceRuleRequest(
    string Name,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    RecurrenceFrequency Frequency,
    int? DayOfMonth,
    DayOfWeek? DayOfWeek,
    int Interval,
    DateOnly? EndDate,
    bool AutoPost,
    bool IsActive);

public sealed record RecurrenceOccurrenceDto(
    Guid Id,
    Guid RecurrenceRuleId,
    string RecurrenceRuleName,
    DateOnly DueDate,
    decimal Amount,
    string Currency,
    RecurrenceOccurrenceStatus Status);
