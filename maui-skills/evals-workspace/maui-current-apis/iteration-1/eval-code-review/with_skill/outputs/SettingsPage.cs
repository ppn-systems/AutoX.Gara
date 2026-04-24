using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace MyApp;

public partial class SettingsPage : ContentPage
{
    private readonly ISettingsService _settingsService;

    public SettingsPage(ISettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        MainThread.BeginInvokeOnMainThread(() => {
            StatusLabel.Text = "Ready";
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _settingsService?.Load();
        SafeAreaEdges = Microsoft.Maui.Primitives.SafeAreaEdges.All;
    }
}
