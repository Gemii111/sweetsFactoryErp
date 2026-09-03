using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.OrderedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(poi => poi.ReceivedQuantity)
            .HasPrecision(18, 4);

        builder.Property(poi => poi.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(poi => poi.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(poi => poi.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(poi => poi.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(poi => poi.TotalPrice)
            .HasPrecision(18, 4);

        builder.Property(poi => poi.Notes)
            .HasMaxLength(500);

        builder.HasOne(poi => poi.Material)
            .WithMany()
            .HasForeignKey(poi => poi.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
