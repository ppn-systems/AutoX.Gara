using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Repositories;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using System.Threading.Tasks;
namespace AutoX.Gara.Infrastructure.Persistence;
public sealed class DataSession : IDataSession
{
    private readonly AutoXDbContext _context;
    private IAccountRepository _accounts = null!;
    private ICustomerRepository _customers = null!;
    private IEmployeeRepository _employees = null!;
    private IEmployeeSalaryRepository _employeeSalaries = null!;
    private IInvoiceRepository _invoices = null!;
    private IPartRepository _parts = null!;
    private IRepairOrderRepository _repairOrders = null!;
    private IRepairOrderItemRepository _repairOrderItems = null!;
    private IRepairTaskRepository _repairTasks = null!;
    private IServiceItemRepository _serviceItems = null!;
    private ISupplierRepository _suppliers = null!;
    private ITransactionRepository _transactions = null!;
    private IVehicleRepository _vehicles = null!;
    public DataSession(AutoXDbContext context)
    {
        _context = context;
    }
    public Microsoft.EntityFrameworkCore.DbContext Context => _context;
    public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
    public IEmployeeSalaryRepository EmployeeSalaries => _employeeSalaries ??= new EmployeeSalaryRepository(_context);
    public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
    public IPartRepository Parts => _parts ??= new PartRepository(_context);
    public IRepairOrderRepository RepairOrders => _repairOrders ??= new RepairOrderRepository(_context);
    public IRepairOrderItemRepository RepairOrderItems => _repairOrderItems ??= new RepairOrderItemRepository(_context);
    public IRepairTaskRepository RepairTasks => _repairTasks ??= new RepairTaskRepository(_context);
    public IServiceItemRepository ServiceItems => _serviceItems ??= new ServiceItemRepository(_context);
    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);
    public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);
    public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);
    public void ClearTracker() => _context.ChangeTracker.Clear();
    public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
    public async System.Threading.Tasks.Task<IDataSessionTransaction> BeginTransactionAsync(System.Threading.CancellationToken ct = default)
        => new DataSessionTransaction(await _context.Database.BeginTransactionAsync(ct));
    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
