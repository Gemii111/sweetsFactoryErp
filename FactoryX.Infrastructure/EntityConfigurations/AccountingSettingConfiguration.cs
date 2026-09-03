using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class AccountingSettingConfiguration : IEntityTypeConfiguration<AccountingSetting>
{
    public void Configure(EntityTypeBuilder<AccountingSetting> builder)
    {
        builder.ToTable("AccountingSettings");
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.Role)
            .IsUnique();

        builder.Property(s => s.Description)
            .HasMaxLength(250);

        builder.HasOne(s => s.Account)
            .WithMany()
            .HasForeignKey(s => s.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
