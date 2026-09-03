using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class WorkOrderConfiguration : EntityBaseConfiguration<WorkOrder>
{
    public override void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        base.Configure(builder);

        builder.ToTable("work_orders");

        builder.Property(w => w.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(w => w.OrderNumber)
            .IsUnique();

        builder.Property(w => w.PlannedQuantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.ActualQuantityDecimal)
            .HasPrecision(18, 2);

        builder.Property(w => w.OutputUnit)
            .HasMaxLength(30)
            .HasDefaultValue("KG");

        builder.Property(w => w.PlannedDate)
            .IsRequired();

        builder.Property(w => w.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.OrderStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(w => w.Product)
            .WithMany(p => p.WorkOrders)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Recipe)
            .WithMany()
            .HasForeignKey(w => w.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.RecipeVersion)
            .WithMany(rv => rv.WorkOrders)
            .HasForeignKey(w => w.RecipeVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ProductionArea)
            .WithMany()
            .HasForeignKey(w => w.ProductionAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ProductionLine)
            .WithMany()
            .HasForeignKey(w => w.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WorkCenter)
            .WithMany()
            .HasForeignKey(w => w.WorkCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Machine)
            .WithMany(m => m.WorkOrders)
            .HasForeignKey(w => w.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Operator)
            .WithMany()
            .HasForeignKey(w => w.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Shift)
            .WithMany()
            .HasForeignKey(w => w.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.MaterialRequirements)
            .WithOne(mr => mr.WorkOrder)
            .HasForeignKey(mr => mr.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.ProductionRecords)
            .WithOne(pr => pr.WorkOrder)
            .HasForeignKey(pr => pr.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.MaterialUsages)
            .WithOne(mu => mu.WorkOrder)
            .HasForeignKey(mu => mu.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}