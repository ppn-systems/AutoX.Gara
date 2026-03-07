// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho navigation, giúp ViewModel không phụ thuộc trực tiếp vào Shell.
/// Dễ mock khi viết unit test sau này.
/// </summary>
public interface INavigationService
{
    /// <summary>Chuyển sang màn hình chính sau khi đăng nhập thành công.</summary>
    System.Threading.Tasks.Task GoToMainPageAsync();
}