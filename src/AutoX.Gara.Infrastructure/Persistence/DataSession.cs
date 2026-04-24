using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Repositories;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Persistence;

public sealed class DataSession(AutoXDbContext context) : IDataSession
{
    private readonly AutoXDbContext _context = context;
    public Microsoft.EntityFrameworkCore.DbContext Context => _context;

    public IAccountRepository Accounts => new AccountRepository(_context);
    public ICustomerRepository Customers => new CustomerRepository(_context);
    public IEmployeeRepository Employees => new EmployeeRepository(_context);
    public IEmployeeSalaryRepository EmployeeSalaries => new EmployeeSalaryRepository(_context);
    public IInvoiceRepository Invoices => new InvoiceRepository(_context);
    public IPartRepository Parts => new PartRepository(_context);
    public IRepairOrderRepository RepairOrders => new RepairOrderRepository(_context);
    public IRepairOrderItemRepository RepairOrderItems => new RepairOrderItemRepository(_context);
    public IRepairTaskRepository RepairTasks => new RepairTaskRepository(_context);
    public IServiceItemRepository ServiceItems => new ServiceItemRepository(_context);
    public ISupplierRepository Suppliers => new SupplierRepository(_context);
    public ITransactionRepository Transactions => new TransactionRepository(_context);
    public IVehicleRepository Vehicles => new VehicleRepository(_context);

    public void ClearTracker() => _context.ChangeTracker.Clear();

    public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async System.Threading.Tasks.Task<IDataSessionTransaction> BeginTransactionAsync(System.Threading.CancellationToken ct = default)
        => new DataSessionTransaction(await _context.Database.BeginTransactionAsync(ct));

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
