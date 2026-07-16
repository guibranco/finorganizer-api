namespace FinOrganizer.Application.Dashboard;

public interface IDashboardService
{
    Task<List<NetWorthPointDto>> GetNetWorthOverTimeAsync(int months, CancellationToken cancellationToken = default);

    Task<AllocationDto> GetAllocationAsync(CancellationToken cancellationToken = default);

    Task<List<IncomeVsExpenseMonthDto>> GetIncomeVsExpenseAsync(int months, CancellationToken cancellationToken = default);

    Task<PassiveIncomeSummaryDto> GetPassiveIncomeAsync(int months, CancellationToken cancellationToken = default);

    Task<List<TopExpenseCategoryDto>> GetTopExpenseCategoriesAsync(DateOnly from, DateOnly to, int top, CancellationToken cancellationToken = default);
}
