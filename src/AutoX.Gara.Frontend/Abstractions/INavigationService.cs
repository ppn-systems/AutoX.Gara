// Copyright (c) 2026 PPN Corporation. All rights reserved.
namespace AutoX.Gara.Frontend.Abstractions;
/// <summary>
/// Abstraction cho navigation, gi�p ViewModel kh�ng ph? thu?c tr?c ti?p v�o Shell.
/// D? mock khi vi?t unit test sau n�y.
/// </summary>
public interface INavigationService
{
    /// <summary>Chuy?n sang m�n h�nh ch�nh sau khi đăng nhập th�nh c�ng.</summary>
    System.Threading.Tasks.Task GoToMainPageAsync();
}
