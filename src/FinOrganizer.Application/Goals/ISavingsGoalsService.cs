namespace FinOrganizer.Application.Goals;

public interface ISavingsGoalsService
{
    Task<List<SavingsGoalDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SavingsGoalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SavingsGoalDto> CreateAsync(CreateSavingsGoalRequest request, CancellationToken cancellationToken = default);

    Task<SavingsGoalDto> UpdateAsync(Guid id, UpdateSavingsGoalRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SavingsGoalContributionDto>> GetContributionsAsync(Guid goalId, CancellationToken cancellationToken = default);

    Task<SavingsGoalContributionDto> AddContributionAsync(Guid goalId, AddSavingsGoalContributionRequest request, CancellationToken cancellationToken = default);
}
