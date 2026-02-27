// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Communication;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared;
using Nalix.Common.Diagnostics;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Network.Dispatch;
using Nalix.Network.Middleware.Inbound;
using Nalix.Network.Middleware.Outbound;

namespace AutoX.Gara.Backend;

public static class Program
{
    [System.STAThread]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void Main(System.String[] args)
    {

    }

    public static void InitializeComponent()
    {
        InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);

        AutoXDbContextFactory factory = InstanceManager.Instance.GetOrCreateInstance<AutoXDbContextFactory>();

        PacketDispatchChannel channel = new(cfg => cfg
            // Inbound
            .WithInbound(new PermissionMiddleware())
            .WithInbound(new ConcurrencyMiddleware())
            .WithInbound(new RateLimitMiddleware())
            .WithInbound(new UnwrapPacketMiddleware())
            .WithInbound(new TimeoutMiddleware())
            // Outbound
            .WithOutbound(new WrapPacketMiddleware())
            // Logging
            .WithLogging(InstanceManager.Instance.GetExistingInstance<ILogger>())
            .WithErrorHandling((exception, command)
                => InstanceManager.Instance.GetExistingInstance<ILogger>()
                                           .Error($"Error handling command: {command}", exception))
            // OPS
            .WithHandler(() => new HandshakeOps())
        //.WithHandler(() => new AccountOps(credentials))
        );

        AppConfig.Register();
    }
}
