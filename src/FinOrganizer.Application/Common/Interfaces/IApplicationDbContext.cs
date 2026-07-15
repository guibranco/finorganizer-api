using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }

    DbSet<Category> Categories { get; }

    DbSet<Transaction> Transactions { get; }

    DbSet<RecurrenceRule> RecurrenceRules { get; }

    DbSet<RecurrenceOccurrence> RecurrenceOccurrences { get; }

    DbSet<Asset> Assets { get; }

    DbSet<AssetEvent> AssetEvents { get; }

    DbSet<AssetPriceSnapshot> AssetPriceSnapshots { get; }

    DbSet<Budget> Budgets { get; }

    DbSet<SavingsGoal> SavingsGoals { get; }

    DbSet<SavingsGoalContribution> SavingsGoalContributions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
