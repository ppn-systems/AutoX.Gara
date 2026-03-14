// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho navigation, giúp ViewModel không ph? thu?c tr?c ti?p vào Shell.
/// D? mock khi vi?t unit test sau này.
/// </summary>
public interface INavigationService
{
    /// <summary>Chuy?n sang màn hình chính sau khi dang nh?p thành công.</summary>
    System.Threading.Tasks.Task GoToMainPageAsync();
}
