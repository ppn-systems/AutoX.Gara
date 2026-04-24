# Chat App Implementation Transcript

## Task
Implement a .NET MAUI chat page with a CollectionView that auto-scrolls to the
newest message and differentiates visually between messages from the current
user (right-aligned) and messages from others (left-aligned).

---

## Key Decisions

### 1. Message Model (`Message.cs`)
- Kept the model a plain C# class (no `INotifyPropertyChanged`) because
  individual messages are immutable once added to the collection. Only the
  collection itself changes.
- `IsFromMe` (bool) is the single flag that drives all layout branching.
- `FormattedTime` is a computed property so the XAML can bind to it without a
  converter.

### 2. ViewModel (`ChatViewModel.cs`)

**ObservableCollection chosen over List**
`ObservableCollection<Message>` notifies the CollectionView of additions and
removals automatically. No manual `OnPropertyChanged` call is needed when
appending a message.

**MessageAdded event instead of CollectionChanged**
`ObservableCollection.CollectionChanged` fires before the CollectionView has
measured and laid out the new cell, so calling `ScrollTo` inside that handler
often does nothing. Raising a `MessageAdded` event from the ViewModel and
handling it in the code-behind — after the framework has had a chance to render
— is more reliable. The code-behind dispatches to the main thread with
`MainThread.BeginInvokeOnMainThread`, which gives the layout pass time to
complete before the scroll is requested.

**Simulated reply**
`SimulateReply` uses `async void` + `Task.Delay` to mimic an incoming message
1.5 s after the user sends. This exercises the scroll path for messages where
`IsFromMe = false`. The `MessageAdded` event is raised for the reply too, so
the auto-scroll fires symmetrically.

**Command with canExecute**
`SendMessageCommand` is disabled when `NewMessageText` is empty or whitespace,
preventing blank messages. `ChangeCanExecute()` is called every time
`NewMessageText` changes.

### 3. XAML Layout (`ChatPage.xaml`)

**Inline dual-layout pattern instead of DataTemplateSelector**
The standard approach for conditional templates in MAUI is to subclass
`DataTemplateSelector`. That requires an additional `.cs` file and wiring it
into the `CollectionView.ItemTemplate`. An equivalent result can be achieved
inline by placing two `Grid` elements inside the item `DataTemplate` and
toggling `IsVisible` via the `IsFromMe` binding (and its inverse using
`InvertedBoolConverter`). This keeps all presentational logic in XAML and avoids
an extra class.

> **Trade-off:** Both layout trees are instantiated for every cell; only one is
> visible. For very large chat histories a `DataTemplateSelector` would be more
> memory-efficient because only the chosen template's view hierarchy is created.
> For typical chat use-cases (hundreds of messages) the inline approach is
> acceptable.

**Right-alignment for "my" messages**
A two-column `Grid` is used: a `*`-width spacer column on the left pushes the
bubble `VerticalStackLayout` (with `Width="Auto"`) to the right edge. The
bubble has `MaximumWidthRequest="280"` so long messages wrap rather than
spanning the full screen width.

**Left-alignment for others' messages**
Mirror of the above: the `Auto` bubble column is first (Column 0), the `*`
spacer is second (Column 1).

**Frame for chat bubbles**
`Frame` with `CornerRadius="16"` and no border gives the rounded-bubble
appearance common in chat UIs. `HasShadow="False"` avoids the default iOS
shadow that would look out of place.

**Input bar**
A two-column `Grid` at the bottom holds an `Entry` (bound to
`NewMessageText`) and a `Send` button. The `Entry` uses `ReturnType="Send"` and
`ReturnCommand` so the keyboard's Return/Done key also submits.

### 4. Code-Behind (`ChatPage.xaml.cs`)

**ScrollTo on MessageAdded**
```csharp
private void OnMessageAdded(object? sender, Message message)
{
    MainThread.BeginInvokeOnMainThread(() => ScrollToLastMessage());
}
```
`BeginInvokeOnMainThread` posts the scroll request to the end of the main
thread's message queue. By the time it runs, the CollectionView has typically
finished measuring the new cell, making `ScrollTo` effective.

**ScrollTo arguments**
```csharp
MessagesCollectionView.ScrollTo(
    item: lastMessage,
    position: ScrollToPosition.End,
    animate: true);
```
- `item: lastMessage` — targeting the object directly is safer than using an
  index; the CollectionView resolves the index internally.
- `ScrollToPosition.End` — ensures the bottom edge of the last item is aligned
  with the bottom edge of the viewport.
- `animate: true` — provides a smooth scroll that communicates to the user
  that new content has appeared.

**OnAppearing scroll**
`ScrollToLastMessage()` is also called in `OnAppearing` so that when the page
loads with pre-seeded messages the view starts at the bottom, not the top.

**Memory management**
The `MessageAdded` event handler is unsubscribed in `OnDisappearing` to prevent
the ViewModel from holding a reference to the page after navigation.

---

## Limitations / Known Issues

- `InvertedBoolConverter` must be registered in `MauiProgram.cs` or in a
  `ResourceDictionary` at the application level (e.g. `App.xaml`). MAUI ships
  `InvertedBoolConverter` in `Microsoft.Maui.Controls` — it can be referenced
  as `x:StaticResource InvertedBoolConverter` once registered.
- The inline dual-template approach instantiates both view hierarchies per cell.
  For production apps with thousands of messages, replace it with a
  `DataTemplateSelector` subclass.
- `ScrollTo` reliability can vary by platform and by whether the CollectionView
  has finished its first layout pass. If scrolling is unreliable on a specific
  platform, wrapping the call in a short `Task.Delay(50)` before invoking on
  the main thread is a common workaround.
