// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));

        base.GoToAsync("///LoginPage");
    }
}
