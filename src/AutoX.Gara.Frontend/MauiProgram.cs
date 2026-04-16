using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Services;
using AutoX.Gara.Frontend.Services.Accounts;
using AutoX.Gara.Frontend.Views;
using AutoX.Gara.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace AutoX.Gara.Frontend;

/// <summary>
/// Điểm khởi đầu của ứng dụng MAUI.
/// Thiết lập DI Container, Logging và các dịch vụ nền tảng theo tiêu chuẩn công nghiệp.
/// </summary>
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

        // 1. Cấu hình Logging tập trung
#if DEBUG
#endif

        // 2. Đăng ký Services (Business Logic)
        // Sử dụng Singleton cho các dịch vụ không lưu trạng thái UI hoặc cần dùng chung.
        builder.Services.AddSingleton<IAccountService, AccountService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        // 3. Đăng ký ViewModels
        // Đăng ký Transient để đảm bảo mỗi lần vào Page sẽ có một state mới sạch sẽ.
        builder.Services.AddTransient<LoginViewModel>();

        // 4. Đăng ký Pages
        builder.Services.AddTransient<LoginPage>();

        return builder.Build();
    }
}