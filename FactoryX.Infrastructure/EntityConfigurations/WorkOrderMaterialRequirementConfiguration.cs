using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class WorkOrderMaterialRequirementConfiguration : EntityBaseConfiguration<WorkOrderMaterialRequirement>
{
    public override void Configure(EntityTypeBuilder<WorkOrderMaterialRequirement> builder)
    {
        base.Configure(builder);

        builder.ToTable("work_order_material_requirements");

        builder.Property(mr => mr.MaterialCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(mr => mr.MaterialName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(mr => mr.MaterialArabicName)
            .HasMaxLength(150);

        builder.Property(mr => mr.StockUnit)
            .HasMaxLength(30)
            .HasDefaultValue("KG");

        builder.Property(mr => mr.RecipeQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(mr => mr.ExpectedOutputQuantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(mr => mr.PlannedProductionQuantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(mr => mr.RequiredQuantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(mr => mr.AllocatedQuantity)
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(mr => mr.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(mr => mr.WorkOrder)
            .WithMany(w => w.MaterialRequirements)
            .HasForeignKey(mr => mr.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.Material)
            .WithMany()
            .HasForeignKey(mr => mr.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
