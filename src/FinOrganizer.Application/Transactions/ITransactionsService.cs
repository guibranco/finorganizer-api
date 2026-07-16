using FinOrganizer.Application.Common.Models;

namespace FinOrganizer.Application.Transactions;

public interface ITransactionsService
{
    Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionFilter filter, CancellationToken cancellationToken = default);

    Task<TransactionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);

    Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransactionImportSummary> ImportCsvAsync(ImportTransactionsRequest request, CancellationToken cancellationToken = default);
}
