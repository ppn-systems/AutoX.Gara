using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AutoX.Gara.Infrastructure.Configurations;
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.Type).HasConversion<byte>();
        builder.Property(t => t.PaymentMethod).HasConversion<byte>();
        builder.Property(t => t.Status).HasConversion<byte>();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.HasIndex(t => t.InvoiceId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.TransactionDate);
    }
}
public class EmployeeSalaryConfiguration : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.Property(es => es.Salary).HasPrecision(18, 2);
        builder.Property(es => es.SalaryUnit).HasPrecision(18, 2);
        builder.Property(es => es.SalaryType).HasConversion<byte>();
        builder.HasIndex(es => es.EmployeeId);
        builder.HasIndex(es => es.SalaryType);
        builder.HasIndex(es => new { es.EffectiveFrom, es.EffectiveTo });
    }
}
public class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> builder)
    {
        builder.Property(si => si.UnitPrice).HasPrecision(18, 2);
        builder.Property(si => si.Type).HasConversion<byte>();
        builder.HasIndex(si => si.Description);
        builder.HasIndex(si => si.Type);
    }
}

