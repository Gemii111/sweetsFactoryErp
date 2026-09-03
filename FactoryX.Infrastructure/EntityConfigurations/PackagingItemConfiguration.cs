using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PackagingItemConfiguration : IEntityTypeConfiguration<PackagingItem>
{
    public void Configure(EntityTypeBuilder<PackagingItem> builder)
    {
        builder.ToTable("PackagingItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantityRequired)
            .HasPrecision(18, 4);

        builder.Property(i => i.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasOne(i => i.PackagingBOM)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.PackagingBOMId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PackagingBOMVersion)
            .WithMany(v => v.Items)
            .HasForeignKey(i => i.PackagingBOMVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Material)
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
