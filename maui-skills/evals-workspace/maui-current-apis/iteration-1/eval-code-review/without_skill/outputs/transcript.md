# Code Review Transcript - SettingsPage.cs (Without Skill Guidance)

## Task
Review a MAUI ContentPage and fix all deprecated or incorrect patterns using general knowledge only (no skill guidance).

## Input Code

```csharp
using Xamarin.Forms;

namespace MyApp;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        Device.BeginInvokeOnMainThread(() => {
            StatusLabel.Text = "Ready";
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var service = DependencyService.Get<ISettingsService>();
        service?.Load();
        On<Xamarin.Forms.PlatformConfiguration.iOS>()
            .SetUseSafeArea(true);
    }
}
```

## Analysis

### Issues Found

#### Issue 1: Wrong/Deprecated Namespace - `using Xamarin.Forms;`
- **Severity**: Critical
- **Description**: The code uses `Xamarin.Forms` namespace which is the old Xamarin.Forms library. .NET MAUI uses `Microsoft.Maui.Controls` and related namespaces.
- **Fix**: Replace `using Xamarin.Forms;` with `using Microsoft.Maui.Controls;`

#### Issue 2: Deprecated API - `Device.BeginInvokeOnMainThread`
- **Severity**: High
- **Description**: `Device.BeginInvokeOnMainThread()` is obsolete in .NET MAUI. The `Device` class has been largely deprecated. The recommended replacement is `MainThread.BeginInvokeOnMainThread()` from `Microsoft.Maui.ApplicationModel`.
- **Fix**: Replace `Device.BeginInvokeOnMainThread(...)` with `MainThread.BeginInvokeOnMainThread(...)`
- **Additional using required**: `using Microsoft.Maui.ApplicationModel;`

#### Issue 3: Deprecated API - `DependencyService.Get<T>()`
- **Severity**: High
- **Description**: `DependencyService` is the old Xamarin.Forms service locator pattern. .NET MAUI uses dependency injection (DI) via the built-in `IServiceProvider` / `MauiApp.CreateBuilder()` service registration. Services should be injected via constructor injection rather than resolved through a static service locator.
- **Fix**: Accept `ISettingsService` as a constructor parameter, storing it as a private field. This aligns with .NET MAUI's DI-first approach.

#### Issue 4: Wrong Namespace in Platform Specific API - `Xamarin.Forms.PlatformConfiguration.iOS`
- **Severity**: Medium
- **Description**: The platform-specific API `On<Xamarin.Forms.PlatformConfiguration.iOS>()` references the old Xamarin.Forms namespace. In .NET MAUI, the equivalent is `Microsoft.Maui.Controls.PlatformConfiguration.iOS`.
- **Fix**: Replace `Xamarin.Forms.PlatformConfiguration.iOS` with `Microsoft.Maui.Controls.PlatformConfiguration.iOS`
- **Additional using required**: `using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;`

## Fixed Code

```csharp
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
```

## Summary of Changes

| # | Original | Fixed | Reason |
|---|----------|-------|--------|
| 1 | `using Xamarin.Forms;` | `using Microsoft.Maui.Controls;` | Xamarin.Forms namespace does not exist in .NET MAUI |
| 2 | `Device.BeginInvokeOnMainThread(...)` | `MainThread.BeginInvokeOnMainThread(...)` | `Device` class is obsolete; `MainThread` is the MAUI replacement |
| 3 | `DependencyService.Get<ISettingsService>()` | Constructor injection `ISettingsService settingsService` | `DependencyService` is obsolete; MAUI uses built-in DI |
| 4 | `On<Xamarin.Forms.PlatformConfiguration.iOS>()` | `On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()` | Must use MAUI namespace, not Xamarin.Forms |

## Confidence Assessment

All 4 issues identified are well-known breaking changes and deprecations when migrating from Xamarin.Forms to .NET MAUI. These are documented in official Microsoft migration guides:
- `Device` -> `MainThread` / `DeviceInfo` split
- `DependencyService` -> Microsoft.Extensions.DependencyInjection
- Namespace changes from `Xamarin.Forms` -> `Microsoft.Maui.Controls`
