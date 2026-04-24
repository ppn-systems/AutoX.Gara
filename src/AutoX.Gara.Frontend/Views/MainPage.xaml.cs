// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.ApplicationModel;using Microsoft.Maui.Controls;using System;

namespace AutoX.Gara.Frontend.Views;

public partial class MainPage : ContentPage

{
    public MainPage() => InitializeComponent();


    private async void OnGitHubClick(object? sender, System.EventArgs e)

    {
        const string repoUrl = "https://github.com/ppn-systems/Nalix";

        try

        {
            await Launcher.Default.OpenAsync(new System.Uri(repoUrl)); // MAUI API to open in browser

        }

        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Cannot open link: {ex.Message}", "OK");
        }

    }
}
