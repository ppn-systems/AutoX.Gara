// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}