using System;
using AutoX.Gara.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Property(p => p.SellingPrice).HasPrecision(18, 2);

        builder.HasIndex(p => p.PartCode).IsUnique();
        builder.HasIndex(p => p.PartName);
        builder.HasIndex(p => p.Manufacturer);
        builder.HasIndex(p => p.SupplierId);
        
        builder.HasIndex(p => new { p.IsDiscontinued, p.IsDefective, p.InventoryQuantity });

        builder.HasOne(p => p.Supplier)
               .WithMany(s => s.Parts)
               .HasForeignKey(p => p.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
