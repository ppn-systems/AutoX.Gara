using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Logging.Sinks;
using Nalix.SDK.Options;
using Nalix.SDK.Transport;
using Nalix.Common.Networking.Packets;
using Nalix.Framework.Configuration;

namespace AutoX.Gara.Frontend;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 1) �ang k� logger
        InstanceManager.Instance.Register<ILogger>(
            new NLogix(cfg =>
                cfg.RegisterTarget(
                    new BatchFileLogTarget(cfg => cfg.LogFileName = "AutoX.log")
                )
            )
        );

        // 2) �ang k� Packet Registry (Shared)
        AppConfig.Register();

        // 3) �ang k� TcpSession (Frontend)
        var catalog = InstanceManager.Instance.GetExistingInstance<IPacketRegistry>();
        
        // S? d?ng ConfigurationManager d? l?y instance chu?n c?a framework
        var options = ConfigurationManager.Instance.Get<TransportOptions>();
        options.Address = "127.0.0.1";
        options.Port = 57206;
        options.EncryptionEnabled = false; // Ph?i t?t ban d?u d? Handshake kh�ng b? crash (Nalix.SDK bug fix)

        var session = new TcpSession(options, catalog!);
        InstanceManager.Instance.Register<TcpSession>(session);

        base.GoToAsync("///LoginPage");
    }
}