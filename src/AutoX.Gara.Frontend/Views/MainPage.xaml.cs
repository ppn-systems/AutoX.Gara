// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace AutoX.Gara.Frontend.Views;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();

    [System.Obsolete]
    private async void OnGitHubClick(System.Object sender, System.EventArgs e)
    {
        const System.String repoUrl = "https://github.com/ppn-systems/Nalix";
        try
        {
            await Launcher.Default.OpenAsync(new System.Uri(repoUrl)); // MAUI API to open in browser
        }
        catch (System.Exception ex)
        {
            await DisplayAlert("Error", $"Cannot open link: {ex.Message}", "OK");
        }
    }
}
