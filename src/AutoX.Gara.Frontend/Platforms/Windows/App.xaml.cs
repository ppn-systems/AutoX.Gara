// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Injection;
using System.Diagnostics;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AutoX.Gara.Frontend.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private static void OnUnhandledException(System.Object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            System.String message = $"[CRASH] source=WinUI.UnhandledException\nmsg={e.Message}\n{e.Exception}";

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

        // keep default behavior; we only log diagnostics
    }
}

