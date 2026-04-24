# Eval Transcript: eval-deprecated-device (with_skill)

## Skill Used
`maui-current-apis` — Always-on guardrail for .NET MAUI API currency.

## Task
Fix a `MainPage.cs` that uses `Device.OpenUri()` to open a URL, replacing the deprecated API with the current .NET MAUI equivalent.

---

## Reasoning Steps

### Step 1 — Detect Target Framework
No `.csproj` file was provided with the task. Per the skill's Quick Rule #1, the target framework should always be read from the project file before generating code. Since none was available, the fix is applied using the current recommended API, which is valid across all supported .NET MAUI versions (.NET 8, 9, and 10).

### Step 2 — Detect Library Versions
No `PackageReference` entries were available. The replacement API (`Launcher.OpenAsync`) is part of the built-in `Microsoft.Maui.ApplicationModel` namespace — no additional NuGet packages are required.

### Step 3 — Verify API Currency
The original code uses:
```csharp
Device.OpenUri(new Uri("https://example.com"));
```

Checking the skill's **Deprecated API Table — Device & Platform APIs**:

| Deprecated | Replacement | Notes |
|---|---|---|
| `Device.OpenUri()` | `Launcher.OpenAsync()` | Use `Microsoft.Maui.ApplicationModel.Launcher` |

The `Device` class is **fully deprecated** in .NET MAUI. `Device.OpenUri()` must be replaced with `Launcher.OpenAsync()` from `Microsoft.Maui.ApplicationModel`.

### Step 4 — Apply Decision Rules
- `Launcher.OpenAsync()` is an async method, so the event handler must be changed from `void` to `async void`.
- The `using Microsoft.Maui.ApplicationModel;` directive must be added (or is available via global usings in SDK-style MAUI projects).
- No Xamarin namespaces are used in the corrected code.
- No Compatibility namespace types are introduced.

---

## Input Code

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

### Issues Identified
1. `Device.OpenUri()` — deprecated; the entire `Device` class is deprecated in .NET MAUI.
2. The handler is synchronous (`void`) when the replacement API is asynchronous.

---

## Output Code

```csharp
using Microsoft.Maui.ApplicationModel;

public partial class MainPage : ContentPage
{
    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://example.com"));
    }
}
```

### Changes Made
1. Replaced `Device.OpenUri(new Uri(...))` with `await Launcher.OpenAsync(new Uri(...))`.
2. Added `async` to the event handler signature (required to use `await`).
3. Added `using Microsoft.Maui.ApplicationModel;` to bring `Launcher` into scope.
4. Removed the now-obsolete `// TODO` comment.

---

## Skill Effectiveness Assessment
The skill directly addressed this issue via the **Device & Platform APIs** section of the deprecated API table. The table entry for `Device.OpenUri()` gave the exact replacement (`Launcher.OpenAsync()`) and the correct namespace (`Microsoft.Maui.ApplicationModel`), enabling a precise, confident fix with no ambiguity.
