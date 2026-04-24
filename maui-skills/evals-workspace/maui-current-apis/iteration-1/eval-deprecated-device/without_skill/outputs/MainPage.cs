using Microsoft.Maui.ApplicationModel;

namespace MyApp;

public partial class MainPage : ContentPage
{
    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://example.com"));
    }
}
