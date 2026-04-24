// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Services;
using AutoX.Gara.Frontend.Services.Accounts;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
namespace AutoX.Gara.Frontend;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        ConfigureFrontendInstances();
        return builder.Build();
    }
    private static void ConfigureFrontendInstances()
    {
        var instanceManager = InstanceManager.Instance;
        if (instanceManager.GetExistingInstance<IAccountService>() is null)
        {
            instanceManager.Register<IAccountService>(new AccountService());
        }
        if (instanceManager.GetExistingInstance<INavigationService>() is null)
        {
            instanceManager.Register<INavigationService>(new ShellNavigationService());
        }
        _ = UiTextConfiguration.Current;
        try
        {
            ConfigurationManager.Instance.Flush();
        }
        catch
        {
            // Keep startup resilient if config flush fails on first launch.
        }
    }
}
