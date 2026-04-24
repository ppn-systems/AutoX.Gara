# Eval Transcript: Recipe Grid with CollectionView

**Task**: Implement a 2-column recipe grid in .NET MAUI with selection highlighting, using MVVM.

---

## Step 1: Read the SKILL.md

Read `/Users/davidortinau/work/maui-skills/plugins/maui-skills/skills/maui-collectionview/SKILL.md` to understand guidance for implementing CollectionView in .NET MAUI.

**Key guidance extracted:**
- Use `GridItemsLayout` with `Span="2"` for a 2-column grid layout.
- Set `x:DataType` on `DataTemplate` for compiled bindings.
- Use `SelectionMode="Single"` and bind `SelectedItem` with `Mode=TwoWay`.
- Use `VisualStateManager` with `Selected` state on the template root to highlight selected items.
- Bind `ItemsSource` to `ObservableCollection<T>` for live updates.
- Never use `ViewCell` — use `Grid`, `StackLayout`, or any `View` as template root.
- Wrap custom `EmptyView` content in a `ContentView`.

---

## Step 2: Create the Model — `Recipe.cs`

Created a simple `Recipe` model with three properties:
- `Name` (string) — the recipe name
- `PhotoUrl` (string) — URL for the recipe photo
- `PrepTime` (string) — human-readable prep time (e.g., "20 min")

Namespace: `RecipeApp.Models`

---

## Step 3: Create the ViewModel — `RecipesViewModel.cs`

Created `RecipesViewModel` following the MVVM pattern:
- Inherits from `BindableObject` (standard MAUI base for data binding without a DI framework dependency).
- Exposes `Recipes` as `ObservableCollection<Recipe>` with 8 sample items seeded using public placeholder image URLs.
- Exposes `SelectedRecipe` as a bindable property with `OnPropertyChanged()` notification.
- Exposes `RecipeSelectedCommand` (`ICommand`) for handling tap/selection events — wired to `SelectionChangedCommand` on the `CollectionView`.

---

## Step 4: Create the View — `RecipesPage.xaml` and `RecipesPage.xaml.cs`

**XAML (`RecipesPage.xaml`)**:
- `CollectionView` bound to `Recipes` with `SelectionMode="Single"`.
- `SelectedItem` bound `TwoWay` to `SelectedRecipe` on the ViewModel.
- `SelectionChangedCommand` wired to `RecipeSelectedCommand`.
- `GridItemsLayout` with `Span="2"`, `VerticalItemSpacing="12"`, `HorizontalItemSpacing="12"`.
- `DataTemplate` uses `x:DataType="models:Recipe"` for compiled bindings.
- Template root is a `Grid` with 3 rows: photo (160px), recipe name, prep time.
- `VisualStateManager` on the root `Grid` applies a light-blue tint (`#E8F4FD` / `#1A3A4A`) when the item is in the `Selected` state.
- `Image` uses `AspectFill` and a `RoundRectangleGeometry` clip for rounded top corners.
- `EmptyView` wrapped in `ContentView` per the skill's gotcha note.

**Code-behind (`RecipesPage.xaml.cs`)**:
- Sets `BindingContext = new RecipesViewModel()` in the constructor.
- Kept minimal — all logic lives in the ViewModel.

---

## Summary of Files Created

| File | Purpose |
|---|---|
| `Recipe.cs` | Model class for a recipe item |
| `RecipesViewModel.cs` | MVVM ViewModel with sample data, selection state, and command |
| `RecipesPage.xaml` | XAML view — 2-column CollectionView grid with selection highlighting |
| `RecipesPage.xaml.cs` | Code-behind — wires BindingContext |
| `transcript.md` | This file |
| `metrics.json` | Metrics summary |

---

## Skill Guidance Applied

| Skill Rule | Applied? |
|---|---|
| Use `GridItemsLayout` with `Span="2"` for 2-column grid | Yes |
| Set `x:DataType` on `DataTemplate` | Yes |
| Use `SelectionMode="Single"` + `SelectedItem` TwoWay | Yes |
| `VisualStateManager` `Selected` state on template root | Yes |
| `ObservableCollection<T>` for `ItemsSource` | Yes |
| Never use `ViewCell` | Yes (used `Grid`) |
| Wrap `EmptyView` custom content in `ContentView` | Yes |
| `SelectionChangedCommand` for selection event | Yes |
