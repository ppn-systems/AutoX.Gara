using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared;
using Microsoft.Extensions.Logging;
using Nalix.Common.Concurrency;
using Nalix.Framework.Injection;
using Nalix.Framework.Options;
using Nalix.Framework.Tasks;
using Nalix.Network.Hosting;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
namespace AutoX.Gara.Backend;
[DebuggerStepThrough]
[ExcludeFromCodeCoverage]
public static class Program
{
    private const int IntervalInMinutes = 5;
    private static readonly TaskManager TaskManager = InstanceManager.Instance.GetOrCreateInstance<TaskManager>();
    private static readonly CancellationTokenSource ShutdownCts = new();
    [STAThread]
    public static void Main(string[] args)
    {
        ILogger logger = Startup.CreateBootstrapLogger();
        try
        {
            // 1. Root initialization
            InstanceManager.Instance.Register<ILogger>(logger);
            AppConfig.Register();
            // 2. Database ensure created (side effect during startup)
            EnsureDatabaseCreated();
            // 3. Configure app pipeline + host lifecycle
            using NetworkApplication app = Startup.Configure(logger);
            SetupConsole(logger);
            app.RunAsync(ShutdownCts.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Expected during graceful shutdown.
        }
        catch (Exception ex)
        {
            (InstanceManager.Instance.GetExistingInstance<ILogger>() ?? logger).Error("Critical failure during startup", ex);
            Environment.ExitCode = -1;
        }
        finally
        {
            ShutdownCts.Dispose();
        }
    }
    private static void EnsureDatabaseCreated()
    {
        var dbFactory = new AutoXDbContextFactory();
        using var context = dbFactory.CreateDbContext();
        if (context.Database.EnsureCreated())
        {
            DataSeeder.SeedAsync(context).Wait();
        }
    }
    private static void SetupConsole(ILogger logger)
    {
        Console.CursorVisible = false;
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            if (!ShutdownCts.IsCancellationRequested)
            {
                logger.Info("Shutdown signal received. Stopping host...");
                ShutdownCts.Cancel();
            }
        };
        TaskManager.ScheduleWorker(
            "console.keyboard",
            "console",
            async (ctx, ct) => await LISTEN_TO_KEYBOARD(ctx, ct).ConfigureAwait(false),
            new WorkerOptions { RetainFor = TimeSpan.FromMinutes(10) }
        );
        TaskManager.ScheduleWorker(
            "report.generator",
            "report",
            async (ctx, ct) => await GENERATE_PERIODIC_REPORTS(ctx, ct).ConfigureAwait(false),
            new WorkerOptions { RetainFor = TimeSpan.FromMinutes(IntervalInMinutes) }
        );
        logger.Info("AutoX Backend System is ONLINE.");
        logger.Info("Press Ctrl+R to print immediate diagnostic reports.");
        logger.Info("Press Ctrl+C to shutdown safely.");
    }
    public static void GenerateReport()
    {
        var inst = InstanceManager.Instance;
        var logger = inst.GetExistingInstance<ILogger>();
        logger?.Info(inst.GenerateReport());
    }
    private static Task LISTEN_TO_KEYBOARD(IWorkerContext ctx, CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            DateTime lastReportTime = DateTime.MinValue;
            while (!IsStopping(ct))
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
                await DelayWithStopAsync(TimeSpan.FromMilliseconds(100), ct).ConfigureAwait(false);
            }
        }, ct);
    }
    private static async Task GENERATE_PERIODIC_REPORTS(IWorkerContext ctx, CancellationToken ct)
    {
        while (!IsStopping(ct))
        {
            ctx.Beat();
            ctx.Advance(1);
            await DelayWithStopAsync(TimeSpan.FromMinutes(IntervalInMinutes), ct).ConfigureAwait(false);
        }
    }
    private static bool IsStopping(CancellationToken token)
        => token.IsCancellationRequested || ShutdownCts.IsCancellationRequested;
    private static async Task DelayWithStopAsync(TimeSpan delay, CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, ShutdownCts.Token);
        try
        {
            await Task.Delay(delay, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Graceful cancellation.
        }
    }
}
