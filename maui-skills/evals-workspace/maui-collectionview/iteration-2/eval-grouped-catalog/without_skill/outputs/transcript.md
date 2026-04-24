# Transcript: Grouped Product Catalog Implementation

## Task Summary
Build a .NET MAUI e-commerce product catalog featuring:
- Grouped CollectionView (by category)
- Group headers with category name and item count
- Per-item price formatted as currency and an Add to Cart button
- Pull-to-refresh via RefreshView
- EmptyView for the no-results state
- Compiled bindings throughout

---

## Key Decisions

### 1. ProductGroup inherits ObservableCollection<Product>
`ProductGroup` extends `ObservableCollection<Product>` rather than wrapping a list. This is the idiomatic MAUI pattern for grouped CollectionView: the control expects `IEnumerable<IEnumerable<T>>`, and `ObservableCollection` satisfies both the grouping contract and change notification automatically.

`ItemCount` is a computed property (`=> Count`) rather than a stored field, so the header always reflects the true number of items without extra synchronisation code.

### 2. ViewModel implements INotifyPropertyChanged manually
The task did not specify a base class or MVVM framework (e.g., CommunityToolkit.Mvvm), so `INotifyPropertyChanged` was implemented by hand to keep the output dependency-free. In a real project, `[ObservableProperty]` from CommunityToolkit.Mvvm would reduce boilerplate.

### 3. ICommand via Command / Command<T>
`RefreshCommand` uses the parameterless `Command` wrapping an `async` lambda. `AddToCartCommand` uses `Command<Product>` so the product instance is passed directly from `CommandParameter="{Binding .}"` without any casting in the ViewModel.

### 4. Reaching the ViewModel's command from inside the DataTemplate
Inside a `DataTemplate` the `BindingContext` is the data item (`Product`), not the page's `BindingContext`. To bind `AddToCartCommand` to the ViewModel from within the item template, the binding uses:
```xml
Command="{Binding Source={RelativeSource AncestorType={x:Type vm:CatalogViewModel}}, Path=AddToCartCommand}"
```
This uses `RelativeSource` with `AncestorType` to walk up the visual tree and find the ancestor whose binding context is a `CatalogViewModel`. This is the recommended compiled-binding-compatible approach in MAUI (avoids fragile element-name references and works with `x:DataType`).

### 5. Compiled bindings (x:DataType)
- `ContentPage` → `x:DataType="vm:CatalogViewModel"` — covers `RefreshCommand`, `IsRefreshing`, and `ProductGroups`.
- `GroupHeaderTemplate` DataTemplate → `x:DataType="models:ProductGroup"` — covers `CategoryName` and `ItemCount`.
- `ItemTemplate` DataTemplate → `x:DataType="models:Product"` — covers `Name` and `Price`.

Each boundary requires its own `x:DataType` declaration; compiled bindings do not inherit across DataTemplate boundaries.

### 6. Price formatted as currency
`StringFormat='{0:C}'` uses the device's current culture to render the price (e.g., `$129.99` in en-US). This is preferable to hard-coding a currency symbol.

### 7. RefreshView placement
`RefreshView` wraps the entire `CollectionView` as its only child. Setting `IsRefreshing` to `TwoWay` is required: the platform sets it to `true` when the user drags, but the ViewModel must be able to reset it to `false` when loading completes. Forgetting `Mode=TwoWay` causes the spinner to run indefinitely.

### 8. EmptyView
`CollectionView.EmptyView` accepts any `View`, not just a string. A `VerticalStackLayout` was used to compose an icon placeholder, a headline, and a subtitle — giving better UX than a plain label.

### 9. Simulated data
`FetchProductsAsync` returns a static list wrapped in `Task.FromResult` to simulate an async data source. A `Task.Delay(800)` simulates network latency so that the `RefreshView` spinner is visible during testing. Replace this method with a real HTTP or repository call in production.

### 10. Namespace layout
| Namespace | Purpose |
|---|---|
| `EcommerceCatalog.Models` | `Product`, `ProductGroup` |
| `EcommerceCatalog.ViewModels` | `CatalogViewModel` |
| `EcommerceCatalog.Views` | `CatalogPage` |

This matches the standard three-layer MAUI project structure.

---

## Files Produced
| File | Role |
|---|---|
| `Product.cs` | Plain model with Id, Name, Category, Price, ImageUrl |
| `ProductGroup.cs` | Category grouping; inherits ObservableCollection<Product> |
| `CatalogViewModel.cs` | State, commands, and simulated data loading |
| `CatalogPage.xaml` | UI: RefreshView > CollectionView with group headers, item cards, EmptyView |
| `CatalogPage.xaml.cs` | Minimal code-behind; wires BindingContext to ViewModel |
