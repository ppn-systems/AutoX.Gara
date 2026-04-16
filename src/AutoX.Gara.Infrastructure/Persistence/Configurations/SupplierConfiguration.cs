using System;
using AutoX.Gara.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.TaxCode).IsUnique();
        builder.HasIndex(s => s.Status);

        builder.HasMany(s => s.PhoneNumbers)
               .WithOne(sp => sp.Supplier)
               .HasForeignKey(sp => sp.SupplierId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupplierContactPhoneConfiguration : IEntityTypeConfiguration<SupplierContactPhone>
{
    public void Configure(EntityTypeBuilder<SupplierContactPhone> builder)
    {
        builder.HasIndex(sp => new { sp.SupplierId, sp.PhoneNumber });
    }
}