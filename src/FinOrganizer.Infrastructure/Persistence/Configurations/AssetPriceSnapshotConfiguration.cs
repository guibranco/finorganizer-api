using FinOrganizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinOrganizer.Infrastructure.Persistence.Configurations;

public class AssetPriceSnapshotConfiguration : IEntityTypeConfiguration<AssetPriceSnapshot>
{
    public void Configure(EntityTypeBuilder<AssetPriceSnapshot> builder)
    {
        builder.Property(s => s.Price).HasPrecision(18, 6);

        builder.HasIndex(s => new { s.AssetId, s.Date }).IsUnique();

        builder.HasOne<Asset>().WithMany().HasForeignKey(s => s.AssetId).OnDelete(DeleteBehavior.Cascade);
    }
}
