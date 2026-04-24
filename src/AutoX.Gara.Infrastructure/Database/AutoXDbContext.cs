using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoX.Gara.Infrastructure.Database;

/// <summary>
/// DbContext cho ung dung quan ly gara o to.
/// </summary>
public sealed class AutoXDbContext(DbContextOptions<AutoXDbContext> options) : DbContext(options), IAutoXDbContext
{
    #region Properties

    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<RepairTask> RepairTasks => Set<RepairTask>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RepairOrderItem> RepairOrderItems => Set<RepairOrderItem>();
    public DbSet<SupplierContactPhone> SupplierContactPhones => Set<SupplierContactPhone>();

    #endregion Properties

    #region Override

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());



        // Performance Monitoring

        var loggerFactory = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.LoggerFactory;
        if (loggerFactory != null)
        {
            optionsBuilder.AddInterceptors(new DbPerformanceInterceptor(loggerFactory.CreateLogger<DbPerformanceInterceptor>()));
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());



        ApplySoftDeleteFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.DeletedAt)),
                    Expression.Constant(null)
                );
                var lambda = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    #endregion Override
}
