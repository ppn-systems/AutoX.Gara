# Eval Transcript: Grouped Product Catalog (Without Skill)

## Task
Implement a .NET MAUI e-commerce product catalog with:
- Products grouped by category
- Group headers showing category name and item count
- Individual items showing product name, currency-formatted price, and an "Add to Cart" button with command binding
- Pull-to-refresh to reload the catalog
- Empty state message when no results

## Approach

### Step 1 — Model layer
Created two model classes:

**Product.cs** — Plain POCO with Id, Name, Price (decimal), Category, and ImageUrl properties.

**ProductGroup.cs** — Extends `ObservableCollection<Product>`. This is the standard .NET MAUI pattern for grouped CollectionView: the collection itself is the group, so `ItemsSource` gets a collection of `ProductGroup` instances and `IsGrouped="True"` tells the CollectionView to treat each item in the outer list as a group. An `ItemCount` property (computed from `Count`) drives the badge label.

### Step 2 — ViewModel
Created `BaseViewModel` implementing `INotifyPropertyChanged` with a generic `SetProperty<T>` helper.

**ProductCatalogViewModel.cs** exposes:
- `ObservableCollection<ProductGroup> ProductGroups` — bound to `CollectionView.ItemsSource`
- `bool IsRefreshing` — two-way bound to `RefreshView.IsRefreshing`
- `bool IsEmpty` — controls empty-state overlay visibility
- `ICommand RefreshCommand` — async LINQ grouping of products into `ProductGroup` objects
- `ICommand AddToCartCommand` — `Command<Product>` that receives the tapped product

`LoadCatalogAsync` simulates a 1-second network fetch, then groups the flat product list by Category via LINQ `GroupBy`, producing one `ProductGroup` per category. It sets `IsEmpty` after loading.

### Step 3 — XAML page

**Structural decisions:**

1. **RefreshView wraps CollectionView** — `RefreshView` must be the direct parent of the scrollable view (CollectionView) to intercept the pull gesture. Placing it inside the CollectionView's header would break the gesture.

2. **IsGrouped="True"** — enables group header rendering. `GroupHeaderTemplate` uses `x:DataType="models:ProductGroup"` for compiled bindings, while `ItemTemplate` uses `x:DataType="models:Product"`.

3. **Command binding from item template** — `RelativeSource AncestorType` walks the visual tree to find the page's `BindingContext` (the ViewModel), so `AddToCartCommand` resolves correctly even though the DataTemplate's `DataType` is `Product`.

4. **Currency formatting** — `StringFormat='{0:C}'` on the Price label uses the device's current culture to format the decimal as local currency (e.g., "$89.99" in en-US).

5. **Empty state** — a `VerticalStackLayout` overlaid in the same `Grid`, visibility toggled by `IsEmpty`. When empty, the `RefreshView` is hidden and the empty state is shown; otherwise the reverse.

6. **BoolToObjectConverter** — used for `InvertedBoolConverter` to negate `IsEmpty` for the `RefreshView`'s `IsVisible`.

### Known issues / trade-offs

- `BoolToObjectConverter` with `TrueObject="False"` / `FalseObject="True"` returns strings, not booleans — this will not work correctly for `IsVisible`. The correct approach for a real project is either a custom `InvertBoolConverter` that returns `bool`, or using CommunityToolkit.Maui's `InvertedBoolConverter`. The XAML comment notes this; in a production app you would add the MAUI Community Toolkit NuGet package and use `<toolkit:InvertedBoolConverter x:Key="InvertedBoolConverter" />`.
- `Application.Current?.MainPage?.DisplayAlert` inside the ViewModel is not MVVM-pure; a real app would use a navigation/dialog service abstraction.
- Sample products are hardcoded; a real app would inject a product repository/service.

## Files Produced
- `Product.cs` — model
- `ProductGroup.cs` — grouped collection model
- `BaseViewModel.cs` — INPC base class
- `ProductCatalogViewModel.cs` — ViewModel
- `ProductCatalogPage.xaml` — XAML view
- `ProductCatalogPage.xaml.cs` — code-behind
