using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class OperatorConfiguration : EntityBaseConfiguration<Operator>
{
    public override void Configure(EntityTypeBuilder<Operator> builder)
    {
        base.Configure(builder);

        builder.ToTable("operators");

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasMany(o => o.ProductionRecords)
            .WithOne(pr => pr.Operator)
            .HasForeignKey(pr => pr.OperatorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}