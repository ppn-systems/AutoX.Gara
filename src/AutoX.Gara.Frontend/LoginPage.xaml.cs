// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Services;
using AutoX.Gara.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        ILoginService loginService = new LoginService();
        INavigationService navigationService = new ShellNavigationService();
        BindingContext = new LoginViewModel(loginService, navigationService);
    }
}