using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class QualityInspectionConfiguration : IEntityTypeConfiguration<QualityInspection>
{
    public void Configure(EntityTypeBuilder<QualityInspection> builder)
    {
        builder.ToTable("QualityInspections");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.InspectionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(q => q.InspectionNumber)
            .IsUnique();

        builder.Property(q => q.Notes)
            .HasMaxLength(1000);

        builder.Property(q => q.ApprovalNotes)
            .HasMaxLength(1000);

        builder.Property(q => q.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(q => q.HoldReason)
            .HasMaxLength(1000);

        builder.Property(q => q.ReinspectionReason)
            .HasMaxLength(500);

        builder.HasOne(q => q.ProductionBatch)
            .WithMany(b => b.QualityInspections)
            .HasForeignKey(q => q.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.WorkOrder)
            .WithMany()
            .HasForeignKey(q => q.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Product)
            .WithMany()
            .HasForeignKey(q => q.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Material)
            .WithMany()
            .HasForeignKey(q => q.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Supplier)
            .WithMany()
            .HasForeignKey(q => q.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.QualityTemplate)
            .WithMany(t => t.Inspections)
            .HasForeignKey(q => q.QualityTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Inspector)
            .WithMany()
            .HasForeignKey(q => q.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.CreatedByUser)
            .WithMany()
            .HasForeignKey(q => q.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.SubmittedByUser)
            .WithMany()
            .HasForeignKey(q => q.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.CompletedByUser)
            .WithMany()
            .HasForeignKey(q => q.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.DecisionByUser)
            .WithMany()
            .HasForeignKey(q => q.DecisionByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.PreviousInspection)
            .WithMany(p => p.Reinspections)
            .HasForeignKey(q => q.PreviousInspectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Items)
            .WithOne(i => i.QualityInspection)
            .HasForeignKey(i => i.QualityInspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
