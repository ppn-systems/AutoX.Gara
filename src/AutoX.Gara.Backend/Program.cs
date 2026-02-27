// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Communication;
using AutoX.Gara.Application.Customers;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Networking;
using AutoX.Gara.Shared;
using Nalix.Common.Diagnostics;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Network.Abstractions;
using Nalix.Network.Dispatch;
using Nalix.Network.Middleware.Inbound;
using Nalix.Network.Middleware.Outbound;

namespace AutoX.Gara.Backend;

public static class Program
{
    private static System.Threading.ManualResetEvent QuitEvent = new(false);

    [System.STAThread]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args)
    {
        InitializeComponent();

        InstanceManager.Instance.GetExistingInstance<IListener>()?
                                .Activate();

        InstanceManager.Instance.GetExistingInstance<ILogger>()
                                           .Info("Server is running. Press Ctrl+C to exit.");

        System.Console.CancelKeyPress += (sender, e) => QuitEvent.Set();
        QuitEvent.WaitOne();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0301:Simplify collection initialization", Justification = "<Pending>")]
    public static void InitializeComponent()
    {
        InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);

        // Register application configuration
        AppConfig.Register();

        AutoXDbContextFactory factory = InstanceManager.Instance.GetOrCreateInstance<AutoXDbContextFactory>();
        AutoXDbContext dbContext = factory.CreateDbContext(System.Array.Empty<System.String>());

        InstanceManager.Instance.Register<AutoXDbContext>(dbContext);

        PacketDispatchChannel channel = new(dispatchOptions =>
        {
            // Inbound
            dispatchOptions.WithInbound(new PermissionMiddleware());
            dispatchOptions.WithInbound(new ConcurrencyMiddleware());
            dispatchOptions.WithInbound(new RateLimitMiddleware());
            dispatchOptions.WithInbound(new UnwrapPacketMiddleware());
            dispatchOptions.WithInbound(new TimeoutMiddleware());

            // Outbound
            dispatchOptions.WithOutbound(new WrapPacketMiddleware());
            // Logging
            dispatchOptions.WithLogging(InstanceManager.Instance.GetExistingInstance<ILogger>());
            dispatchOptions.WithErrorHandling((exception, command)
                => InstanceManager.Instance.GetExistingInstance<ILogger>()
                                           .Error($"Error handling command: {command}", exception));
            // OPS
            dispatchOptions.WithHandler(() => new HandshakeOps());
            dispatchOptions.WithHandler(() => new AccountOps(InstanceManager.Instance.GetExistingInstance<AutoXDbContext>()));
            dispatchOptions.WithHandler(() => new CustomerOps(InstanceManager.Instance.GetExistingInstance<AutoXDbContext>()));
        });

        AutoXProtocol xProtocol = new(channel);
        AutoXListener xListener = new(xProtocol);

        InstanceManager.Instance.Register<IProtocol>(xProtocol);
        InstanceManager.Instance.Register<IListener>(xListener);
    }
}
