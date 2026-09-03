using FactoryX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryX.Infrastructure.EntityConfigurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ArabicName)
            .HasMaxLength(200);

        builder.Property(c => c.ContactPerson)
            .HasMaxLength(150);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Mobile)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.TaxNumber)
            .HasMaxLength(50);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.CreditLimit)
            .HasPrecision(18, 2);

        builder.Property(c => c.CurrentBalance)
            .HasPrecision(18, 2);

        builder.HasMany(c => c.SalesOrders)
            .WithOne(so => so.Customer)
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.SalesFulfillments)
            .WithOne(sf => sf.Customer)
            .HasForeignKey(sf => sf.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
