using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AutoX.Gara.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Buffers;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Tasks;
using Nalix.Logging;
using Nalix.Network.Connections;
using Nalix.Network.Hosting;
using Nalix.Common.Networking;
using Nalix.Common.Concurrency;
using Nalix.Framework.Options;
using AutoX.Gara.Shared;

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
    public static void Main(string[] args)
    {
        try
        {
            // 1. Root Initialization
            ILogger logger = Startup.CreateBootstrapLogger();
            InstanceManager.Instance.Register<ILogger>(logger);

            AppConfig.Register();

            // 2. Database Ensure Created (Side-effect during startup)
            var dbFactory = new AutoXDbContextFactory();
            using (var context = dbFactory.CreateDbContext())
            {
                if (context.Database.EnsureCreated())
                {
                    DataSeeder.SeedAsync(context).Wait();
                }
            }

            // 3. Configure App Pipeline
            App = Startup.Configure(logger);
            App.ActivateAsync().GetAwaiter().GetResult();

            SetupConsole(logger);

            QuitEvent.WaitOne();
            
            logger.Info("Deactivating application...");
            App.DeactivateAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            logger?.Error("Critical failure during startup", ex);
            Environment.Exit(-1);
        }
    }

    private static void SetupConsole(ILogger logger)
    {
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

        logger.Info("AutoX Backend System is ONLINE.");
        logger.Info("Press 'Ctrl+R' to print immediate diagnostic reports.");
        logger.Info("Press Ctrl+C to shutdown safely.");
    }

    public static void GenerateReport()
    {
        var inst = InstanceManager.Instance;
        var logger = inst.GetExistingInstance<ILogger>();
        
        logger.Info(inst.GenerateReport());
        // Simple report for now to avoid assembly issues
        // logger.Info(inst.GetOrCreateInstance<BufferPoolManager>().GenerateReport());
        // logger.Info(inst.GetExistingInstance<ObjectPoolManager>().GenerateReport());
        // logger.Info(_taskManager.GenerateReport());
    }

    private static Task LISTEN_TO_KEYBOARD(IWorkerContext ctx, System.Threading.CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            DateTime lastReportTime = DateTime.MinValue;
            while (!ct.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.R && (key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        if ((DateTime.UtcNow - lastReportTime).TotalSeconds >= 5.0)
                        {
                            GenerateReport();
                            lastReportTime = DateTime.UtcNow;
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
            // InstanceManager.Instance.GetOrCreateInstance<BufferPoolManager>().SaveReportToFile("buffer");
            // InstanceManager.Instance.GetExistingInstance<ObjectPoolManager>().SaveReportToFile("object");
            // _taskManager.SaveReportToFile("task");
            ctx.Beat();
            ctx.Advance(1);
            await Task.Delay(TimeSpan.FromMinutes(IntervalInMinutes), ct);
        }
    }
}
