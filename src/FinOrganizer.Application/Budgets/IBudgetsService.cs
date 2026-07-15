namespace FinOrganizer.Application.Budgets;

public interface IBudgetsService
{
    Task<List<BudgetDto>> GetAllAsync(DateOnly? month, CancellationToken cancellationToken = default);

    Task<BudgetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BudgetDto> CreateAsync(CreateBudgetRequest request, CancellationToken cancellationToken = default);

    Task<BudgetDto> UpdateAsync(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<BudgetVsActualDto>> GetBudgetVsActualAsync(DateOnly month, CancellationToken cancellationToken = default);
}
