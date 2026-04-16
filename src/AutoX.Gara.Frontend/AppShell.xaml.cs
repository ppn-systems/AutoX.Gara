// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Logging.Sinks;

namespace AutoX.Gara.Frontend;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Ðang ký logger vụi custom log file name formatter
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
