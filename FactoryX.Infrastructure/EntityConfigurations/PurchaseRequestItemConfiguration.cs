using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseRequestItemConfiguration : IEntityTypeConfiguration<PurchaseRequestItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestItem> builder)
    {
        builder.ToTable("PurchaseRequestItems");
        builder.HasKey(pri => pri.Id);

        builder.Property(pri => pri.RequestedQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(pri => pri.EstimatedUnitPrice)
            .HasPrecision(18, 4);

        builder.Property(pri => pri.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pri => pri.Notes)
            .HasMaxLength(500);

        builder.HasOne(pri => pri.Material)
            .WithMany()
            .HasForeignKey(pri => pri.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
