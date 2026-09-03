using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(soi => soi.Id);

        builder.Property(soi => soi.OrderedQuantity)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.FulfilledQuantity)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.Unit)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(soi => soi.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.TotalPrice)
            .HasPrecision(18, 4);

        builder.Property(soi => soi.BatchNumber)
            .HasMaxLength(100);

        builder.Property(soi => soi.Notes)
            .HasMaxLength(500);

        builder.HasOne(soi => soi.Product)
            .WithMany()
            .HasForeignKey(soi => soi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(soi => soi.FulfillmentItems)
            .WithOne(sfi => sfi.SalesOrderItem)
            .HasForeignKey(sfi => sfi.SalesOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
