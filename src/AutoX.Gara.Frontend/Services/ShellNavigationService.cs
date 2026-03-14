// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using Microsoft.Maui.Controls;
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
        // Xóa LoginPage ra kh?i shell history d? back không v? du?c
        ShellItem? loginItem = Shell.Current.Items
            .FirstOrDefault(i => i.Title is "Login" or "Ðang nh?p");

        if (loginItem is not null)
        {
            Shell.Current.Items.Remove(loginItem);
        }

        // Resize window v? kích thu?c app chính (Windows/Mac)
        if (Application.Current?.Windows[0] is { } window)
        {
            window.Width = 1280;
            window.Height = 720;
        }

        await Shell.Current.GoToAsync("///MainPage");
    }
}
