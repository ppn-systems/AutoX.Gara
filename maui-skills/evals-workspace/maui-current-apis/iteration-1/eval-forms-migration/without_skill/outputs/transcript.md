# Migration Transcript: Xamarin.Forms to .NET MAUI 10

## Task

Migrate a Xamarin.Forms ProductListPage XAML to use current .NET MAUI 10 APIs without skill guidance.

## Input

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.ProductListPage"
             Title="Products">
    <ContentPage.Content>
        <StackLayout>
            <ListView ItemsSource="{Binding Products}"
                      HasUnevenRows="True">
                <ListView.ItemTemplate>
                    <DataTemplate>
                        <ViewCell>
                            <StackLayout Orientation="Horizontal" Padding="10">
                                <Label Text="{Binding Name}" FontAttributes="Bold" />
                                <Label Text="{Binding Price, StringFormat='{0:C}'}" />
                            </StackLayout>
                        </ViewCell>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
        </StackLayout>
    </ContentPage.Content>
</ContentPage>
```

## Analysis of Issues

### 1. XML Namespace (xmlns)
- **Before:** `http://xamarin.com/schemas/2014/forms`
- **After:** `http://schemas.microsoft.com/dotnet/2021/maui`
- The Xamarin.Forms XML namespace must be replaced with the .NET MAUI namespace in every MAUI file.

### 2. ContentPage.Content wrapper
- **Before:** Content was wrapped in `<ContentPage.Content>` property element syntax
- **After:** In .NET MAUI, the direct child of `ContentPage` is implicitly the Content property; the explicit `<ContentPage.Content>` wrapper is not needed (though it still works). Removing it reduces boilerplate.

### 3. Outer StackLayout wrapper
- **Before:** `<StackLayout>` wrapping the `ListView`
- **After:** Removed entirely. The `CollectionView` is the direct child of the page. A single-child wrapper `StackLayout` added no layout value here.

### 4. ListView -> CollectionView
- **Before:** `<ListView HasUnevenRows="True">`
- **After:** `<CollectionView>`
- `ListView` is a legacy control in .NET MAUI. `CollectionView` is the recommended replacement. It handles variable-height rows natively without needing `HasUnevenRows="True"` — that property does not exist on `CollectionView` and is not needed.

### 5. ViewCell wrapper
- **Before:** Items inside `ListView.ItemTemplate` required a `<ViewCell>` wrapper
- **After:** `CollectionView.ItemTemplate` `DataTemplate` accepts any view directly; `ViewCell` is not used and not valid inside `CollectionView`.

### 6. StackLayout with Orientation="Horizontal" -> HorizontalStackLayout
- **Before:** `<StackLayout Orientation="Horizontal" Padding="10">`
- **After:** `<HorizontalStackLayout Padding="10">`
- .NET MAUI introduced `HorizontalStackLayout` and `VerticalStackLayout` as purpose-built, more performant replacements for `StackLayout` with an explicit orientation. `StackLayout` still works in MAUI but `HorizontalStackLayout`/`VerticalStackLayout` are preferred.

## Output

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.ProductListPage"
             Title="Products">
    <CollectionView ItemsSource="{Binding Products}">
        <CollectionView.ItemTemplate>
            <DataTemplate>
                <HorizontalStackLayout Padding="10">
                    <Label Text="{Binding Name}" FontAttributes="Bold" />
                    <Label Text="{Binding Price, StringFormat='{0:C}'}" />
                </HorizontalStackLayout>
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</ContentPage>
```

## Changes Summary

| # | Category | Change |
|---|----------|--------|
| 1 | XML Namespace | `http://xamarin.com/schemas/2014/forms` → `http://schemas.microsoft.com/dotnet/2021/maui` |
| 2 | Page structure | Removed `<ContentPage.Content>` property element wrapper |
| 3 | Layout | Removed unnecessary outer `<StackLayout>` wrapper |
| 4 | Control | `ListView` (with `HasUnevenRows="True"`) → `CollectionView` |
| 5 | Cell | Removed `<ViewCell>` wrapper (not used in `CollectionView`) |
| 6 | Layout | `<StackLayout Orientation="Horizontal">` → `<HorizontalStackLayout>` |

## Notes

- `Label` bindings (`Name`, `Price`) and `FontAttributes` are identical in .NET MAUI; no changes required.
- `StringFormat='{0:C}'` for currency formatting works the same in .NET MAUI.
- The `x:Class` attribute requires no change as it reflects the app's own namespace.
- No code-behind changes were required for these structural XAML changes.
