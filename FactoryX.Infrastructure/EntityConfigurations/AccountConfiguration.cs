using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.AccountCode)
            .IsUnique();

        builder.Property(a => a.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AccountNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.HasOne(a => a.ParentAccount)
            .WithMany(a => a.ChildAccounts)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.JournalEntryLines)
            .WithOne(l => l.Account)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
