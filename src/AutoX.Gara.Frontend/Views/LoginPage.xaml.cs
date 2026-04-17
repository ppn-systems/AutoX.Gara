using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Trang đăng nhập chính.
/// Tuân thủ Dependency Injection: nhận ViewModel trực tiếp từ Container thay vì khởi tạo thủ công.
/// </summary>
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        
        // Gán BindingContext từ đối tượng được inject qua Constructor
        BindingContext = viewModel;
    }
}
