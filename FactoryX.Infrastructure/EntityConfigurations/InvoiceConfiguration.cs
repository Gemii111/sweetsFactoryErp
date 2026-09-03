using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("EGP");

        builder.Property(i => i.SubTotal)
            .HasPrecision(18, 4);

        builder.Property(i => i.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.TaxRate)
            .HasPrecision(18, 4);

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.PaidAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.RemainingAmount)
            .HasPrecision(18, 4);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedByName)
            .HasMaxLength(150);

        builder.Property(i => i.CancellationReason)
            .HasMaxLength(500);

        builder.Property(i => i.RowVersion)
            .IsRowVersion();

        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.SalesOrderId);
        builder.HasIndex(i => i.FulfillmentId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.InvoiceDate);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SalesOrder)
            .WithMany(so => so.Invoices)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SalesFulfillment)
            .WithMany(sf => sf.Invoices)
            .HasForeignKey(i => i.FulfillmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Items)
            .WithOne(item => item.Invoice)
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
