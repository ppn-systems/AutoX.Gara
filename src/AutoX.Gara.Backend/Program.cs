// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Communication;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Application.Inventory;
using AutoX.Gara.Application.Suppliers;
using AutoX.Gara.Application.Vehicles;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Networking;
using AutoX.Gara.Shared;
using Nalix.Common.Concurrency;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Diagnostics.Enums;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Framework.Options;
using Nalix.Framework.Tasks;
using Nalix.Framework.Time;
using Nalix.Logging;
using Nalix.Logging.Configuration;
using Nalix.Logging.Sinks;
using Nalix.Network.Abstractions;
using Nalix.Network.Connections;
using Nalix.Network.Middleware.Inbound;
using Nalix.Network.Middleware.Outbound;
using Nalix.Network.Routing;
using Nalix.Shared.Extensions;
using Nalix.Shared.Memory.Pooling;

[assembly: System.Reflection.AssemblyMetadata("Version", "1.0.0")]
[assembly: System.Reflection.AssemblyMetadata("Author", "PPN Corporation")]
[assembly: System.Runtime.CompilerServices.RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace AutoX.Gara.Backend;

[System.Diagnostics.DebuggerStepThrough]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class Program
{
    private const System.Int32 IntervalInMinutes = 5;

    private static readonly System.Threading.ManualResetEvent QuitEvent = new(false);
    private static readonly TaskManager Task = InstanceManager.Instance.GetOrCreateInstance<TaskManager>();

    [System.STAThread]
    [System.Diagnostics.DebuggerNonUserCode]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args)
    {
        try
        {
            InitializeComponent();

            InstanceManager.Instance.GetExistingInstance<IListener>()?
                                    .Activate();

            InstanceManager.Instance.GetExistingInstance<PacketDispatchChannel>()
                                    .Activate();

            System.Console.CursorVisible = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Ngăn dừng đột ngột
                QuitEvent.Set();
            };


            // We can use a worker to listen to keyboard input without blocking the main thread
            InstanceManager.Instance.GetOrCreateInstance<TaskManager>().ScheduleWorker(
                "console.keyboard", "console",
                async (ctx, ct) => await LISTEN_TO_KEYBOARD(ctx, ct),
                new WorkerOptions
                {
                    RetainFor = System.TimeSpan.FromMinutes(10)
                }
            );

            // Schedule periodic report generation every 5 minutes
            InstanceManager.Instance.GetOrCreateInstance<TaskManager>().ScheduleWorker(
                "report.generator", "report",
                async (ctx, ct) => await GENERATE_PERIODIC_REPORTS(ctx, ct),
                new WorkerOptions
                {
                    RetainFor = System.TimeSpan.FromMinutes(IntervalInMinutes)
                }
            );

            InstanceManager.Instance.GetExistingInstance<ILogger>()
                                    .Info("Press 'Ctrl+R' to print reports.");

            InstanceManager.Instance.GetExistingInstance<ILogger>()
                                    .Info("Server is running. Press Ctrl+C to exit.");

            QuitEvent.WaitOne();
        }
        catch (System.Exception ex)
        {
            if (InstanceManager.Instance.GetExistingInstance<ILogger>() is null)
            {
                System.Console.Error.WriteLine("Fatal error in Main: " + ex);
            }
            else
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Error("Unhandled exception in Main", ex);
            }

            System.Environment.Exit(-1);
        }
        finally
        {
            System.Console.CursorVisible = false;
        }
    }

    public static void GenerateReport()
    {

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<ConnectionHub>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<PacketDispatchChannel>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<IProtocol>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<IListener>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetOrCreateInstance<BufferPoolManager>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<ObjectPoolManager>()
                                .GenerateReport());

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                .Info(InstanceManager.Instance
                                .GetExistingInstance<TaskManager>()
                                .GenerateReport());

    }

    public static void InitializeComponent()
    {
#if DEBUG
        ConfigurationManager.Instance.Get<NLogixOptions>()
                            .MinLevel = LogLevel.Debug;

        ILogger logger = new NLogix(cfg => cfg.RegisterTarget(new BatchConsoleLogTarget(t => t.EnableColors = true)));
#else
        ConfigurationManager.Instance.Get<NLogixOptions>()
                            .MinLevel = LogLevel.Debug;

        ILogger logger = new NLogix(cfg =>
        {
            cfg.RegisterTarget(new BatchConsoleLogTarget(t => t.EnableColors = false))
               .RegisterTarget(new BatchFileLogTarget());
        });
#endif
        InstanceManager.Instance.Register<ILogger>(logger);

        // Register application configuration
        AppConfig.Register();

        AutoXDbContextFactory factory = new();
        InstanceManager.Instance.Register<AutoXDbContextFactory>(factory);

        // Seed initial data if necessary
        using (AutoXDbContext context = factory.CreateDbContext())
        {
            // EnsureCreated sẽ tự kiểm tra database chưa có mới tạo, nếu đã có thì không làm gì cả.
            if (context.Database.EnsureCreated())
            {
                // Sau khi TẠO MỚI database thành công, seed data.
                DataSeeder.SeedAsync(context).Wait();
            }
        }

        System.Environment.Exit(0);

        PacketDispatchChannel channel = new(dispatchOptions =>
        {
            // Inbound
            dispatchOptions.WithMiddleware(new PermissionMiddleware());
            dispatchOptions.WithMiddleware(new ConcurrencyMiddleware());
            dispatchOptions.WithMiddleware(new RateLimitMiddleware());
            dispatchOptions.WithMiddleware(new UnwrapPacketMiddleware());
            dispatchOptions.WithMiddleware(new TimeoutMiddleware());

            // Outbound
            dispatchOptions.WithMiddleware(new WrapPacketMiddleware());

            // Logging
            dispatchOptions.WithLogging(InstanceManager.Instance.GetExistingInstance<ILogger>());
            dispatchOptions.WithErrorHandling((exception, command)
                => InstanceManager.Instance.GetExistingInstance<ILogger>()
                                           .Error($"Error handling command: {command}", exception));

            // OPS
            dispatchOptions.WithHandler(() => new PingOps());
            dispatchOptions.WithHandler(() => new HandshakeOps());
            dispatchOptions.WithHandler(() =>
                new AccountOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
            dispatchOptions.WithHandler(() =>
                new CustomerOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
            dispatchOptions.WithHandler(() =>
                new VehicleOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
            dispatchOptions.WithHandler(() =>
                new ReplacementPartOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
            dispatchOptions.WithHandler(() =>
                new SparePartOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
            dispatchOptions.WithHandler(() =>
                new SupplierOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                )
            );
        });

        AutoXProtocol xProtocol = new(channel);
        AutoXListener xListener = new(xProtocol);

        InstanceManager.Instance.Register<PacketDispatchChannel>(channel);
        InstanceManager.Instance.RegisterForClassOnly<IProtocol>(xProtocol);
        InstanceManager.Instance.RegisterForClassOnly<IListener>(xListener);
    }

    private static System.Threading.Tasks.Task LISTEN_TO_KEYBOARD(IWorkerContext ctx, System.Threading.CancellationToken ct)
    {
        return System.Threading.Tasks.Task.Run(async () =>
        {
            const System.Double TileCooldownSeconds = 1.0;
            const System.Double ReportCooldownSeconds = 5.0;

            System.DateTime startTime = Clock.NowUtc();
            System.DateTime lastTileTime = System.DateTime.MinValue;
            System.DateTime lastReportTime = System.DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                System.DateTime now = Clock.NowUtc();

                // Kiểm tra cooldown để tránh spam
                if ((now - lastTileTime).TotalSeconds >= TileCooldownSeconds)
                {
                    System.TimeSpan runningTime = now - startTime;
                    System.String runningTimeString = System.String.Format("{0:D2}:{1:D2}:{2:D2}", runningTime.Hours, runningTime.Minutes, runningTime.Seconds);
                    System.Console.Title = $"AutoX | Level: {ConfigurationManager.Instance.Get<NLogixOptions>().MinLevel} | {Task.Title} | {runningTimeString}";
                }

                if (System.Console.KeyAvailable)
                {
                    System.ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);
                    if (key.Key == System.ConsoleKey.R && (key.Modifiers & System.ConsoleModifiers.Control) != 0)
                    {
                        // Kiểm tra cooldown để tránh spam
                        if ((now - lastReportTime).TotalSeconds >= ReportCooldownSeconds)
                        {
                            GenerateReport();
                            lastReportTime = now;

                            ctx.Advance(1);
                        }
                    }

                    ctx.Beat();
                    await System.Threading.Tasks.Task.Delay(100, ct);
                }
            }

        }, ct);
    }

    private static async System.Threading.Tasks.Task GENERATE_PERIODIC_REPORTS(IWorkerContext ctx, System.Threading.CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>()
                                    .SaveReportToFile("buffer");

            InstanceManager.Instance.GetExistingInstance<ObjectPoolManager>()
                                    .SaveReportToFile("object");

            InstanceManager.Instance.GetExistingInstance<TaskManager>()
                                    .SaveReportToFile("task");

            ctx.Beat();
            ctx.Advance(1);

            await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromMinutes(IntervalInMinutes), ct);
        }
    }
}
