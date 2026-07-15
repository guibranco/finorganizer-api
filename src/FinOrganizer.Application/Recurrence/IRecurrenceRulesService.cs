namespace FinOrganizer.Application.Recurrence;

public interface IRecurrenceRulesService
{
    Task<List<RecurrenceRuleDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RecurrenceRuleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RecurrenceRuleDto> CreateAsync(CreateRecurrenceRuleRequest request, CancellationToken cancellationToken = default);

    Task<RecurrenceRuleDto> UpdateAsync(Guid id, UpdateRecurrenceRuleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<RecurrenceOccurrenceDto>> GetPendingOccurrencesAsync(CancellationToken cancellationToken = default);

    Task<RecurrenceOccurrenceDto> ConfirmOccurrenceAsync(Guid occurrenceId, CancellationToken cancellationToken = default);

    Task<RecurrenceOccurrenceDto> SkipOccurrenceAsync(Guid occurrenceId, CancellationToken cancellationToken = default);
}
