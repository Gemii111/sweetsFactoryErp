using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseReceiptConfiguration : IEntityTypeConfiguration<PurchaseReceipt>
{
    public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
    {
        builder.ToTable("PurchaseReceipts");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(pr => pr.ReceiptNumber)
            .IsUnique();

        builder.Property(pr => pr.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        builder.HasOne(pr => pr.PurchaseOrder)
            .WithMany(po => po.Receipts)
            .HasForeignKey(pr => pr.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Supplier)
            .WithMany(s => s.PurchaseReceipts)
            .HasForeignKey(pr => pr.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Warehouse)
            .WithMany()
            .HasForeignKey(pr => pr.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.ReceivedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pr => pr.Items)
            .WithOne(pri => pri.PurchaseReceipt)
            .HasForeignKey(pri => pri.PurchaseReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
