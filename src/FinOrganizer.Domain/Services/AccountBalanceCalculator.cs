using FinOrganizer.Domain.Entities;

namespace FinOrganizer.Domain.Services;

/// <summary>Computes an account's current balance. Never stored — always derived from its transactions.</summary>
public static class AccountBalanceCalculator
{
    /// <param name="transactions">
    /// Every transaction touching this account, either as owner or as a transfer counterparty.
    /// </param>
    public static decimal ComputeBalance(Account account, IEnumerable<Transaction> transactions)
        => account.InitialBalance + transactions.Sum(t => t.SignedAmount(account.Id));
}
