# Eval Transcript: Deprecated Device API (without skill)

## Task

Fix the following .NET MAUI code that uses a deprecated API to open a URL in the browser:

```csharp
public partial class MainPage : ContentPage
{
    private void OnHelpClicked(object sender, EventArgs e)
    {
        // TODO: open https://example.com in browser
        Device.OpenUri(new Uri("https://example.com"));
    }
}
```

## Analysis

### Problem Identified

The code uses `Device.OpenUri()`, which was carried over from Xamarin.Forms. In .NET MAUI, the `Device` class has been significantly restructured and `Device.OpenUri()` is not available. Calling it will result in a compile error or runtime failure depending on the MAUI version.

### Correct API

In .NET MAUI, opening a URI in the default browser is handled by the `Launcher` class in the `Microsoft.Maui.ApplicationModel` namespace:

- `Launcher.OpenAsync(Uri uri)` — opens the given URI using the appropriate app (browser for http/https URLs)
- `Launcher.OpenAsync(string uri)` — string overload also available
- `Launcher.TryOpenAsync(Uri uri)` — safer variant that returns a bool indicating success

Since this is an async operation, the event handler must also be made `async`.

### Changes Made

1. Replaced `Device.OpenUri(new Uri("https://example.com"))` with `await Launcher.OpenAsync(new Uri("https://example.com"))`
2. Changed the event handler signature from `private void` to `private async void` to support `await`
3. Added `using Microsoft.Maui.ApplicationModel;` to bring `Launcher` into scope

## Corrected Code

```csharp
using Microsoft.Maui.ApplicationModel;

namespace MyApp;

public partial class MainPage : ContentPage
{
    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://example.com"));
    }
}
```

## Confidence

High. `Launcher.OpenAsync` is the well-established .NET MAUI replacement for `Device.OpenUri`. This is a direct, documented migration path from Xamarin.Forms to .NET MAUI.
