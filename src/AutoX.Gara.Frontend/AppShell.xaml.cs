// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Views;
using AutoX.Gara.Shared;
using Microsoft.Maui.Controls;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Logging.Sinks;

namespace AutoX.Gara.Frontend;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Đăng ký logger với custom log file name formatter
        InstanceManager.Instance.Register<ILogger>(
            new NLogix(cfg =>
                cfg.RegisterTarget(
                    new BatchFileLogTarget(cfg => cfg.LogFileName = "AutoX.log")
                )
            )
        );

        AppConfig.Register();

        base.GoToAsync("///LoginPage");
    }
}
