using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Debit)
            .HasPrecision(18, 2);

        builder.Property(l => l.Credit)
            .HasPrecision(18, 2);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.ReferenceNumber)
            .HasMaxLength(100);

        builder.HasIndex(l => l.AccountId);
        builder.HasIndex(l => l.CustomerId);
        builder.HasIndex(l => l.SupplierId);
        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.MaterialId);

        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Supplier)
            .WithMany()
            .HasForeignKey(l => l.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Material)
            .WithMany()
            .HasForeignKey(l => l.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
