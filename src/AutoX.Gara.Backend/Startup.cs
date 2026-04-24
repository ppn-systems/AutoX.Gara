using AutoX.Gara.Api.Handlers.Auth;
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
using Nalix.Common.Networking.Packets;
using Nalix.Framework.Extensions;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Logging.Options;
using Nalix.Logging.Sinks;
using Nalix.Network.Hosting;
using System;

namespace AutoX.Gara.Backend;

/// <summary>
/// Quan ly cau hinh khoi dong cho he thong Backend.
/// Tach biet logic khoi tao giup Program.cs chi tap trung vao luong thuc thi chinh.
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

        // 3. Build Network Application with Hosting API (v12.1.0)
        var packetRegistry = InstanceManager.Instance.GetExistingInstance<IPacketRegistry>()
            ?? throw new InvalidOperationException("IPacketRegistry is not registered. Ensure AppConfig.Register() is called before Startup.Configure().");

        return NetworkApplication.CreateBuilder()
            .ConfigureLogging(logger)
            .ConfigurePacketRegistry(packetRegistry)
            .ConfigureDispatch(options => options.WithLogging(logger))
            .AddHandlers<AccountHandler>()
            .AddTcp<AutoXProtocol>(57206)
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
