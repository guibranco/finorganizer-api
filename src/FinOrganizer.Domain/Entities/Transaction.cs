using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class Transaction : Entity
{
    public Guid AccountId { get; set; }

    public TransactionType Type { get; set; }

    /// <summary>Always positive; the sign is derived from <see cref="Type"/> when computing balances.</summary>
    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public DateOnly Date { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? CounterpartyAccountId { get; set; }

    public Guid? RecurrenceId { get; set; }

    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Signed amount from <paramref name="perspectiveAccountId"/>'s point of view: positive for Income,
    /// negative for Expense (both only when that account owns the transaction), negative for an outgoing
    /// Transfer, positive for an incoming Transfer, and zero for any account this transaction doesn't touch.
    /// </summary>
    public decimal SignedAmount(Guid perspectiveAccountId) => Type switch
    {
        TransactionType.Income when perspectiveAccountId == AccountId => Amount,
        TransactionType.Expense when perspectiveAccountId == AccountId => -Amount,
        TransactionType.Transfer when perspectiveAccountId == AccountId => -Amount,
        TransactionType.Transfer when perspectiveAccountId == CounterpartyAccountId => Amount,
        _ => 0m,
    };
}
