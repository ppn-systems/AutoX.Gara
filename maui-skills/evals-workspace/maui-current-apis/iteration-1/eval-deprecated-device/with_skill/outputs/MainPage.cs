using Microsoft.Maui.ApplicationModel;

public partial class MainPage : ContentPage
{
    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://example.com"));
    }
}
