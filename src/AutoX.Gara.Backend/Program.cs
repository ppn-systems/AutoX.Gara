// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Communication;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Networking;
using AutoX.Gara.Shared;
using Nalix.Common.Concurrency;
using Nalix.Common.Diagnostics;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Framework.Options;
using Nalix.Framework.Tasks;
using Nalix.Framework.Time;
using Nalix.Logging;
using Nalix.Logging.Configuration;
using Nalix.Network.Abstractions;
using Nalix.Network.Connections;
using Nalix.Network.Dispatch;
using Nalix.Network.Middleware.Inbound;
using Nalix.Network.Middleware.Outbound;
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
    private static readonly System.Int32 IntervalInMinutes = 5;
    private static readonly System.Threading.ManualResetEvent QuitEvent = new(false);

    [System.STAThread]
    [System.Diagnostics.DebuggerNonUserCode]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args)
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0301:Simplify collection initialization", Justification = "<Pending>")]
    public static void InitializeComponent()
    {
#if DEBUG
        ConfigurationManager.Instance.Get<NLogixOptions>()
                            .MinLevel = LogLevel.Meta;
#else 
        ConfigurationManager.Instance.Get<NLogixOptions>()
                            .MinLevel = LogLevel.Information;
#endif

        InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);

        // Register application configuration
        AppConfig.Register();

        AutoXDbContextFactory factory = new();
        InstanceManager.Instance.Register<AutoXDbContextFactory>(factory);

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
            dispatchOptions.WithHandler(() => new HandshakeOps());
            dispatchOptions.WithHandler(() =>
                new AccountOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                                            .CreateDbContext(System.Array.Empty<System.String>())
                )
            );
            dispatchOptions.WithHandler(() =>
                new CustomerOps(
                    InstanceManager.Instance.GetExistingInstance<AutoXDbContextFactory>()
                                            .CreateDbContext(System.Array.Empty<System.String>())
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
            System.DateTime lastReportTime = System.DateTime.MinValue;
            const System.Double ReportCooldownSeconds = 5.0;

            await System.Threading.Tasks.Task.Delay(50, ct);

            while (!ct.IsCancellationRequested)
            {
                if (System.Console.KeyAvailable)
                {
                    System.ConsoleKeyInfo key = System.Console.ReadKey(intercept: true);
                    if (key.Key == System.ConsoleKey.R && (key.Modifiers & System.ConsoleModifiers.Control) != 0)
                    {
                        System.DateTime now = Clock.NowUtc();

                        // Kiểm tra cooldown để tránh spam
                        if ((now - lastReportTime).TotalSeconds >= ReportCooldownSeconds)
                        {
                            GenerateReport();
                            lastReportTime = now;

                            ctx.Advance(1);
                        }
                    }

                    ctx.Beat();
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
