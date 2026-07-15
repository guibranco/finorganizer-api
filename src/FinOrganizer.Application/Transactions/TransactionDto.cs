using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Transactions;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateOnly Date,
    string? Description,
    Guid? CategoryId,
    Guid? CounterpartyAccountId,
    Guid? RecurrenceId,
    IReadOnlyList<string> Tags);

public sealed record CreateTransactionRequest(
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateOnly Date,
    string? Description,
    Guid? CategoryId,
    Guid? CounterpartyAccountId,
    IReadOnlyList<string>? Tags);

public sealed record UpdateTransactionRequest(
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateOnly Date,
    string? Description,
    Guid? CategoryId,
    Guid? CounterpartyAccountId,
    IReadOnlyList<string>? Tags);

public sealed record TransactionFilter(
    Guid? AccountId = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? SearchText = null,
    int Page = 1,
    int PageSize = 50);
