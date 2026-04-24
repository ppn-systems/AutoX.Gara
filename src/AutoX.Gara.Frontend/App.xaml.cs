// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend;

/// <summary>
/// Lớp ứng dụng chính, quản lý vòng đời và xử lý lỗi toàn cục.
/// Tuân thủ tiêu chuẩn Resilience và Traceability công nghiệp.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Cài đặt các trình xử lý ngoại lệ không mong muốn toàn hệ thống
        InstallGlobalExceptionHandlers();

        // Ghi lại thông tin môi trường thực thi để hỗ trợ chuẩn đoán
        LogAppDataPath();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Khởi tạo cửa sổ ban đầu dành cho màn hình Đăng nhập (kích thước nhỏ)
        var window = new Window(new AppShell())
        {
            Width = 400,
            Height = 520,
        };

        return window;
    }

    private static void InstallGlobalExceptionHandlers()
    {
        // Xử lý lỗi UnhandledException trên AppDomain
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            LogCrash("AppDomain.UnhandledException", e.ExceptionObject as Exception);
        };

        // Xử lý lỗi trong các tác vụ bất đồng bộ (Background Tasks)
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogCrash("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved(); // Đánh dấu đã xử lý để tránh sập app ngay lập tức
        };
    }

    private static void LogAppDataPath()
    {
        try
        {
            string path = Microsoft.Maui.Storage.FileSystem.AppDataDirectory;
            var logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

            logger?.LogInformation("[FE.App] AppDataDirectory={Path}", path);
            Debug.WriteLine($"[FE.App] AppDataDirectory={path}");
        }
        catch { /* Phớt lờ lỗi trong quá trình cấu hình logging */ }
    }

    /// <summary>
    /// Ghi lại nhật ký khi ứng dụng gặp sự cố nghiêm trọng (Crash).
    /// Hỗ trợ truy vết vị trí xảy ra lỗi và nội dung exception.
    /// </summary>
    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            string location = Shell.Current?.CurrentState?.Location?.ToString() ?? "(no-shell)";
            string message = $"[CRASH] source={source} location={location}\n{(ex is null ? "(null exception)" : ex.ToString())}";

            // Log vào hệ thống logging chính
            var logger = InstanceManager.Instance.GetExistingInstance<ILogger>()
                      ?? InstanceManager.Instance.GetOrCreateInstance<ILogger>();

            logger.LogError(new EventId(999, "Crash"), ex, "{Message}", message);
            Debug.WriteLine(message);

            // Ghi dự phòng vào file local phục vụ gửi báo cáo lỗi sau này
            try
            {
                string path = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "crash.log");
                File.AppendAllText(path, $"{DateTimeOffset.Now:o}\n{message}\n\n");
            }
            catch { /* File system lock hoặc Permission issue */ }
        }
        catch { /* Fail-safe tối thượng */ }
    }
}
