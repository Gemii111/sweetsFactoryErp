using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class PackagingBOMVersionConfiguration : IEntityTypeConfiguration<PackagingBOMVersion>
{
    public void Configure(EntityTypeBuilder<PackagingBOMVersion> builder)
    {
        builder.ToTable("PackagingBOMVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.HasOne(v => v.PackagingBOM)
            .WithMany(b => b.Versions)
            .HasForeignKey(v => v.PackagingBOMId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Items)
            .WithOne(i => i.PackagingBOMVersion)
            .HasForeignKey(i => i.PackagingBOMVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
