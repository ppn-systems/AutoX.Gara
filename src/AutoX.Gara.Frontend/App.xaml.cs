// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend;

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}