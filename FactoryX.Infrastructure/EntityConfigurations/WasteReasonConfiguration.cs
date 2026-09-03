using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class WasteReasonConfiguration : IEntityTypeConfiguration<WasteReason>
{
    public void Configure(EntityTypeBuilder<WasteReason> builder)
    {
        builder.ToTable("WasteReasons");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Code)
            .IsUnique();

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
    }
}
