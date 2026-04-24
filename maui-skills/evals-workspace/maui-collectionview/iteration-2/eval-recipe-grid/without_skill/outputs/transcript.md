# Recipe Grid Implementation Transcript

## Task Summary

Implemented a 2-column recipe grid in .NET MAUI with item selection and visual highlighting, using MVVM architecture and compiled bindings.

---

## Files Created

| File | Purpose |
|------|---------|
| `Recipe.cs` | Model class |
| `RecipesViewModel.cs` | ViewModel with selection logic |
| `RecipesPage.xaml` | XAML UI with 2-column grid |
| `RecipesPage.xaml.cs` | Code-behind (minimal) |

---

## Key Decisions

### 1. Model: `Recipe.cs`

- Kept as a plain C# class (not `ObservableObject`) because individual recipe properties do not change at runtime — only the `SelectedRecipe` reference in the ViewModel changes.
- Added a computed `PrepTimeDisplay` property that formats minutes into a human-friendly string ("45 min", "1h 30m"), avoiding any converter requirement in XAML.

### 2. ViewModel: `RecipesViewModel.cs`

- Implemented `INotifyPropertyChanged` manually (without a base class or toolkit) to keep the sample self-contained.
- Used `ObservableCollection<Recipe>` for `Recipes` so the `CollectionView` responds to future add/remove operations automatically.
- `SelectedRecipe` is a nullable `Recipe?` property. Setting it triggers `OnPropertyChanged`, which causes the `DataTrigger` in each card to re-evaluate.
- `SelectRecipeCommand` is a `Command<Recipe>` that toggles selection off if the same item is tapped twice, and selects the new item otherwise.
- Sample data is loaded synchronously in the constructor via `LoadRecipes()`. In a production app this would be async, fetching from a local database or remote API.

### 3. Page: `RecipesPage.xaml`

**Layout structure:**
- Top-level `Grid` with two rows: a page header `Label` and the `CollectionView`.
- `CollectionView` uses `GridItemsLayout` with `Span="2"` for the 2-column layout, with 10pt horizontal and vertical item spacing.

**Item template:**
- Each card is a `Border` with `RoundRectangle 12` for rounded corners and a `Shadow` for card-style elevation.
- Inside the `Border`, a `Grid` with three rows holds the photo (`Image`), the recipe name (`Label`), and the prep time row (`HorizontalStackLayout` + `Label`).
- The photo uses `Aspect="AspectFill"` to fill the fixed 130pt height without distortion, clipped by a `RoundRectangleGeometry` that rounds only the top corners.

**Selection highlighting:**
- `CollectionView.SelectionMode="Single"` is set, and `SelectedItem` is two-way bound to `ViewModel.SelectedRecipe`. This keeps the CollectionView's own selection state in sync.
- A `DataTrigger` on the card `Border` watches `SelectedRecipe` in the ancestor ViewModel via `RelativeSource AncestorType`. When the bound item matches the current card's binding context, the trigger fires two `Setter`s:
  - `Stroke` changes to an accent orange color (different tones for light/dark theme).
  - `BackgroundColor` shifts to a tinted background for additional contrast.
- This approach avoids a custom `IValueConverter` or any code-behind logic for visual feedback.

**Compiled bindings:**
- `x:DataType="vm:RecipesViewModel"` is set on the `ContentPage` root.
- `x:DataType="models:Recipe"` is set on the `DataTemplate` so all bindings inside the item template are compiled at build time, improving runtime performance and enabling build-time binding error detection.

**Command binding inside DataTemplate:**
- `TapGestureRecognizer.Command` uses `RelativeSource AncestorType={x:Type vm:RecipesViewModel}` to reach the ViewModel's `SelectRecipeCommand` from within the item template, where `BindingContext` is a `Recipe`. This is the standard MAUI pattern for this scenario.

**Theme support:**
- All colors use `AppThemeBinding` with explicit `Light` and `Dark` values, so the UI adapts automatically to the device theme.

**Empty state:**
- `CollectionView.EmptyView` shows a friendly message when `Recipes` is empty, preventing a blank screen.

**Icon approach:**
- The clock icon next to prep time uses a FontAwesome Unicode glyph (`&#xf017;`). This requires `FontAwesome` to be registered as a font in `MauiProgram.cs`. The font glyph is colored with the accent orange to reinforce the branding. Alternatively, an image asset (`clock_icon.png`) can be substituted by changing the `Label` to an `Image`.

### 4. Code-behind: `RecipesPage.xaml.cs`

- Minimal: receives the `RecipesViewModel` via constructor injection (compatible with MAUI's dependency injection in `MauiProgram.cs`) and assigns it to `BindingContext`.
- No logic lives in code-behind; all behavior is in the ViewModel.

---

## Integration Notes

To wire this up in a real project:

**MauiProgram.cs** — register services:
```csharp
builder.Services.AddTransient<RecipesViewModel>();
builder.Services.AddTransient<RecipesPage>();
```

**AppShell.xaml** — add a route or `ShellContent`:
```xml
<ShellContent Title="Recipes" ContentTemplate="{DataTemplate views:RecipesPage}" Route="recipes" />
```

**Font registration** (if using FontAwesome glyph):
```csharp
.ConfigureFonts(fonts =>
{
    fonts.AddFont("FontAwesome6Free-Regular-400.otf", "FontAwesome");
})
```

---

## Known Limitations / Trade-offs

| Area | Decision | Alternative |
|------|----------|-------------|
| Selection trigger | `DataTrigger` comparing `SelectedRecipe` to each item | `VisualStateManager` with `Selected` state — cleaner but requires CollectionView to own the selection state exclusively |
| Icons | FontAwesome glyph or image asset | Material Icons or a custom SVG via `Microsoft.Maui.Graphics` |
| Images | Remote URL via `picsum.photos` | Local embedded resources or a caching library like `FFImageLoading` |
| ViewModel base | Manual `INotifyPropertyChanged` | `CommunityToolkit.Mvvm` `ObservableObject` + `[ObservableProperty]` for less boilerplate |
| Data loading | Synchronous hardcoded list | Async `InitializeAsync` with a loading indicator |
