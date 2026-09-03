using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class RecipeConfiguration : EntityBaseConfiguration<Recipe>
{
    public override void Configure(EntityTypeBuilder<Recipe> builder)
    {
        base.Configure(builder);

        builder.ToTable("Recipes");

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Code)
            .IsUnique();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.ArabicName)
            .HasMaxLength(150);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.Unit)
            .HasMaxLength(50);

        builder.Property(r => r.BaseOutputQuantity)
            .HasPrecision(18, 2);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Recipes)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Versions)
            .WithOne(v => v.Recipe)
            .HasForeignKey(v => v.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
