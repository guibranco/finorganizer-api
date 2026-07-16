using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinOrganizer.Infrastructure.Persistence.Configurations;

public class AssetEventConfiguration : IEntityTypeConfiguration<AssetEvent>
{
    public void Configure(EntityTypeBuilder<AssetEvent> builder)
    {
        builder.Property(e => e.Quantity).HasPrecision(18, 6);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 6);
        builder.Property(e => e.Fees).HasPrecision(18, 2);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => new { e.AssetId, e.Date });

        builder.HasOne<Asset>().WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>().WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
