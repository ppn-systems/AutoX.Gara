# Implementation Transcript: Recipe Grid with CollectionView

## Task

Build a 2-column recipe grid in .NET MAUI with:
- Recipe photo, name, and prep time per card
- Tap-to-select with visual highlight
- MVVM architecture

## Skill Used

`maui-collectionview` — read SKILL.md and references/collectionview-api.md

## Files Created

- `Recipe.cs` — model
- `RecipesViewModel.cs` — ViewModel
- `RecipesPage.xaml` — XAML page
- `RecipesPage.xaml.cs` — code-behind

---

## Key Decisions

### Model (`Recipe.cs`)

Kept intentionally minimal: `Name`, `PhotoUrl`, and `PrepTime` as plain strings. No `INotifyPropertyChanged` needed since individual recipe properties are not mutated after creation — only the collection and the selected item change.

### ViewModel (`RecipesViewModel.cs`)

- Used `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`), which is the standard MVVM toolkit for .NET MAUI projects.
- `Recipes` is an `ObservableCollection<Recipe>` so the UI reacts to add/remove operations.
- `SelectedRecipe` is an `[ObservableProperty]` bound two-way to `CollectionView.SelectedItem`.
- `SelectRecipeCommand` is wired to `SelectionChangedCommand` — in this simple case it's a no-op handler, but provides an extension point for navigation or side effects (e.g., pushing a detail page).
- Sample data is loaded synchronously in the constructor for simplicity; in a real app this would be an async `LoadAsync` call.

### XAML (`RecipesPage.xaml`)

**Grid layout:** Used `GridItemsLayout` with `Span="2"`, `VerticalItemSpacing="12"`, and `HorizontalItemSpacing="12"` per the reference API docs.

**Compiled bindings:** `x:DataType` is set both on the page (`viewmodels:RecipesViewModel`) and on each `DataTemplate` (`models:Recipe`), satisfying the skill's requirement for compiled bindings throughout.

**Selection:**
- `SelectionMode="Single"` on `CollectionView`
- `SelectedItem="{Binding SelectedRecipe, Mode=TwoWay}"` — TwoWay is required so both the VM and CollectionView stay in sync (e.g., if the VM clears selection programmatically)
- `SelectionChangedCommand` bound to `SelectRecipeCommand` for handling selection side effects

**Visual highlight (selected state):** The `DataTemplate` root `Grid` carries a `VisualStateManager` with `Normal` and `Selected` states. The `Selected` state applies a semi-transparent primary color background. This follows the pattern from the skill reference exactly — the VSM must be on the template root element (the `Grid`), not on the inner `Border`, because `CollectionView` sets the visual state on the root.

**Card design:** A `Border` with `RoundRectangle 12` stroke shape wraps an `Image` (AspectFill, fixed height) and a `VerticalStackLayout` for text. The `Border` provides the card styling while the outer `Grid` owns the selection highlight, keeping concerns separated.

**`AppThemeBinding`** used throughout for Light/Dark mode support on borders, text colors, and selection highlight.

**EmptyView:** A `ContentView`-wrapped `VerticalStackLayout` is used per the skill guidance (custom empty views must be wrapped in `ContentView` to render correctly).

### Code-Behind (`RecipesPage.xaml.cs`)

ViewModel is injected via constructor (dependency injection pattern). `BindingContext` is set in code-behind rather than XAML to support DI registration in `MauiProgram.cs`.

---

## Skill Guidance Applied

| Skill rule | Applied |
|---|---|
| Use `ObservableCollection<T>` | Yes — `Recipes` collection |
| Never use `ViewCell` | Yes — `Grid` is template root |
| Set `x:DataType` on `DataTemplate` | Yes — `models:Recipe` |
| Use `GridItemsLayout` for grids | Yes — `Span="2"` |
| `SelectedItem` with `Mode=TwoWay` for Single selection | Yes |
| `VisualState Name="Selected"` on template root | Yes — on the `Grid` |
| Wrap custom `EmptyView` in `ContentView` | Yes |
