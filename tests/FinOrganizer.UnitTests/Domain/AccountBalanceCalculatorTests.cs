using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;

namespace FinOrganizer.UnitTests.Domain;

public class AccountBalanceCalculatorTests
{
    [Fact]
    public void ComputeBalance_CombinesInitialBalanceWithIncomeExpenseAndTransfersInBothDirections()
    {
        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 1000m };
        var otherAccountId = Guid.NewGuid();

        var transactions = new[]
        {
            new Transaction { AccountId = account.Id, Type = TransactionType.Income, Amount = 200m, Currency = "USD", Date = new DateOnly(2026, 1, 1) },
            new Transaction { AccountId = account.Id, Type = TransactionType.Expense, Amount = 50m, Currency = "USD", Date = new DateOnly(2026, 1, 2) },
            new Transaction { AccountId = account.Id, Type = TransactionType.Transfer, Amount = 100m, Currency = "USD", Date = new DateOnly(2026, 1, 3), CounterpartyAccountId = otherAccountId },
            new Transaction { AccountId = otherAccountId, Type = TransactionType.Transfer, Amount = 30m, Currency = "USD", Date = new DateOnly(2026, 1, 4), CounterpartyAccountId = account.Id },
        };

        var balance = AccountBalanceCalculator.ComputeBalance(account, transactions);

        Assert.Equal(1080m, balance); // 1000 + 200 - 50 - 100 + 30
    }

    [Fact]
    public void ComputeBalance_UnrelatedTransaction_DoesNotAffectBalance()
    {
        var account = new Account { Name = "Checking", Currency = "USD", InitialBalance = 500m };
        var unrelated = new Transaction { AccountId = Guid.NewGuid(), Type = TransactionType.Income, Amount = 999m, Currency = "USD", Date = new DateOnly(2026, 1, 1) };

        var balance = AccountBalanceCalculator.ComputeBalance(account, [unrelated]);

        Assert.Equal(500m, balance);
    }
}
