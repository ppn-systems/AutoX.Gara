# Eval Transcript: Chat Scroll with CollectionView (with_skill)

## Task

> I have a MAUI chat app where new messages arrive via an ObservableCollection. I need a CollectionView that displays messages and automatically scrolls to the bottom when a new message is added. Messages from me should appear on the right, messages from others on the left. How do I implement this?

---

## Step 1: Read the skill

Read `/Users/davidortinau/work/maui-skills/plugins/maui-skills/skills/maui-collectionview/SKILL.md`.

Key guidance consumed from the skill:

- **DataTemplateSelector** — the skill explains how to subclass `DataTemplateSelector` and wire it up in XAML with `ItemTemplate="{StaticResource ...}"`. This directly maps to the left/right bubble requirement.
- **Scrolling — ScrollTo** — the skill documents `collectionView.ScrollTo(item, position: ScrollToPosition.End, animate: true)` for programmatic scrolling, and lists all `ScrollToPosition` values.
- **ObservableCollection** — the skill explicitly states that `ItemsSource` must be bound to an `ObservableCollection<T>` for the UI to update on add/remove.
- **Common gotchas** — the skill flags that `ItemsSource` and the collection must be updated on the UI thread (`MainThread.BeginInvokeOnMainThread`). Applied this in `ChatViewModel.OnMessagesCollectionChanged`.
- **EmptyView** — wrapped in `ContentView` per the skill's gotcha note.
- **No ViewCell** — used `Border` + `VerticalStackLayout` as the template root, never `ViewCell`.
- **x:DataType on DataTemplate** — applied to both `MyMessageTemplate` and `TheirMessageTemplate` for compiled bindings.
- **ItemSpacing on LinearItemsLayout** — used `LinearItemsLayout` with `ItemSpacing="2"` for compact message spacing.

---

## Step 2: Design decisions

### Message model (`Message.cs`)
Simple POCO with `Text`, `SenderName`, `Timestamp`, and `IsFromMe` (bool). `IsFromMe` drives the template selector.

### ViewModel (`ChatViewModel.cs`)
- `Messages` is `ObservableCollection<Message>`.
- Subscribes to `CollectionChanged` on itself; when `NotifyCollectionChangedAction.Add` fires, it invokes a callback on the UI thread using `MainThread.BeginInvokeOnMainThread`.
- The callback (`_scrollToBottomCallback`) is registered by the code-behind, keeping the ViewModel free of UI dependencies.
- `SendCommand` sends the user's own message; `SimulateIncomingCommand` simulates receiving a message (for demo).
- `MessageText` property re-evaluates `SendCommand.CanExecute` on every change.

### Template selector (`MessageTemplateSelector.cs`)
Subclasses `DataTemplateSelector`, holds two `DataTemplate` properties (`MyMessageTemplate`, `TheirMessageTemplate`), and returns the correct one based on `Message.IsFromMe`.

### XAML page (`ChatPage.xaml`)
- Two `DataTemplate` resources keyed in `ResourceDictionary` with `x:DataType="models:Message"`.
- `MessageTemplateSelector` resource references both templates.
- `CollectionView` uses `ItemTemplate="{StaticResource MessageTemplateSelector}"` and `LinearItemsLayout` with `ItemSpacing="2"`.
- Input bar uses an `Entry` + Send `Button` + simulate-incoming `Button` in a `Grid`.
- Right-aligned bubbles: `Border` placed in `Grid.Column="1"` with spacer `*` column on the left.
- Left-aligned bubbles: `Border` placed in `Grid.Column="0"` with spacer `*` column on the right.
- `EmptyView` wraps content in `ContentView` per the skill gotcha.

### Code-behind (`ChatPage.xaml.cs`)
- Creates `ChatViewModel`, sets `BindingContext`, and calls `SetScrollToBottomCallback(ScrollToBottom)`.
- `ScrollToBottom(Message message)` calls `MessagesCollectionView.ScrollTo(item, ScrollToPosition.End, animate: true)`.
- This pattern keeps scroll logic in the view (where UI controls live) while the ViewModel triggers it via a callback, maintaining separation of concerns.

---

## Step 3: Files produced

| File | Purpose |
|---|---|
| `Message.cs` | Data model |
| `ChatViewModel.cs` | ViewModel with ObservableCollection, send/receive commands, and scroll callback |
| `MessageTemplateSelector.cs` | DataTemplateSelector for left/right bubble routing |
| `ChatPage.xaml` | Full XAML page with CollectionView, DataTemplates, and input bar |
| `ChatPage.xaml.cs` | Code-behind wiring scroll callback |

---

## Skill applicability

The skill was directly applicable and covered every required technique:
- `DataTemplateSelector` for left/right templates
- `ScrollTo` with `ScrollToPosition.End` for auto-scroll
- `ObservableCollection` binding requirement
- `MainThread.BeginInvokeOnMainThread` gotcha
- `EmptyView` `ContentView` wrapper gotcha
- `x:DataType` on `DataTemplate` for compiled bindings
