using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
namespace AutoX.Gara.Frontend;
[Application]
public class MainApplication(System.IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
