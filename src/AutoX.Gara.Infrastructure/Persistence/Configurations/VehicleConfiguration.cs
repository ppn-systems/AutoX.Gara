using System;
using AutoX.Gara.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.Property(v => v.Brand).HasConversion<byte>();
        builder.Property(v => v.Type).HasConversion<byte>();
        builder.Property(v => v.Color).HasConversion<byte>();

        builder.HasIndex(v => v.LicensePlate).IsUnique();
        builder.HasIndex(v => v.CustomerId);
        builder.HasIndex(v => v.Brand);
        builder.HasIndex(v => new { v.Brand, v.Type, v.Color, v.Year });
    }
}
