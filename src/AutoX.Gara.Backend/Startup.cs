using AutoX.Gara.Backend.Transport.Auth;
using AutoX.Gara.Backend.Transport.Customers;
using AutoX.Gara.Backend.Transport.Financial;
using AutoX.Gara.Backend.Transport.Identity;
using AutoX.Gara.Backend.Transport.Inventory;
using AutoX.Gara.Backend.Transport.Repairs;
using AutoX.Gara.Backend.Transport.Suppliers;
using AutoX.Gara.Backend.Transport.Vehicles;
using AutoX.Gara.Application.Abstractions.Persistence;
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
using Nalix.Framework.Injection;
using Nalix.Logging;
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
        var inst = InstanceManager.Instance;
        return NetworkApplication.CreateBuilder()
            .ConfigureLogging(logger)
            .ConfigurePacketRegistry(packetRegistry)
            .ConfigureDispatch(options => options.WithLogging(logger))
            .AddHandler<AccountHandler>(() => new AccountHandler(
                ResolveRequired<AccountAppService>(inst),
                ResolveRequired<IDataSessionFactory>(inst)))
            .AddHandler<CustomerHandler>(() => new CustomerHandler(
                ResolveRequired<CustomerAppService>(inst)))
            .AddHandler<EmployeeHandler>(() => new EmployeeHandler(
                ResolveRequired<EmployeeAppService>(inst)))
            .AddHandler<EmployeeSalaryHandler>(() => new EmployeeSalaryHandler(
                ResolveRequired<EmployeeSalaryAppService>(inst)))
            .AddHandler<PartHandler>(() => new PartHandler(
                ResolveRequired<PartAppService>(inst)))
            .AddHandler<SupplierHandler>(() => new SupplierHandler(
                ResolveRequired<SupplierAppService>(inst)))
            .AddHandler<VehicleHandler>(() => new VehicleHandler(
                ResolveRequired<VehicleAppService>(inst)))
            .AddHandler<InvoiceHandler>(() => new InvoiceHandler(
                ResolveRequired<InvoiceAppService>(inst)))
            .AddHandler<TransactionHandler>(() => new TransactionHandler(
                ResolveRequired<TransactionAppService>(inst)))
            .AddHandler<ServiceItemHandler>(() => new ServiceItemHandler(
                ResolveRequired<ServiceItemAppService>(inst)))
            .AddHandler<RepairOrderHandler>(() => new RepairOrderHandler(
                ResolveRequired<RepairOrderAppService>(inst)))
            .AddHandler<RepairOrderItemHandler>(() => new RepairOrderItemHandler(
                ResolveRequired<RepairOrderItemAppService>(inst)))
            .AddHandler<RepairTaskHandler>(() => new RepairTaskHandler(
                ResolveRequired<RepairTaskAppService>(inst)))
            .AddTcp<AutoXProtocol>(57206)
            .Build();
    }
    private static void RegisterServices(IDataSessionFactory factory, ILoggerFactory loggerFactory)
    {
        var inst = InstanceManager.Instance;
        inst.Register<AccountAppService>(new AccountAppService(factory, loggerFactory.CreateLogger<AccountAppService>()));
        inst.Register<EmployeeAppService>(new EmployeeAppService(factory, loggerFactory.CreateLogger<EmployeeAppService>()));
        inst.Register<EmployeeSalaryAppService>(new EmployeeSalaryAppService(factory, loggerFactory.CreateLogger<EmployeeSalaryAppService>()));
        inst.Register<CustomerAppService>(new CustomerAppService(factory, loggerFactory.CreateLogger<CustomerAppService>()));
        inst.Register<VehicleAppService>(new VehicleAppService(factory, loggerFactory.CreateLogger<VehicleAppService>()));
        inst.Register<PartAppService>(new PartAppService(factory, loggerFactory.CreateLogger<PartAppService>()));
        inst.Register<SupplierAppService>(new SupplierAppService(factory, loggerFactory.CreateLogger<SupplierAppService>()));
        inst.Register<InvoiceAppService>(new InvoiceAppService(factory, loggerFactory.CreateLogger<InvoiceAppService>()));
        inst.Register<RepairOrderAppService>(new RepairOrderAppService(factory, loggerFactory.CreateLogger<RepairOrderAppService>()));
        inst.Register<TransactionAppService>(new TransactionAppService(factory, loggerFactory.CreateLogger<TransactionAppService>()));
        inst.Register<ServiceItemAppService>(new ServiceItemAppService(factory, loggerFactory.CreateLogger<ServiceItemAppService>()));
        inst.Register<RepairTaskAppService>(new RepairTaskAppService(factory, loggerFactory.CreateLogger<RepairTaskAppService>()));
        inst.Register<RepairOrderItemAppService>(new RepairOrderItemAppService(factory, loggerFactory.CreateLogger<RepairOrderItemAppService>()));
    }
    public static ILogger CreateBootstrapLogger() => new NLogix(cfg => cfg.RegisterTarget(new BatchConsoleLogTarget(t => t.EnableColors = true)));
    private static T ResolveRequired<T>(InstanceManager inst)
        where T : class
        => inst.GetExistingInstance<T>()
        ?? throw new InvalidOperationException($"Service '{typeof(T).FullName}' is not registered in InstanceManager.");
}
