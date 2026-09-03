using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class MaterialConfiguration : EntityBaseConfiguration<Material>
{
    public override void Configure(EntityTypeBuilder<Material> builder)
    {
        base.Configure(builder);

        builder.ToTable("materials");

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Code).IsUnique();

        builder.Property(m => m.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.SKU).IsUnique();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.ArabicName)
            .HasMaxLength(150);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Unit)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(m => m.PurchaseUnit)
            .HasMaxLength(30);

        builder.Property(m => m.ConversionFactor)
            .HasPrecision(18, 4)
            .HasDefaultValue(1.0m);

        builder.Property(m => m.StandardCost).HasPrecision(18, 2);
        builder.Property(m => m.CurrentCost).HasPrecision(18, 2);
        builder.Property(m => m.LastPurchaseCost).HasPrecision(18, 2);
        builder.Property(m => m.UnitCost).HasPrecision(18, 2);

        builder.Property(m => m.MinimumStock).HasPrecision(18, 2);
        builder.Property(m => m.ReorderLevel).HasPrecision(18, 2);
        builder.Property(m => m.MaximumStock).HasPrecision(18, 2);
        builder.Property(m => m.CurrentStock).HasPrecision(18, 2);

        builder.HasOne(m => m.MaterialCategory)
            .WithMany(c => c.Materials)
            .HasForeignKey(m => m.MaterialCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Warehouse)
            .WithMany()
            .HasForeignKey(m => m.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.MaterialUsages)
            .WithOne(mu => mu.Material)
            .HasForeignKey(mu => mu.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}