using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseReceiptItemConfiguration : IEntityTypeConfiguration<PurchaseReceiptItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReceiptItem> builder)
    {
        builder.ToTable("PurchaseReceiptItems");
        builder.HasKey(pri => pri.Id);

        builder.Property(pri => pri.OrderedQuantity)
            .HasPrecision(18, 4);

        builder.Property(pri => pri.ReceivedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(pri => pri.AcceptedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(pri => pri.RejectedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(pri => pri.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pri => pri.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(pri => pri.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(pri => pri.SupplierBatchNumber)
            .HasMaxLength(100);

        builder.Property(pri => pri.InternalBatchNumber)
            .HasMaxLength(100);

        builder.Property(pri => pri.Notes)
            .HasMaxLength(500);

        builder.HasOne(pri => pri.PurchaseOrderItem)
            .WithMany(poi => poi.ReceiptItems)
            .HasForeignKey(pri => pri.PurchaseOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.Material)
            .WithMany()
            .HasForeignKey(pri => pri.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.Warehouse)
            .WithMany()
            .HasForeignKey(pri => pri.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.Location)
            .WithMany()
            .HasForeignKey(pri => pri.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.InventoryTransaction)
            .WithMany()
            .HasForeignKey(pri => pri.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
