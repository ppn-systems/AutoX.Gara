# Implementation Transcript: Grouped Product Catalog

## Task
Build a .NET MAUI e-commerce product catalog with grouped CollectionView, pull-to-refresh, and an empty state.

## Skill Used
`maui-collectionview` SKILL.md + `references/collectionview-api.md`

---

## Key Decisions

### 1. ProductGroup inherits from `List<Product>`
The skill's grouping pattern (and the MAUI CollectionView grouping contract) requires the group class to inherit from `List<T>`. This lets CollectionView enumerate items within each group automatically. An `ItemCount` computed property (`=> Count`) exposes the group size for display in the header without storing a separate field.

### 2. `ObservableCollection<ProductGroup>` in the ViewModel
The skill explicitly calls out using `ObservableCollection<T>` so the UI reacts to add/remove at the group level. The full collection is replaced on each refresh (rather than mutating in place) to keep the refresh logic simple and predictable.

### 3. `MainThread.BeginInvokeOnMainThread` for UI updates
The skill flags this as a gotcha: collection mutations must happen on the UI thread. The async `LoadCatalogAsync` method offloads the simulated work, then marshals the `GroupedProducts` assignment and `IsRefreshing = false` back to the main thread.

### 4. `RelativeSource AncestorType` for the Add-to-Cart command
The `AddToCartCommand` lives on `CatalogViewModel`, but the item `DataTemplate` is typed to `Product`. Following the skill's SwipeView pattern, the button uses `RelativeSource AncestorType={x:Type viewmodels:CatalogViewModel}` to reach the ViewModel command, with `CommandParameter="{Binding}"` passing the current `Product` instance.

### 5. Price formatted as currency via `StringFormat='{0:C}'`
Standard .NET composite formatting. The `C` specifier uses the device's current culture, so the currency symbol and decimal separator adapt automatically without any extra converter.

### 6. `EmptyView` wrapped in `ContentView`
The skill's gotcha table notes that custom empty views must be wrapped in `ContentView` to render correctly. The empty state presents a heading and sub-label with instructions to pull-to-refresh.

### 7. `x:DataType` on all DataTemplates (compiled bindings)
Every `DataTemplate` — including `GroupHeaderTemplate` (typed to `ProductGroup`) and `ItemTemplate` (typed to `Product`) — carries an `x:DataType` attribute. The page root carries `x:DataType="viewmodels:CatalogViewModel"`. This satisfies the compiled bindings requirement and catches binding path errors at build time.

### 8. `IsRefreshing` two-way bound to `RefreshView`
`RefreshView` requires `IsRefreshing` to be set back to `false` by the ViewModel when the load completes; otherwise the spinner never stops. The ViewModel sets it to `false` inside `LoadCatalogAsync` after the data is applied.

---

## File Summary

| File | Purpose |
|---|---|
| `Product.cs` | Plain model: Id, Name, Price (decimal), Category |
| `ProductGroup.cs` | `List<Product>` subclass with `CategoryName` and `ItemCount` |
| `CatalogViewModel.cs` | INotifyPropertyChanged ViewModel; `GroupedProducts`, `IsRefreshing`, `RefreshCommand`, `AddToCartCommand` |
| `CatalogPage.xaml` | RefreshView > CollectionView with `IsGrouped=True`, `GroupHeaderTemplate`, `ItemTemplate`, `EmptyView` |
| `CatalogPage.xaml.cs` | Minimal code-behind; sets `BindingContext = new CatalogViewModel()` |
