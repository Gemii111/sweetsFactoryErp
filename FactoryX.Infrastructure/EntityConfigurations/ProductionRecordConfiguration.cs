using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class ProductionRecordConfiguration : EntityBaseConfiguration<ProductionRecord>
{
    public override void Configure(EntityTypeBuilder<ProductionRecord> builder)
    {
        base.Configure(builder);

        builder.ToTable("production_records");

        builder.Property(pr => pr.QuantityProduced)
            .IsRequired();

        builder.Property(pr => pr.Timestamp)
            .IsRequired();

        builder.HasOne(pr => pr.WorkOrder)
            .WithMany(w => w.ProductionRecords)
            .HasForeignKey(pr => pr.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Operator)
            .WithMany(o => o.ProductionRecords)
            .HasForeignKey(pr => pr.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}