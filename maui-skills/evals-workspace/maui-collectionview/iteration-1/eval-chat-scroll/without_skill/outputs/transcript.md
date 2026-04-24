# Eval Transcript – Chat Auto-Scroll (without skill)

## Task

Implement a MAUI chat page where:
- Messages are stored in an `ObservableCollection<Message>`.
- A `CollectionView` displays the messages.
- The view automatically scrolls to the bottom whenever a new message is added.
- Messages from the current user appear right-aligned (blue bubble).
- Messages from others appear left-aligned (grey bubble).

## Approach

### No skill guidance was used. All decisions were made from general MAUI / .NET knowledge.

---

## Files Produced

| File | Purpose |
|---|---|
| `Message.cs` | Plain model class with `Text`, `SenderName`, `IsMe`, `Timestamp` properties |
| `ChatViewModel.cs` | ViewModel with `ObservableCollection<Message>`, `SendMessageCommand`, scroll delegate, simulated reply |
| `ChatPage.xaml` | CollectionView with inline `DataTrigger`-based template switching |
| `ChatPage.xaml.cs` | Code-behind: subscribes to `CollectionChanged`, calls `CollectionView.ScrollTo` on main thread |
| `MauiProgram.cs` | DI registration of ViewModel and Page |
| `AppShell.xaml` | Shell with a single `ShellContent` pointing at `ChatPage` |

---

## Key Implementation Decisions

### 1. Scroll-to-bottom mechanism

`CollectionView.ScrollTo(item, position: ScrollToPosition.End, animate: true)` is the correct MAUI API.
The call must happen on the main thread, so it is wrapped in `MainThread.BeginInvokeOnMainThread`.

Two trigger points are wired up:
- The ViewModel calls a registered `Action` delegate immediately after adding a message to `Messages`.
- The code-behind also subscribes to `ObservableCollection.CollectionChanged` so that messages arriving from any external source (push, background service, etc.) also trigger a scroll.

An initial scroll occurs in `OnAppearing` with a 100 ms delay to allow the first layout pass to complete before scrolling.

### 2. Left / right bubble alignment

MAUI's `CollectionView` does not expose a `DataTemplateSelector` property in XAML, but one can be assigned in code-behind. The simpler XAML-only approach used here is a `ContentView` with two `DataTrigger`s that swap `Content` based on `IsMe`. This avoids requiring a separate C# `DataTemplateSelector` class while keeping all layout in XAML.

An alternative (and more performant) approach would be to subclass `DataTemplateSelector`:

```csharp
public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate MyMessageTemplate { get; set; }
    public DataTemplate TheirMessageTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        => item is Message { IsMe: true } ? MyMessageTemplate : TheirMessageTemplate;
}
```

and assign it to `CollectionView.ItemTemplate` via `<CollectionView.ItemTemplate><local:MessageTemplateSelector .../></CollectionView.ItemTemplate>`.

### 3. ViewModel / View decoupling for scrolling

The ViewModel does not reference any MAUI types. Instead it exposes `RegisterScrollToBottomAction(Action)`. The `ChatPage` code-behind provides the concrete lambda that calls the UI API. This keeps the ViewModel fully unit-testable.

### 4. Seeded data and simulated reply

`ChatViewModel` seeds four messages so the UI is non-empty on launch and simulates a 1.5-second reply after each sent message, demonstrating that the auto-scroll works for both outgoing and incoming messages.

---

## Known Limitations / Issues

1. **DataTrigger template switching**: Setting `Content` from a `DataTrigger` `Setter` is technically replacing the entire visual tree per item, which is less efficient than a proper `DataTemplateSelector`. For production use, prefer the selector approach.

2. **`Frame` is deprecated**: `Frame` was deprecated in .NET MAUI in favour of `Border`. The code uses `Frame` for familiarity; replace with `Border` + `StrokeShape="RoundRectangle 16"` for forward compatibility.

3. **Initial scroll delay**: The 100 ms hard-coded delay in `OnAppearing` is a workaround for layout timing. A more robust approach is to listen to `SizeChanged` on the `CollectionView` and scroll only once it has a non-zero height.

4. **No `x:DataType` on the outer `DataTemplate`**: Because the `DataTrigger`-based approach places two different layout trees inside one `DataTemplate`, the compiled bindings (`x:DataType`) on the inner views bind correctly to `Message`; the outer `ContentView` does not have `x:DataType` set, which suppresses compiled binding warnings but slightly reduces performance.
