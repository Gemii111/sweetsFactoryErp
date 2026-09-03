using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.Description)
            .HasMaxLength(250);

        builder.Property(ii => ii.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ii => ii.Quantity)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.TaxRate)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.LineTotal)
            .HasPrecision(18, 4);

        builder.Property(ii => ii.Notes)
            .HasMaxLength(500);

        builder.HasIndex(ii => ii.InvoiceId);
        builder.HasIndex(ii => ii.ProductId);
        builder.HasIndex(ii => ii.SalesOrderItemId);
        builder.HasIndex(ii => ii.SalesFulfillmentItemId);

        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ii => ii.Product)
            .WithMany(p => p.InvoiceItems)
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.SalesOrderItem)
            .WithMany()
            .HasForeignKey(ii => ii.SalesOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ii => ii.SalesFulfillmentItem)
            .WithMany(sfi => sfi.InvoiceItems)
            .HasForeignKey(ii => ii.SalesFulfillmentItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
