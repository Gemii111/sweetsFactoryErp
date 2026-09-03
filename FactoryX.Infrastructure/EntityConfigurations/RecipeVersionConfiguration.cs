using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class RecipeVersionConfiguration : EntityBaseConfiguration<RecipeVersion>
{
    public override void Configure(EntityTypeBuilder<RecipeVersion> builder)
    {
        base.Configure(builder);

        builder.ToTable("RecipeVersions");

        builder.Property(v => v.VersionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.VersionName)
            .HasMaxLength(150);

        builder.Property(v => v.OutputUnit)
            .HasMaxLength(50);

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.Property(v => v.ExpectedOutput)
            .HasPrecision(18, 2);

        builder.Property(v => v.ExpectedWastePercentage)
            .HasPrecision(18, 2);

        builder.Property(v => v.MaterialCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.PackagingCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.LaborCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.MachineCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.OverheadCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.TotalProductionCost)
            .HasPrecision(18, 2);

        builder.Property(v => v.CostPerKg)
            .HasPrecision(18, 2);

        builder.Property(v => v.CostPerPiece)
            .HasPrecision(18, 2);

        builder.HasOne(v => v.Recipe)
            .WithMany(r => r.Versions)
            .HasForeignKey(v => v.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Items)
            .WithOne(i => i.RecipeVersion)
            .HasForeignKey(i => i.RecipeVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
