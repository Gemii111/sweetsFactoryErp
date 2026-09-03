using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PackagingBOMConfiguration : IEntityTypeConfiguration<PackagingBOM>
{
    public void Configure(EntityTypeBuilder<PackagingBOM> builder)
    {
        builder.ToTable("PackagingBOMs");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(b => b.Code)
            .IsUnique();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.PackUnit)
            .HasMaxLength(50);

        builder.Property(b => b.Unit)
            .HasMaxLength(50);

        builder.Property(b => b.PackSize)
            .HasPrecision(18, 4);

        builder.Property(b => b.PackSizeKg)
            .HasPrecision(18, 4);

        builder.Property(b => b.OutputProductQuantity)
            .HasPrecision(18, 4);

        builder.HasOne(b => b.Product)
            .WithMany()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Versions)
            .WithOne(v => v.PackagingBOM)
            .HasForeignKey(v => v.PackagingBOMId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
