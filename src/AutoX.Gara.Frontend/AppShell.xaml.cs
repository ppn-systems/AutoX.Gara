// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Primitives;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Logging.Sinks;
using Nalix.SDK.Options;
using Nalix.SDK.Transport;

namespace AutoX.Gara.Frontend;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 1) Đăng ký logger
        InstanceManager.Instance.Register<ILogger>(
            new NLogix(cfg =>
                cfg.RegisterTarget(
                    new BatchFileLogTarget(cfg => cfg.LogFileName = "AutoX.log")
                )
            )
        );

        // 2) Đăng ký Packet Registry (Shared)
        AppConfig.Register();

        // 3) Đăng ký TcpSession (Frontend)
        var catalog = InstanceManager.Instance.GetExistingInstance<IPacketRegistry>();

        // Sử dụng ConfigurationManager để lấy instance chuẩn của framework
        var options = ConfigurationManager.Instance.Get<TransportOptions>();

        // Fix: Ensure encryption is disabled if no secret is present to avoid initialization errors before handshake
        if (options.Secret.IsZero)
        {
            options.EncryptionEnabled = false;
            options.Secret = Bytes32.Zero;
            options.ServerPublicKey = "2CBA6DF1C0440662D7FAD1DFC59640C5669C4B5A03885813C9DF20CE69832274";
        }

        var session = new TcpSession(options, catalog!);
        InstanceManager.Instance.Register<TcpSession>(session);

        base.GoToAsync("///LoginPage");
    }
}
