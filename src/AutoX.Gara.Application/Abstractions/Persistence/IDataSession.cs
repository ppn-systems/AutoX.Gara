using AutoX.Gara.Application.Abstractions.Repositories;

namespace AutoX.Gara.Application.Abstractions.Persistence;

public interface IDataSession : System.IAsyncDisposable
{
    Microsoft.EntityFrameworkCore.DbContext Context { get; }
    IAccountRepository Accounts { get; }
    ICustomerRepository Customers { get; }
    IEmployeeRepository Employees { get; }
    IEmployeeSalaryRepository EmployeeSalaries { get; }
    IInvoiceRepository Invoices { get; }
    IPartRepository Parts { get; }
    IRepairOrderRepository RepairOrders { get; }
    IRepairOrderItemRepository RepairOrderItems { get; }
    IRepairTaskRepository RepairTasks { get; }
    IServiceItemRepository ServiceItems { get; }
    ISupplierRepository Suppliers { get; }
    ITransactionRepository Transactions { get; }
    IVehicleRepository Vehicles { get; }

    void ClearTracker();
    System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<IDataSessionTransaction> BeginTransactionAsync(System.Threading.CancellationToken ct = default);
}
