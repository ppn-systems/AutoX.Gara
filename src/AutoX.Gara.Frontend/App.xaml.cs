// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new(new AppShell())
        {
            // Chỉ Windows desktop mới đổi size window được
            Width = 400,
            Height = 520
        };

        return window;
    }
}