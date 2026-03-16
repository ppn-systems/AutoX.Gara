// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services;

/// <summary>
/// Implementation dùng Shell MAUI. Ðây là noi DUY NH?T trong UI layer
/// du?c phép g?i Shell.Current tr?c ti?p.
/// </summary>
public sealed class ShellNavigationService : INavigationService
{
    public async Task GoToMainPageAsync()
    {
        // Xóa LoginPage ra kh?i shell history d? back không vụ du?c
        ShellItem? loginItem = Shell.Current.Items
            .FirstOrDefault(i => i.Title is "Login" or "Đăng nhập");

        if (loginItem is not null)
        {
            Shell.Current.Items.Remove(loginItem);
        }

        _ = Task.Run(async () =>
            await InstanceManager.Instance.GetOrCreateInstance<TcpSession>().HeartbeatLoopAsync(default));

        // Resize window vụ kích thu?c app chính (Windows/Mac)
        if (Application.Current?.Windows[0] is { } window)
        {
            window.Width = 1280;
            window.Height = 720;
        }

        await Shell.Current.GoToAsync("///MainPage");
    }
}
