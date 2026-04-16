using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Api.Handlers.Auth;
using AutoX.Gara.Api.Handlers.Customers;
using AutoX.Gara.Api.Handlers.Financial;
using AutoX.Gara.Api.Handlers.Identity;
using AutoX.Gara.Api.Handlers.Inventory;
using AutoX.Gara.Api.Handlers.Repairs;
using AutoX.Gara.Api.Handlers.Suppliers;
using AutoX.Gara.Api.Handlers.Vehicles;
using AutoX.Gara.Api.Handlers.Financial;
using AutoX.Gara.Api.Handlers.Identity;
using AutoX.Gara.Api.Handlers.Inventory;
using AutoX.Gara.Api.Handlers.Repairs;
using AutoX.Gara.Api.Handlers.Suppliers;
using AutoX.Gara.Api.Handlers.Vehicles;
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
using AutoX.Gara.Shared;
using AutoX.Gara.Shared.Protocol.Auth;
using Microsoft.Extensions.Logging;
using Nalix.Common.Concurrency;
using Nalix.Common.Networking;
using Nalix.Framework.Configuration;
using Nalix.Framework.Extensions;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Buffers;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Options;
using Nalix.Framework.Tasks;
using Nalix.Framework.Time;
using Nalix.Logging;
using Nalix.Logging.Options;
using Nalix.Logging.Sinks;
using Nalix.Network.Connections;
using Nalix.Network.Hosting;
using Nalix.Network.Options;

