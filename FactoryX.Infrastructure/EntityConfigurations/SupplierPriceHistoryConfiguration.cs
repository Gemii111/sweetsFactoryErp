using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class SupplierPriceHistoryConfiguration : IEntityTypeConfiguration<SupplierPriceHistory>
{
    public void Configure(EntityTypeBuilder<SupplierPriceHistory> builder)
    {
        builder.ToTable("SupplierPriceHistories");
        builder.HasKey(sph => sph.Id);

        builder.Property(sph => sph.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(sph => sph.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(sph => sph.Supplier)
            .WithMany(s => s.PriceHistories)
            .HasForeignKey(sph => sph.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sph => sph.Material)
            .WithMany()
            .HasForeignKey(sph => sph.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sph => sph.PurchaseOrder)
            .WithMany()
            .HasForeignKey(sph => sph.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sph => sph.PurchaseReceipt)
            .WithMany()
            .HasForeignKey(sph => sph.PurchaseReceiptId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
