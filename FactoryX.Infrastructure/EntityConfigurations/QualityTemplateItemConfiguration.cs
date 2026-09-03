using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class QualityTemplateItemConfiguration : IEntityTypeConfiguration<QualityTemplateItem>
{
    public void Configure(EntityTypeBuilder<QualityTemplateItem> builder)
    {
        builder.ToTable("QualityTemplateItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.SpecificationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasMaxLength(500);

        builder.Property(i => i.Unit)
            .HasMaxLength(20);

        builder.Property(i => i.AllowedTextValues)
            .HasMaxLength(500);

        builder.Property(i => i.MinValue)
            .HasPrecision(18, 4);

        builder.Property(i => i.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(i => i.TargetValue)
            .HasPrecision(18, 4);

        builder.HasOne(i => i.QualityTemplate)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.QualityTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
