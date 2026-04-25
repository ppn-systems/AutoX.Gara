using AutoX.Gara.Domain.Entities.Billings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AutoX.Gara.Infrastructure.Configurations;
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.Discount).HasPrecision(18, 2);
        builder.Property(i => i.Subtotal).HasPrecision(18, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.BalanceDue).HasPrecision(18, 2);
        builder.Property(i => i.ServiceSubtotal).HasPrecision(18, 2);
        builder.Property(i => i.PartsSubtotal).HasPrecision(18, 2);
        builder.Property(i => i.PaymentStatus).HasConversion<byte>();
        builder.Property(i => i.TaxRate).HasConversion<byte>();
        builder.Property(i => i.DiscountType).HasConversion<byte>();
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.InvoiceDate);
    }
}

