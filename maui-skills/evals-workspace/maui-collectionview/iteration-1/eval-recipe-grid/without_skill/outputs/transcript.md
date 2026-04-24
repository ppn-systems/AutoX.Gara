# Eval Transcript — Recipe Grid (without skill guidance)

**Task:** Build a 2-column recipe grid in .NET MAUI with selection highlighting, using MVVM.

---

## Steps Taken

### Step 1 — Create output directory
Created the target directory:
`/Users/davidortinau/work/maui-skills/plugins/maui-skills/evals-workspace/maui-collectionview/iteration-1/eval-recipe-grid/without_skill/outputs/`

No existing files were present; the directory was empty.

---

### Step 2 — Design the architecture

Decided on the following file structure based on standard .NET MAUI MVVM conventions:

| File | Purpose |
|---|---|
| `Recipe.cs` | Model — plain data class with a computed `PrepTimeDisplay` property |
| `RecipesViewModel.cs` | ViewModel — `ObservableCollection<Recipe>`, `SelectedRecipe` property, `SelectRecipeCommand` |
| `IsSelectedConverter.cs` | `IMultiValueConverter` — compares current item to `SelectedRecipe` for highlight logic |
| `RecipesPage.xaml` | View — `CollectionView` with a `GridItemsLayout` (Span=2) and card `DataTemplate` |
| `RecipesPage.xaml.cs` | Code-behind — assigns `BindingContext = new RecipesViewModel()` |

Key design decisions:
- Used `INotifyPropertyChanged` (manual, no third-party library) so the code compiles with zero additional NuGet packages.
- Used `CollectionView` with `SelectionMode="None"` and a manual `TapGestureRecognizer` + command to keep full control over the toggle-selection behaviour (tapping the same card deselects it).
- Selection highlight is driven by a `MultiBinding` on the `Border.Stroke` property, comparing `{Binding .}` (the current item) to `{Binding SelectedRecipe, Source={RelativeSource AncestorType=...}}`. The `IsSelectedConverter` returns `true`/`false` but Border.Stroke expects a `Brush`, so the converter actually returns the correct `Color` objects (`Colors.DodgerBlue` for selected, `Colors.Transparent` otherwise). *(See correction note below.)*
- The `GridItemsLayout` provides the two-column layout without any additional containers.
- `AppThemeBinding` is used for light/dark mode compatibility on background and text colors.

**Correction note:** The initial design had `IsSelectedConverter` return a `bool`. However, `Border.Stroke` is of type `Brush`, not `bool`, so the converter was updated to return a `Color` (which MAUI auto-converts to a `SolidColorBrush`). The final `IsSelectedConverter.cs` returns `Colors.DodgerBlue` when selected and `Colors.Transparent` when not selected.

*(On reflection, having the converter drive the Stroke color directly is simpler and avoids needing a secondary `IValueConverter` or a `DataTrigger`. This is the pattern used in the final code.)*

---

### Step 3 — Write `Recipe.cs`

Simple POCO with:
- `Id`, `Name`, `PhotoUrl`, `PrepTimeMinutes`, `Category`
- Computed `PrepTimeDisplay` (e.g. "25 min", "1h 30min")

---

### Step 4 — Write `RecipesViewModel.cs`

- Implements `INotifyPropertyChanged` manually.
- `Recipes` is an `ObservableCollection<Recipe>` populated with 10 sample items (using `https://picsum.photos` placeholder images).
- `SelectedRecipe` raises `PropertyChanged` on change.
- `SelectRecipeCommand` (typed `Command<Recipe>`) toggles selection: if the tapped recipe is already selected, it deselects (sets to `null`).

---

### Step 5 — Write `IsSelectedConverter.cs`

`IMultiValueConverter` that:
- Receives `values[0]` = current item, `values[1]` = `SelectedRecipe`.
- Returns `Colors.DodgerBlue` (selected) or `Colors.Transparent` (not selected).
- `ConvertBack` throws `NotImplementedException` (one-way binding).

---

### Step 6 — Write `RecipesPage.xaml`

Key XAML structure:

```xml
<CollectionView ItemsSource="{Binding Recipes}" SelectionMode="None">
    <CollectionView.ItemsLayout>
        <GridItemsLayout Orientation="Vertical" Span="2" ... />
    </CollectionView.ItemsLayout>
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:Recipe">
            <Border Style="{StaticResource CardBorderStyle}">
                <!-- MultiBinding drives Border.Stroke -->
                <Border.Stroke>
                    <MultiBinding Converter="{StaticResource IsSelectedConverter}">
                        <Binding Path="." />
                        <Binding Path="SelectedRecipe"
                                 Source="{RelativeSource AncestorType={x:Type vm:RecipesViewModel}}" />
                    </MultiBinding>
                </Border.Stroke>

                <Border.GestureRecognizers>
                    <TapGestureRecognizer
                        Command="{Binding SelectRecipeCommand,
                                  Source={RelativeSource AncestorType={x:Type vm:RecipesViewModel}}}"
                        CommandParameter="{Binding .}" />
                </Border.GestureRecognizers>

                <Grid RowDefinitions="160,Auto,Auto">
                    <Image  Grid.Row="0" Source="{Binding PhotoUrl}" Aspect="AspectFill" />
                    <Label  Grid.Row="1" Text="{Binding Name}" ... />
                    <HorizontalStackLayout Grid.Row="2">
                        <Label Text="⏱" />
                        <Label Text="{Binding PrepTimeDisplay}" />
                    </HorizontalStackLayout>
                </Grid>
            </Border>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

The `CardBorderStyle` resource sets `StrokeShape="RoundRectangle 12"`, a drop shadow, and `Margin="6"`.

---

### Step 7 — Write `RecipesPage.xaml.cs`

Minimal code-behind; assigns `BindingContext = new RecipesViewModel()`.

---

### Step 8 — Write `transcript.md` and `metrics.json`

Documented all steps and recorded metrics.

---

## Issues / Observations

1. **`IsSelectedConverter` return type** — `Border.Stroke` is a `Brush`, not `bool`. The converter must return a `Color` (MAUI coerces it to a `SolidColorBrush`). This is a subtle point that is easy to get wrong.

2. **`RelativeSource AncestorType` in `DataTemplate`** — Reaching the ViewModel from inside a `CollectionView` `DataTemplate` requires `RelativeSource AncestorType` pointing to the page or the ViewModel type. Because the page's `BindingContext` *is* the ViewModel, binding to `Source={RelativeSource AncestorType={x:Type vm:RecipesViewModel}}` works only if the MAUI visual tree exposes the ViewModel as an ancestor. A more robust alternative is to bind to the page and use `BindingContext.SelectRecipeCommand`. Either approach works; the code uses `AncestorType` of the ViewModel for explicitness.

3. **No CommunityToolkit.Mvvm** — The task did not specify a library, so plain `INotifyPropertyChanged` + `Command<T>` was used. CommunityToolkit source generators would reduce boilerplate but add a dependency.

4. **Images** — Remote URLs (`picsum.photos`) require internet access at runtime. For a production app, bundled resources or a local image service would be preferable.
