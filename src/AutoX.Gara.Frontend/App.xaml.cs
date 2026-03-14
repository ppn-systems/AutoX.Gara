// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Framework.Injection;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        InstallGlobalExceptionHandlers();
        LogAppDataPath();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new(new AppShell())
        {
            // Ch? Windows desktop mới đổi size window du?c
            Width = 400,
            Height = 520,
        };

        return window;
    }

    private static void InstallGlobalExceptionHandlers()
    {
        System.AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                LogCrash("AppDomain.UnhandledException", e.ExceptionObject as System.Exception);
            }
            catch
            {
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                LogCrash("TaskScheduler.UnobservedTaskException", e.Exception);
                e.SetObserved();
            }
            catch
            {
            }
        };
    }

    private static void LogAppDataPath()
    {
        try
        {
            string path = Microsoft.Maui.Storage.FileSystem.AppDataDirectory;
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            logger?.Info($"[FE.{nameof(App)}] AppDataDirectory={path}");
            Debug.WriteLine($"[FE.{nameof(App)}] AppDataDirectory={path}");
        }
        catch
        {
        }
    }

    private static void LogCrash(System.String source, System.Exception? ex)
    {
        try
        {
            System.String location = Shell.Current?.CurrentState?.Location?.ToString() ?? "(no-shell)";
            System.String message = $"[CRASH] source={source} location={location}\n{(ex is null ? "(null exception)" : ex.ToString())}";

            // Prefer existing logger (registered by AppShell) to keep logs in AutoX.log.
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            if (logger is not null)
            {
                logger.Error(message);
            }
            else
            {
                InstanceManager.Instance.GetOrCreateInstance<ILogger>().Error(message);
            }

            Debug.WriteLine(message);

            // Extra fallback: write crash log to a known location for easy sharing.
            try
            {
                System.String path = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "crash.log");
                File.AppendAllText(path, $"{System.DateTimeOffset.Now:o}\n{message}\n\n");
            }
            catch
            {
            }
        }
        catch
        {
        }
    }
}
