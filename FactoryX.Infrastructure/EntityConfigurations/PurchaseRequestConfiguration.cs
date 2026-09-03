using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(pr => pr.RequestNumber)
            .IsUnique();

        builder.Property(pr => pr.Priority)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        builder.HasOne(pr => pr.Department)
            .WithMany()
            .HasForeignKey(pr => pr.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.RequestedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(pr => pr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pr => pr.Items)
            .WithOne(pri => pri.PurchaseRequest)
            .HasForeignKey(pri => pri.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.PurchaseOrders)
            .WithOne(po => po.PurchaseRequest)
            .HasForeignKey(po => po.PurchaseRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
