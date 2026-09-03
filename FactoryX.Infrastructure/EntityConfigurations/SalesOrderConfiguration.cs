using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(so => so.Id);

        builder.Property(so => so.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(so => so.OrderNumber)
            .IsUnique();

        builder.Property(so => so.SubTotal)
            .HasPrecision(18, 4);

        builder.Property(so => so.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(so => so.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(so => so.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(so => so.Notes)
            .HasMaxLength(1000);

        builder.HasOne(so => so.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.Warehouse)
            .WithMany()
            .HasForeignKey(so => so.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(so => so.ConfirmedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(so => so.Items)
            .WithOne(soi => soi.SalesOrder)
            .HasForeignKey(soi => soi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(so => so.Fulfillments)
            .WithOne(sf => sf.SalesOrder)
            .HasForeignKey(sf => sf.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
