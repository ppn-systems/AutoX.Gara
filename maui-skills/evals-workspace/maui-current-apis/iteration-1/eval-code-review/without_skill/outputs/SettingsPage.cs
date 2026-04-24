using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace MyApp;

public partial class SettingsPage : ContentPage
{
    private readonly ISettingsService _settingsService;

    public SettingsPage(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        MainThread.BeginInvokeOnMainThread(() => {
            StatusLabel.Text = "Ready";
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _settingsService?.Load();
        On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()
            .SetUseSafeArea(true);
    }
}
