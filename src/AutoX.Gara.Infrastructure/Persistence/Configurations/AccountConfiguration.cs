using AutoX.Gara.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(a => a.Username).IsUnique();
        builder.Property(a => a.Role).HasConversion<byte>();
        builder.Property(a => a.Salt).HasColumnType("binary(64)");
        builder.Property(a => a.Hash).HasColumnType("binary(64)");
    }
}
