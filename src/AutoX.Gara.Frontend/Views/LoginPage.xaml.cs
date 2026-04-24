// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Services;
using AutoX.Gara.Frontend.Services.Accounts;
using AutoX.Gara.UI.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        var inst = InstanceManager.Instance;

        var accountService = inst.GetExistingInstance<IAccountService>();
        if (accountService is null)
        {
            accountService = new AccountService();
            inst.Register<IAccountService>(accountService);
        }

        var navigationService = inst.GetExistingInstance<INavigationService>();
        if (navigationService is null)
        {
            navigationService = new ShellNavigationService();
            inst.Register<INavigationService>(navigationService);
        }

        BindingContext = new LoginViewModel(accountService, navigationService);
    }
}
