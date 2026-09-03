using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("AccountingPeriods");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PeriodName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.PeriodName)
            .IsUnique();

        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);
        builder.HasIndex(p => p.Status);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasOne(p => p.ClosedByUser)
            .WithMany()
            .HasForeignKey(p => p.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.JournalEntries)
            .WithOne(j => j.AccountingPeriod)
            .HasForeignKey(j => j.AccountingPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
