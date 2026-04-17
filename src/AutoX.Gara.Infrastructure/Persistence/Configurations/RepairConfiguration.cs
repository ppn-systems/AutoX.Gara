using System;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoX.Gara.Infrastructure.Persistence.Configurations;

public class RepairOrderConfiguration : IEntityTypeConfiguration<RepairOrder>
{
    public void Configure(EntityTypeBuilder<RepairOrder> builder)
    {
        builder.HasMany(ro => ro.Tasks)
               .WithOne(rt => rt.RepairOrder)
               .HasForeignKey(rt => rt.RepairOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ro => ro.Parts)
               .WithOne(rsp => rsp.RepairOrder)
               .HasForeignKey(rsp => rsp.RepairOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ro => ro.Invoice)
               .WithMany(i => i.RepairOrders)
               .HasForeignKey(ro => ro.InvoiceId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ro => ro.VehicleId);
        builder.HasIndex(ro => ro.InvoiceId).IsUnique();
        builder.HasIndex(ro => ro.CustomerId);
    }
}

public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.HasOne(rt => rt.ServiceItem)
               .WithMany()
               .HasForeignKey(rt => rt.ServiceItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rt => rt.Employee)
               .WithMany()
               .HasForeignKey(rt => rt.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rt => rt.Status);
        builder.HasIndex(rt => new { rt.StartDate, rt.CompletionDate });
        builder.HasIndex(rt => rt.EmployeeId);
    }
}

public class RepairOrderItemConfiguration : IEntityTypeConfiguration<RepairOrderItem>
{
    public void Configure(EntityTypeBuilder<RepairOrderItem> builder)
    {
        builder.HasOne(rsp => rsp.SparePart)
               .WithMany()
               .HasForeignKey(rsp => rsp.PartId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rsp => rsp.RepairOrderId);
        builder.HasIndex(rsp => rsp.PartId);
    }
}
