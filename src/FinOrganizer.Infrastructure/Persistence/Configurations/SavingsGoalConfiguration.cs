using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinOrganizer.Infrastructure.Persistence.Configurations;

public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.TargetAmount).HasPrecision(18, 2);

        builder.HasOne<Account>().WithMany().HasForeignKey(g => g.LinkedAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SavingsGoalContributionConfiguration : IEntityTypeConfiguration<SavingsGoalContribution>
{
    public void Configure(EntityTypeBuilder<SavingsGoalContribution> builder)
    {
        builder.Property(c => c.Amount).HasPrecision(18, 2);
        builder.Property(c => c.Note).HasMaxLength(300);

        builder.HasOne<SavingsGoal>().WithMany().HasForeignKey(c => c.SavingsGoalId).OnDelete(DeleteBehavior.Cascade);
    }
}
