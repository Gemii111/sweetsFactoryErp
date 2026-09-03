using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(po => po.OrderNumber)
            .IsUnique();

        builder.Property(po => po.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(po => po.TotalBeforeTax)
            .HasPrecision(18, 4);

        builder.Property(po => po.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(po => po.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(po => po.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(po => po.Notes)
            .HasMaxLength(1000);

        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.ApprovedByUser)
            .WithMany()
            .HasForeignKey(po => po.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(poi => poi.PurchaseOrder)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.Receipts)
            .WithOne(pr => pr.PurchaseOrder)
            .HasForeignKey(pr => pr.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