[assembly: System.Reflection.AssemblyMetadata("Version", "1.0.0")]
[assembly: System.Reflection.AssemblyMetadata("Author", "PPN Corporation")]
[assembly: System.Runtime.CompilerServices.RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace AutoX.Gara.Backend;

[DebuggerStepThrough]
[ExcludeFromCodeCoverage]
public static class Program
{
    private const int IntervalInMinutes = 5;

    private static readonly System.Threading.ManualResetEvent QuitEvent = new(false);
    private static readonly TaskManager _taskManager = InstanceManager.Instance.GetOrCreateInstance<TaskManager>();
    private static NetworkApplication App;

    [STAThread]
    [DebuggerNonUserCode]
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void Main(string[] args)
    {
        try
        {
            InitializeComponent();
            App.ActivateAsync().GetAwaiter().GetResult();

            Console.CursorVisible = false;
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                QuitEvent.Set();
            };

            _taskManager.ScheduleWorker(
                "console.keyboard", "console",
                async (ctx, ct) => await LISTEN_TO_KEYBOARD(ctx, ct),
                new WorkerOptions { RetainFor = TimeSpan.FromMinutes(10) }
            );

            _taskManager.ScheduleWorker(
                "report.generator", "report",
                async (ctx, ct) => await GENERATE_PERIODIC_REPORTS(ctx, ct),
                new WorkerOptions { RetainFor = TimeSpan.FromMinutes(IntervalInMinutes) }
            );

            InstanceManager.Instance.GetExistingInstance<ILogger>().Info("Press 'Ctrl+R' to print reports.");
            InstanceManager.Instance.GetExistingInstance<ILogger>().Info("Server is running. Press Ctrl+C to exit.");

            QuitEvent.WaitOne();
            App.DeactivateAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?.Error("Unhandled exception in Main", ex);
            Environment.Exit(-1);
        }
        finally
        {
            Console.CursorVisible = false;
        }
    }

    public static void GenerateReport()
    {
        InstanceManager.Instance.GetExistingInstance<ILogger>().Info(InstanceManager.Instance.GenerateReport());
        ConnectionHub hub = (ConnectionHub)InstanceManager.Instance.GetExistingInstance<IConnectionHub>();
        InstanceManager.Instance.GetExistingInstance<ILogger>().Info(hub.GenerateReport());
        InstanceManager.Instance.GetExistingInstance<ILogger>().Info(InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>().GenerateReport());
        InstanceManager.Instance.GetExistingInstance<ILogger>().Info(InstanceManager.Instance.GetExistingInstance<ObjectPoolManager>().GenerateReport());
        InstanceManager.Instance.GetExistingInstance<ILogger>().Info(_taskManager.GenerateReport());
    }

    public static void InitializeComponent()
    {
        ConfigurationManager.Instance.Get<NLogixOptions>().MinLevel = LogLevel.Trace;
        ILogger logger = new NLogix(cfg => cfg.RegisterTarget(new BatchConsoleLogTarget(t => t.EnableColors = true)));
        InstanceManager.Instance.Register<ILogger>(logger);

        AppConfig.Register();

        AutoXDbContextFactory factory = new();
        InstanceManager.Instance.Register<AutoXDbContextFactory>(factory);
        InstanceManager.Instance.Register<IDataSessionFactory>(new DataSessionFactory(factory));

        using (AutoXDbContext context = factory.CreateDbContext())
        {
            if (context.Database.EnsureCreated())
            {
                DataSeeder.SeedAsync(context).Wait();
            }
        }

        var sessionFactory = InstanceManager.Instance.GetExistingInstance<IDataSessionFactory>();
        InstanceManager.Instance.Register<IAccountAppService>(new AccountAppService(sessionFactory, logger.Create<AccountAppService>()));
        InstanceManager.Instance.Register<IEmployeeAppService>(new EmployeeAppService(sessionFactory, logger.Create<EmployeeAppService>()));
        InstanceManager.Instance.Register<IEmployeeSalaryAppService>(new EmployeeSalaryAppService(sessionFactory, logger.Create<EmployeeSalaryAppService>()));
        InstanceManager.Instance.Register<ICustomerAppService>(new CustomerAppService(sessionFactory, logger.Create<CustomerAppService>()));
        InstanceManager.Instance.Register<IVehicleAppService>(new VehicleAppService(sessionFactory, logger.Create<VehicleAppService>()));
        InstanceManager.Instance.Register<IPartAppService>(new PartAppService(sessionFactory, logger.Create<PartAppService>()));
        InstanceManager.Instance.Register<ISupplierAppService>(new SupplierAppService(sessionFactory, logger.Create<SupplierAppService>()));
        InstanceManager.Instance.Register<IInvoiceAppService>(new InvoiceAppService(sessionFactory, logger.Create<InvoiceAppService>()));
        InstanceManager.Instance.Register<IRepairOrderAppService>(new RepairOrderAppService(sessionFactory, logger.Create<RepairOrderAppService>()));
        InstanceManager.Instance.Register<ITransactionAppService>(new TransactionAppService(sessionFactory, logger.Create<TransactionAppService>()));
        InstanceManager.Instance.Register<IServiceItemAppService>(new ServiceItemAppService(sessionFactory, logger.Create<ServiceItemAppService>()));
        InstanceManager.Instance.Register<IRepairTaskAppService>(new RepairTaskAppService(sessionFactory, logger.Create<RepairTaskAppService>()));
        InstanceManager.Instance.Register<IRepairOrderItemAppService>(new RepairOrderItemAppService(sessionFactory, logger.Create<RepairOrderItemAppService>()));

        App = NetworkApplication.CreateBuilder()
            .ConfigureLogging(logger)
            .ConfigureConnectionHub(new ConnectionHub(null, logger))
            .ConfigureBufferPoolManager(new BufferPoolManager(logger))
            .Configure<NetworkSocketOptions>(options => { options.Port = 57206; })
            .ConfigureDispatch(dispatchOptions =>
            {
                dispatchOptions.WithLogging(InstanceManager.Instance.GetExistingInstance<ILogger>());
                dispatchOptions.WithHandler(() => new AccountHandler(InstanceManager.Instance.GetExistingInstance<IAccountAppService>(), InstanceManager.Instance.GetExistingInstance<IDataSessionFactory>()));
                dispatchOptions.WithHandler(() => new EmployeeHandler(InstanceManager.Instance.GetExistingInstance<IEmployeeAppService>()));
                dispatchOptions.WithHandler(() => new EmployeeSalaryHandler(InstanceManager.Instance.GetExistingInstance<IEmployeeSalaryAppService>()));
                dispatchOptions.WithHandler(() => new CustomerHandler(InstanceManager.Instance.GetExistingInstance<ICustomerAppService>()));
                dispatchOptions.WithHandler(() => new VehicleHandler(InstanceManager.Instance.GetExistingInstance<IVehicleAppService>()));
                dispatchOptions.WithHandler(() => new PartHandler(InstanceManager.Instance.GetExistingInstance<IPartAppService>()));
                dispatchOptions.WithHandler(() => new SupplierHandler(InstanceManager.Instance.GetExistingInstance<ISupplierAppService>()));
                dispatchOptions.WithHandler(() => new InvoiceHandler(InstanceManager.Instance.GetExistingInstance<IInvoiceAppService>()));
                dispatchOptions.WithHandler(() => new RepairOrderHandler(InstanceManager.Instance.GetExistingInstance<IRepairOrderAppService>()));
                dispatchOptions.WithHandler(() => new TransactionHandler(InstanceManager.Instance.GetExistingInstance<ITransactionAppService>()));
                dispatchOptions.WithHandler(() => new ServiceItemHandler(InstanceManager.Instance.GetExistingInstance<IServiceItemAppService>()));
                dispatchOptions.WithHandler(() => new RepairTaskHandler(InstanceManager.Instance.GetExistingInstance<IRepairTaskAppService>()));
                dispatchOptions.WithHandler(() => new RepairOrderItemHandler(InstanceManager.Instance.GetExistingInstance<IRepairOrderItemAppService>()));
            })
            .AddTcp<AutoXProtocol>()
            .Build();
    }

    private static Task LISTEN_TO_KEYBOARD(IWorkerContext ctx, System.Threading.CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            const double TileCooldownSeconds = 1.0;
            const double ReportCooldownSeconds = 5.0;
            DateTime startTime = Clock.NowUtc();
            DateTime lastTileTime = DateTime.MinValue;
            DateTime lastReportTime = DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                DateTime now = Clock.NowUtc();
                if ((now - lastTileTime).TotalSeconds >= TileCooldownSeconds)
                {
                    TimeSpan runningTime = now - startTime;
                    string runningTimeString = string.Format("{0:D2}:{1:D2}:{2:D2}", runningTime.Hours, runningTime.Minutes, runningTime.Seconds);
                    Console.Title = "AutoX | " + _taskManager.Title + " | " + runningTimeString;
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.R && (key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        if ((now - lastReportTime).TotalSeconds >= ReportCooldownSeconds)
                        {
                            GenerateReport();
                            lastReportTime = now;
                            ctx.Advance(1);
                        }
                    }
                }
                ctx.Beat();
                await Task.Delay(100, ct);
            }
        }, ct);
    }

    private static async Task GENERATE_PERIODIC_REPORTS(IWorkerContext ctx, System.Threading.CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>().SaveReportToFile("buffer");
            InstanceManager.Instance.GetExistingInstance<ObjectPoolManager>().SaveReportToFile("object");
            _taskManager.SaveReportToFile("task");
            ctx.Beat();
            ctx.Advance(1);
            await Task.Delay(TimeSpan.FromMinutes(IntervalInMinutes), ct);
        }
    }
}

internal static class LoggerExtensions
{
    public static Microsoft.Extensions.Logging.ILogger<T> Create<T>(this Microsoft.Extensions.Logging.ILogger logger) => new LoggerWrapper<T>(logger);
    private class LoggerWrapper<T>(Microsoft.Extensions.Logging.ILogger logger) : Microsoft.Extensions.Logging.ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => logger.BeginScope(state);
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => logger.IsEnabled(logLevel);
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => logger.Log(logLevel, eventId, state, exception, formatter);
    }
}