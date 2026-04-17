using System;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Dinh nghia abstraction cho DbContext cua he thong gara AutoX.
/// </summary>
public interface IAutoXDbContext
{
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeSalary> EmployeeSalaries { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Part> Parts { get; }
    DbSet<RepairTask> RepairTasks { get; }
    DbSet<ServiceItem> ServiceItems { get; }
    DbSet<RepairOrder> RepairOrders { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<RepairOrderItem> RepairOrderItems { get; }
    DbSet<SupplierContactPhone> SupplierContactPhones { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
