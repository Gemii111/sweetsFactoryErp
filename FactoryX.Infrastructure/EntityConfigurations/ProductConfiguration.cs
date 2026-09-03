using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class ProductConfiguration : EntityBaseConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("products");

        // Unique indexes
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.SKU)
            .IsUnique();

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.HasIndex(p => p.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL AND [Barcode] <> ''");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.ArabicName)
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Unit)
            .HasMaxLength(50);

        builder.Property(p => p.WeightUnit)
            .HasMaxLength(50);

        builder.Property(p => p.ExpiryUnit)
            .HasMaxLength(50);

        // Precision
        builder.Property(p => p.Weight)
            .HasPrecision(18, 2);

        builder.Property(p => p.UnitWeightKg)
            .HasPrecision(18, 2);

        builder.Property(p => p.SellingPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.WholesalePrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.DistributorPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.StandardCost)
            .HasPrecision(18, 2);

        builder.Property(p => p.MinimumStock)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(p => p.ProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.WorkOrders)
            .WithOne(w => w.Product)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}