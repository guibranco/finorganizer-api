using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<RecurrenceRule> RecurrenceRules => Set<RecurrenceRule>();

    public DbSet<RecurrenceOccurrence> RecurrenceOccurrences => Set<RecurrenceOccurrence>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<AssetEvent> AssetEvents => Set<AssetEvent>();

    public DbSet<AssetPriceSnapshot> AssetPriceSnapshots => Set<AssetPriceSnapshot>();

    public DbSet<Budget> Budgets => Set<Budget>();

    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    public DbSet<SavingsGoalContribution> SavingsGoalContributions => Set<SavingsGoalContribution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
