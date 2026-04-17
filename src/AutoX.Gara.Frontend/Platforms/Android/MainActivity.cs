using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Android.App;

using Android.Content.PM;

using Microsoft.Maui;

namespace AutoX.Gara.Frontend;

[Activity(

    Theme = "@style/Maui.SplashTheme",

    MainLauncher = true,

    LaunchMode = LaunchMode.SingleTop,

    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

public class MainActivity : MauiAppCompatActivity;
