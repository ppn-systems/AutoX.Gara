using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Services;

/// <summary>
/// Implementation điều hướng dựa trên MAUI Shell.
/// Đây là nơi duy nhất quản lý việc chuyển trang và cấu hình cửa sổ ứng dụng.
/// </summary>
public sealed class ShellNavigationService : INavigationService
{
    /// <summary>
    /// Chuyển hướng tới trang chủ và định cấu hình lại giao diện.
    /// </summary>
    public async Task GoToMainPageAsync()
    {
        // 1. Loại bỏ trang đăng nhập khỏi lịch sử Shell để ngăn người dùng quay lại bằng nút Back.
        var loginItem = Shell.Current.Items
            .FirstOrDefault(i => i.Title is "Login" or "Đăng nhập");

        if (loginItem is not null)
        {
            Shell.Current.Items.Remove(loginItem);
        }

        // 2. Định cấu hình kích thước Window chuẩn cho giao diện desktop chính.
        if (Application.Current?.Windows.Count > 0)
        {
            var window = Application.Current.Windows[0];
            window.Width = 1280;
            window.Height = 720;
        }

        // 3. Thực hiện chuyển hướng Shell tới Route chính.
        await Shell.Current.GoToAsync("///MainPage");
    }
}
