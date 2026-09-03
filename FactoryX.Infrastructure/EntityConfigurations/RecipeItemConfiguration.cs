using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class RecipeItemConfiguration : EntityBaseConfiguration<RecipeItem>
{
    public override void Configure(EntityTypeBuilder<RecipeItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("RecipeItems");

        builder.Property(i => i.Unit)
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(250);

        builder.Property(i => i.Quantity)
            .HasPrecision(18, 4);

        builder.Property(i => i.Percentage)
            .HasPrecision(18, 2);

        builder.Property(i => i.UnitCost)
            .HasPrecision(18, 2);

        builder.Property(i => i.TotalCost)
            .HasPrecision(18, 2);

        builder.HasOne(i => i.RecipeVersion)
            .WithMany(v => v.Items)
            .HasForeignKey(i => i.RecipeVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Material)
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
