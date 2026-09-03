using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class SalesFulfillmentItemConfiguration : IEntityTypeConfiguration<SalesFulfillmentItem>
{
    public void Configure(EntityTypeBuilder<SalesFulfillmentItem> builder)
    {
        builder.ToTable("SalesFulfillmentItems");
        builder.HasKey(sfi => sfi.Id);

        builder.Property(sfi => sfi.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sfi => sfi.OrderedQuantity)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.ShippedQuantity)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.Unit)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(sfi => sfi.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.TotalPrice)
            .HasPrecision(18, 4);

        builder.Property(sfi => sfi.Notes)
            .HasMaxLength(500);

        builder.HasOne(sfi => sfi.Product)
            .WithMany()
            .HasForeignKey(sfi => sfi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sfi => sfi.FinishedGoodsStock)
            .WithMany()
            .HasForeignKey(sfi => sfi.FinishedGoodsStockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sfi => sfi.Warehouse)
            .WithMany()
            .HasForeignKey(sfi => sfi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sfi => sfi.Location)
            .WithMany()
            .HasForeignKey(sfi => sfi.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sfi => sfi.InventoryTransaction)
            .WithMany()
            .HasForeignKey(sfi => sfi.InventoryTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
