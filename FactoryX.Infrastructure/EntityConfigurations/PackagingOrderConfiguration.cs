using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PackagingOrderConfiguration : IEntityTypeConfiguration<PackagingOrder>
{
    public void Configure(EntityTypeBuilder<PackagingOrder> builder)
    {
        builder.ToTable("PackagingOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.PlannedQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.ActualQuantity)
            .HasPrecision(18, 4);

        builder.Property(o => o.TheoreticalMaxPacks)
            .HasPrecision(18, 4);

        builder.Property(o => o.PackagingMaterialCost)
            .HasPrecision(18, 4);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne(o => o.ProductionBatch)
            .WithMany()
            .HasForeignKey(o => o.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Product)
            .WithMany()
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.PackagingBOM)
            .WithMany()
            .HasForeignKey(o => o.PackagingBOMId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.PackagingBOMVersion)
            .WithMany()
            .HasForeignKey(o => o.PackagingBOMVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Operator)
            .WithMany()
            .HasForeignKey(o => o.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.CreatedByUser)
            .WithMany()
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.CompletedByUser)
            .WithMany()
            .HasForeignKey(o => o.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Consumptions)
            .WithOne(c => c.PackagingOrder)
            .HasForeignKey(c => c.PackagingOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
