// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services;

/// <summary>
/// Implementation dùng Shell MAUI. Đây là nơi DUY NHẤT trong UI layer
/// được phép gọi Shell.Current trực tiếp.
/// </summary>
public sealed class ShellNavigationService : INavigationService
{
    public async Task GoToMainPageAsync()
    {
        // Xóa LoginPage ra khỏi shell history để back không về được
        ShellItem? loginItem = Shell.Current.Items
            .FirstOrDefault(i => i.Title == "Login");

        if (loginItem is not null)
        {
            Shell.Current.Items.Remove(loginItem);
        }

        // Resize window về kích thước app chính (Windows/Mac)
        if (Application.Current?.Windows[0] is { } window)
        {
            window.Width = 1280;
            window.Height = 720;
        }

        await Shell.Current.GoToAsync("///MainPage");
    }
}