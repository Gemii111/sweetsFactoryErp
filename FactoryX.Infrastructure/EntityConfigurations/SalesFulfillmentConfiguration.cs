using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class SalesFulfillmentConfiguration : IEntityTypeConfiguration<SalesFulfillment>
{
    public void Configure(EntityTypeBuilder<SalesFulfillment> builder)
    {
        builder.ToTable("SalesFulfillments");
        builder.HasKey(sf => sf.Id);

        builder.Property(sf => sf.FulfillmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(sf => sf.FulfillmentNumber)
            .IsUnique();

        builder.Property(sf => sf.TotalQuantity)
            .HasPrecision(18, 4);

        builder.Property(sf => sf.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(sf => sf.TotalPrice)
            .HasPrecision(18, 4);

        builder.Property(sf => sf.Notes)
            .HasMaxLength(1000);

        builder.HasOne(sf => sf.SalesOrder)
            .WithMany(so => so.Fulfillments)
            .HasForeignKey(sf => sf.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sf => sf.Customer)
            .WithMany(c => c.SalesFulfillments)
            .HasForeignKey(sf => sf.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sf => sf.Warehouse)
            .WithMany()
            .HasForeignKey(sf => sf.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sf => sf.ShippedByUser)
            .WithMany()
            .HasForeignKey(sf => sf.ShippedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sf => sf.Items)
            .WithOne(sfi => sfi.SalesFulfillment)
            .HasForeignKey(sfi => sfi.SalesFulfillmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
