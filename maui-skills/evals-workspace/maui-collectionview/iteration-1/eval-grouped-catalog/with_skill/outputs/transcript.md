# Eval Transcript: Grouped Product Catalog (with_skill)

## Task

> "My .NET MAUI e-commerce app needs a product catalog. Products should be grouped by
> category — each group needs a header showing the category name and item count.
> Individual items show product name, price formatted as currency, and an 'Add to Cart'
> button that fires a command. I also need pull-to-refresh to reload the catalog and an
> empty state message when there are no results."

---

## Skill Applied

Skill file: `maui-collectionview/SKILL.md`

---

## Reasoning Steps

### Step 1 — Read the skill file

The skill file was read in full. Key sections consulted:

- **Grouping** — how to create a group class inheriting from `List<T>`, set `IsGrouped="True"`,
  and use `GroupHeaderTemplate` / `GroupFooterTemplate`.
- **Pull-to-refresh** — wrap `CollectionView` in `RefreshView`, bind `IsRefreshing` and `Command`.
- **EmptyView** — wrap custom empty view content in `ContentView` to avoid layout issues (noted gotcha).
- **Commands in item templates** — use `RelativeSource AncestorType` to reach the ViewModel's
  `AddToCartCommand` from inside the item `DataTemplate`.
- **x:DataType** — set on every `DataTemplate` for compiled bindings.
- **ObservableCollection** — used so the UI updates when the collection is replaced.
- **MainThread** — collection updates applied via `MainThread.BeginInvokeOnMainThread` after async work.

### Step 2 — Design the model layer

**Product.cs**
- Plain model: `Id`, `Name`, `Price` (decimal), `Category`.

**ProductGroup.cs**
- Inherits `List<Product>` as prescribed by the skill.
- Exposes `CategoryName` (used in the header label) and computed `ItemCount` (returns `Count`).

### Step 3 — Design the ViewModel

**ProductCatalogViewModel.cs**
- Implements `INotifyPropertyChanged` manually (no CommunityToolkit dependency assumed).
- `ProductGroups`: `ObservableCollection<ProductGroup>` — bound to the CollectionView.
- `IsRefreshing`: bool — two-way bound to `RefreshView.IsRefreshing`.
- `RefreshCommand`: calls `LoadCatalogAsync`, which simulates a 1.2-second network delay,
  then groups products by `Category`, sorts groups alphabetically, and updates
  `ProductGroups` on the UI thread.
- `AddToCartCommand`: `Command<Product>` — receives the tapped product as the `CommandParameter`.
  Stub implementation writes to console; real apps would call a cart service.
- Sample data covers four categories (Electronics, Footwear, Clothing, Kitchen).

### Step 4 — Design the XAML page

**ProductCatalogPage.xaml**

Structure:
```
ContentPage (x:DataType="ProductCatalogViewModel")
  RefreshView (IsRefreshing, Command=RefreshCommand)
    CollectionView (IsGrouped="True", ItemsSource=ProductGroups)
      GroupHeaderTemplate  -> DataTemplate x:DataType="ProductGroup"
      ItemTemplate         -> DataTemplate x:DataType="Product"
      EmptyView            -> ContentView > VerticalStackLayout
```

GroupHeaderTemplate:
- Two-column `Grid`: category name on the left, item count badge on the right.
- Badge uses a `Border` with `RoundRectangle` corner radius for a pill shape.
- Binds `CategoryName` and `ItemCount` (StringFormat='{0} items').

ItemTemplate:
- `Border` with rounded corners and a light stroke for card appearance.
- Inner `Grid` (2 cols, 2 rows): product name (bold), price (currency formatted via
  `StringFormat='{0:C}'`), and "Add to Cart" button spanning both rows on the right.
- Button `Command` uses `RelativeSource AncestorType={x:Type viewmodels:ProductCatalogViewModel}`
  to reach `AddToCartCommand`; `CommandParameter="{Binding}"` passes the current `Product`.

EmptyView:
- Wrapped in `ContentView` per the skill's gotcha note.
- Displays a bold "No products found" headline and a sub-message prompting the user to refresh.

### Step 5 — Code-behind

**ProductCatalogPage.xaml.cs**
- Instantiates `ProductCatalogViewModel` in the constructor and assigns it to `BindingContext`.
- No other logic; all behavior lives in the ViewModel.

---

## Skill Rules Applied

| Rule | Applied |
|---|---|
| Group class inherits `List<T>` | Yes — `ProductGroup : List<Product>` |
| `IsGrouped="True"` on CollectionView | Yes |
| `x:DataType` on every DataTemplate | Yes — all three DataTemplates have it |
| `ObservableCollection<T>` for live updates | Yes |
| Wrap EmptyView custom content in `ContentView` | Yes |
| UI thread update after async work | Yes — `MainThread.BeginInvokeOnMainThread` |
| `RelativeSource AncestorType` for commands in item template | Yes — AddToCartCommand |
| `RefreshView` wraps `CollectionView` | Yes |
| `IsRefreshing` set to `false` when done | Yes — in `finally` block |
| No `ViewCell` used | Yes — `Border`/`Grid` used as template roots |

---

## Output Files

| File | Purpose |
|---|---|
| `Product.cs` | Product model |
| `ProductGroup.cs` | Grouping wrapper inheriting `List<Product>` |
| `ProductCatalogViewModel.cs` | ViewModel with refresh, grouping, add-to-cart |
| `ProductCatalogPage.xaml` | XAML page with grouped CollectionView |
| `ProductCatalogPage.xaml.cs` | Code-behind (minimal) |
| `transcript.md` | This file |
| `metrics.json` | Evaluation metrics |
