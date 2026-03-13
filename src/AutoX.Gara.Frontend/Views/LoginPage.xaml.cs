// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Services;
using AutoX.Gara.Frontend.Services.Accounts;
using AutoX.Gara.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        IAccountService loginService = new AccountService();
        INavigationService navigationService = new ShellNavigationService();
        BindingContext = new LoginViewModel(loginService, navigationService);
    }
}