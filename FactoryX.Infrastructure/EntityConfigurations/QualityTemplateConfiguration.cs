using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class QualityTemplateConfiguration : IEntityTypeConfiguration<QualityTemplate>
{
    public void Configure(EntityTypeBuilder<QualityTemplate> builder)
    {
        builder.ToTable("QualityTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.HasOne(t => t.ProductCategory)
            .WithMany()
            .HasForeignKey(t => t.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(i => i.QualityTemplate)
            .HasForeignKey(i => i.QualityTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
