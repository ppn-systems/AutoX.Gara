using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Application.Billings;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Application.Employees;
using AutoX.Gara.Application.Inventory;
using AutoX.Gara.Application.Invoices;
using AutoX.Gara.Application.Repairs;
using AutoX.Gara.Application.Suppliers;
using AutoX.Gara.Application.Vehicles;
using Microsoft.Extensions.DependencyInjection;

namespace AutoX.Gara.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký tất cả Application Services vào DI Container.
    /// Giúp Program.cs sạch sẽ và tuân thủ nguyên tắc Modular.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountAppService, AccountAppService>();
        services.AddScoped<IEmployeeAppService, EmployeeAppService>();
        services.AddScoped<IEmployeeSalaryAppService, EmployeeSalaryAppService>();
        services.AddScoped<ICustomerAppService, CustomerAppService>();
        services.AddScoped<IVehicleAppService, VehicleAppService>();
        services.AddScoped<IPartAppService, PartAppService>();
        services.AddScoped<ISupplierAppService, SupplierAppService>();
        services.AddScoped<IInvoiceAppService, InvoiceAppService>();
        services.AddScoped<IRepairOrderAppService, RepairOrderAppService>();
        services.AddScoped<ITransactionAppService, TransactionAppService>();
        services.AddScoped<IServiceItemAppService, ServiceItemAppService>();
        services.AddScoped<IRepairTaskAppService, RepairTaskAppService>();
        services.AddScoped<IRepairOrderItemAppService, RepairOrderItemAppService>();

        return services;
    }
}
