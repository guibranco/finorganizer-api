using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinOrganizer.Infrastructure.Persistence.Configurations;

public class RecurrenceOccurrenceConfiguration : IEntityTypeConfiguration<RecurrenceOccurrence>
{
    public void Configure(EntityTypeBuilder<RecurrenceOccurrence> builder)
    {
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.Property(o => o.Amount).HasPrecision(18, 2);

        builder.HasIndex(o => new { o.RecurrenceRuleId, o.DueDate }).IsUnique();

        builder.HasOne<RecurrenceRule>().WithMany().HasForeignKey(o => o.RecurrenceRuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Transaction>().WithMany().HasForeignKey(o => o.PostedTransactionId).OnDelete(DeleteBehavior.SetNull);
    }
}
