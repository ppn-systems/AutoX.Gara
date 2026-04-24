using AutoX.Gara.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Position);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate);

        builder.Property(e => e.Gender).HasConversion<byte>();
        builder.Property(e => e.Position).HasConversion<byte>();
        builder.Property(e => e.Status).HasConversion<byte>();
    }
}
