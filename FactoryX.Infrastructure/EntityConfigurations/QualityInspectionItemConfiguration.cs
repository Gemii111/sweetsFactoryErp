using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class QualityInspectionItemConfiguration : IEntityTypeConfiguration<QualityInspectionItem>
{
    public void Configure(EntityTypeBuilder<QualityInspectionItem> builder)
    {
        builder.ToTable("QualityInspectionItems");
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

        builder.Property(i => i.ActualTextValue)
            .HasMaxLength(500);

        builder.Property(i => i.ActualPassFailValue)
            .HasMaxLength(20);

        builder.Property(i => i.InspectorNotes)
            .HasMaxLength(500);

        builder.Property(i => i.MinValue)
            .HasPrecision(18, 4);

        builder.Property(i => i.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(i => i.TargetValue)
            .HasPrecision(18, 4);

        builder.Property(i => i.ActualNumericValue)
            .HasPrecision(18, 4);

        builder.HasOne(i => i.QualityInspection)
            .WithMany(q => q.Items)
            .HasForeignKey(i => i.QualityInspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.QualityTemplateItem)
            .WithMany()
            .HasForeignKey(i => i.QualityTemplateItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
