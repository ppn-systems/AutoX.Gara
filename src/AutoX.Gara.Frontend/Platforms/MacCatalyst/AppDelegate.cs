using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Foundation;

using Microsoft.Maui;

using Microsoft.Maui.Hosting;

namespace AutoX.Gara.Frontend;

[Register("AppDelegate")]

public class AppDelegate : MauiUIApplicationDelegate

{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
