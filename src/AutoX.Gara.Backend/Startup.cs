using AutoX.Gara.Api.Handlers.Auth;
using AutoX.Gara.Api.Handlers.Customers;
using AutoX.Gara.Api.Handlers.Financial;
using AutoX.Gara.Api.Handlers.Identity;
using AutoX.Gara.Api.Handlers.Inventory;
using AutoX.Gara.Api.Handlers.Repairs;
using AutoX.Gara.Api.Handlers.Suppliers;
using AutoX.Gara.Api.Handlers.Vehicles;
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Application.Billings;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Application.Employees;
using AutoX.Gara.Application.Inventory;
using AutoX.Gara.Application.Invoices;
using AutoX.Gara.Application.Repairs;
using AutoX.Gara.Application.Suppliers;
using AutoX.Gara.Application.Vehicles;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Networking;
using AutoX.Gara.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Nalix.Network.Connections;
using Nalix.Network.Hosting;
using Nalix.Network.Options;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Buffers;
using Nalix.Logging.Options;
using Nalix.Logging.Sinks;
using Nalix.Logging;
using Nalix.Framework.Extensions;
using System;

namespace AutoX.Gara.Backend;

/// <summary>
/// Quản lý cấu hình khởi động cho hệ thống Backend.
/// Tách biệt logic khởi tạo giúp Program.cs chỉ tập trung vào luồng thực thi chính.
/// </summary>
public static class Startup
{
    public static NetworkApplication Configure(ILogger logger)
    {
        // 1. Database & Persistence Layer
        var dbFactory = new AutoXDbContextFactory();
        var sessionFactory = new DataSessionFactory(dbFactory);
        
        InstanceManager.Instance.Register<AutoXDbContextFactory>(dbFactory);
        InstanceManager.Instance.Register<IDataSessionFactory>(sessionFactory);

        // Create a logger factory for typed loggers
        using var loggerFactory = LoggerFactory.Create(builder => { });

        // 2. Application Services Registration
        RegisterServices(sessionFactory, loggerFactory);

        // 3. Build Network Application with Middleware Pipeline
        return NetworkApplication.CreateBuilder()
            .ConfigureLogging(logger)
            .ConfigureConnectionHub(new ConnectionHub(null, logger))
            .ConfigureBufferPoolManager(new BufferPoolManager(logger))
            .Configure<NetworkSocketOptions>(options => { options.Port = 57206; })
            .ConfigureDispatch(options =>
            {
                options.WithLogging(logger);
                
                var inst = InstanceManager.Instance;
                options.WithHandler(() => new AccountHandler(inst.GetExistingInstance<IAccountAppService>(), inst.GetExistingInstance<IDataSessionFactory>()));
                options.WithHandler(() => new EmployeeHandler(inst.GetExistingInstance<IEmployeeAppService>()));
                options.WithHandler(() => new EmployeeSalaryHandler(inst.GetExistingInstance<IEmployeeSalaryAppService>()));
                options.WithHandler(() => new CustomerHandler(inst.GetExistingInstance<ICustomerAppService>()));
                options.WithHandler(() => new VehicleHandler(inst.GetExistingInstance<IVehicleAppService>()));
                options.WithHandler(() => new PartHandler(inst.GetExistingInstance<IPartAppService>()));
                options.WithHandler(() => new SupplierHandler(inst.GetExistingInstance<ISupplierAppService>()));
                options.WithHandler(() => new InvoiceHandler(inst.GetExistingInstance<IInvoiceAppService>()));
                options.WithHandler(() => new RepairOrderHandler(inst.GetExistingInstance<IRepairOrderAppService>()));
                options.WithHandler(() => new TransactionHandler(inst.GetExistingInstance<ITransactionAppService>()));
                options.WithHandler(() => new ServiceItemHandler(inst.GetExistingInstance<IServiceItemAppService>()));
                options.WithHandler(() => new RepairTaskHandler(inst.GetExistingInstance<IRepairTaskAppService>()));
                options.WithHandler(() => new RepairOrderItemHandler(inst.GetExistingInstance<IRepairOrderItemAppService>()));
            })
            .AddTcp<AutoXProtocol>()
            .Build();
    }

    private static void RegisterServices(IDataSessionFactory factory, ILoggerFactory loggerFactory)
    {
        var inst = InstanceManager.Instance;
        inst.Register<IAccountAppService>(new AccountAppService(factory, loggerFactory.CreateLogger<AccountAppService>()));
        inst.Register<IEmployeeAppService>(new EmployeeAppService(factory, loggerFactory.CreateLogger<EmployeeAppService>()));
        inst.Register<IEmployeeSalaryAppService>(new EmployeeSalaryAppService(factory, loggerFactory.CreateLogger<EmployeeSalaryAppService>()));
        inst.Register<ICustomerAppService>(new CustomerAppService(factory, loggerFactory.CreateLogger<CustomerAppService>()));
        inst.Register<IVehicleAppService>(new VehicleAppService(factory, loggerFactory.CreateLogger<VehicleAppService>()));
        inst.Register<IPartAppService>(new PartAppService(factory, loggerFactory.CreateLogger<PartAppService>()));
        inst.Register<ISupplierAppService>(new SupplierAppService(factory, loggerFactory.CreateLogger<SupplierAppService>()));
        inst.Register<IInvoiceAppService>(new InvoiceAppService(factory, loggerFactory.CreateLogger<InvoiceAppService>()));
        inst.Register<IRepairOrderAppService>(new RepairOrderAppService(factory, loggerFactory.CreateLogger<RepairOrderAppService>()));
        inst.Register<ITransactionAppService>(new TransactionAppService(factory, loggerFactory.CreateLogger<TransactionAppService>()));
        inst.Register<IServiceItemAppService>(new ServiceItemAppService(factory, loggerFactory.CreateLogger<ServiceItemAppService>()));
        inst.Register<IRepairTaskAppService>(new RepairTaskAppService(factory, loggerFactory.CreateLogger<RepairTaskAppService>()));
        inst.Register<IRepairOrderItemAppService>(new RepairOrderItemAppService(factory, loggerFactory.CreateLogger<RepairOrderItemAppService>()));
    }

    public static ILogger CreateBootstrapLogger()
    {
        return new NLogix(cfg => cfg.RegisterTarget(new BatchConsoleLogTarget(t => t.EnableColors = true)));
    }
}
