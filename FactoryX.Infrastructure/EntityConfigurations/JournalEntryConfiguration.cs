using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.JournalNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(j => j.JournalNumber)
            .IsUnique();

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(j => j.ReferenceDocumentNumber)
            .HasMaxLength(100);

        builder.Property(j => j.ReversalReason)
            .HasMaxLength(500);

        builder.Property(j => j.TotalDebit)
            .HasPrecision(18, 2);

        builder.Property(j => j.TotalCredit)
            .HasPrecision(18, 2);

        builder.HasIndex(j => j.EntryDate);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => new { j.ReferenceType, j.ReferenceId });

        builder.HasOne(j => j.AccountingPeriod)
            .WithMany(p => p.JournalEntries)
            .HasForeignKey(j => j.AccountingPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.ReversalOfJournalEntry)
            .WithMany()
            .HasForeignKey(j => j.ReversalOfJournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.CreatedByUser)
            .WithMany()
            .HasForeignKey(j => j.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.PostedByUser)
            .WithMany()
            .HasForeignKey(j => j.PostedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
