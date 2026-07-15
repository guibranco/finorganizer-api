using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Accounts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    decimal InitialBalance,
    decimal CurrentBalance,
    bool IsArchived,
    DateTime CreatedAtUtc);

public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency, decimal InitialBalance);

public sealed record UpdateAccountRequest(string Name, AccountType Type, string Currency, bool IsArchived);
