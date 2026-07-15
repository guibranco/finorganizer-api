namespace FinOrganizer.Application.Accounts;

public interface IAccountsService
{
    Task<List<AccountDto>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default);

    Task<AccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
