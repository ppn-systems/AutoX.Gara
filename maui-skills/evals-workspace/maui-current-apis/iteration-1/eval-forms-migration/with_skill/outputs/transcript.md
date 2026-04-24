# Eval Transcript: forms-migration (with_skill)

## Task

Migrate a Xamarin.Forms `ProductListPage` XAML file to .NET MAUI 10 APIs using the `maui-current-apis` skill.

## Skill Applied

`maui-current-apis` — Always-on guardrail for .NET MAUI API currency.

## Reasoning Steps

### Step 1 — Detect the Target Framework

No `.csproj` file was provided with the input. The task explicitly states the target is **.NET MAUI 10**, so `net10.0` APIs and deprecation rules from the skill's deprecated API table are applied.

### Step 2 — Detect Library Versions

No `PackageReference` entries were provided. No third-party library APIs (CommunityToolkit, MauiReactor, etc.) are used in this XAML file, so library version detection is not required.

### Step 3 — Verify API Currency (Deprecated Patterns Found)

The following deprecated or Xamarin.Forms-specific patterns were identified in the input XAML:

| # | Pattern Found | Status | Replacement |
|---|---------------|--------|-------------|
| 1 | `xmlns="http://xamarin.com/schemas/2014/forms"` | Xamarin.Forms namespace — not valid in MAUI | `xmlns="http://schemas.microsoft.com/dotnet/2021/maui"` |
| 2 | `<ContentPage.Content>` wrapper element | Not required in MAUI; `ContentPage` accepts a single child directly | Removed |
| 3 | `<StackLayout>` (outer, vertical) | Deprecated — skill says use `VerticalStackLayout` or `HorizontalStackLayout`; the outer wrapper was also redundant since `CollectionView` is the direct content | Removed entirely (CollectionView is now the direct page content) |
| 4 | `<ListView>` with `HasUnevenRows="True"` | `ListView`, `ViewCell`, and related cell types are all **deprecated in .NET 10** per the skill table | Replaced with `CollectionView` |
| 5 | `<ListView.ItemTemplate>` | Part of `ListView` pattern | Replaced with `<CollectionView.ItemTemplate>` |
| 6 | `<ViewCell>` | **Deprecated in .NET 10** per skill table (listed alongside `ListView`) | Removed; `CollectionView.ItemTemplate` accepts a `DataTemplate` directly |
| 7 | `<StackLayout Orientation="Horizontal">` (inner) | Deprecated — skill says use `HorizontalStackLayout` | Replaced with `<HorizontalStackLayout>` |

### Step 4 — Apply Decision Rules

- Used the newest MAUI XML namespace.
- Removed the `ContentPage.Content` property element tag — MAUI `ContentPage` takes its single child as implicit content.
- Replaced `ListView` with `CollectionView`: `CollectionView` requires no `ViewCell` wrapper, supports uneven row heights by default, and is the recommended control for .NET MAUI 10.
- `HasUnevenRows="True"` was dropped: `CollectionView` measures each item's actual size automatically; there is no equivalent property because it is the default behavior.
- Replaced outer `StackLayout` with direct `CollectionView` as page content. A wrapping `VerticalStackLayout` around a `CollectionView` that is the sole child is unnecessary and would break scrolling (the `CollectionView` would have unbounded height inside a stack layout).
- Replaced inner `<StackLayout Orientation="Horizontal">` with `<HorizontalStackLayout>`.
- All `Binding` expressions (`Name`, `Price` with `StringFormat`) are valid in MAUI and were preserved unchanged.

## Changes Summary

| File | Action |
|------|--------|
| `ProductListPage.xaml` | Migrated from Xamarin.Forms to .NET MAUI 10 |

## Input XAML

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

## Output XAML

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

## Skill Effectiveness

The `maui-current-apis` skill provided direct, actionable guidance for every deprecated pattern found:

- The deprecated API table explicitly listed `ListView`, `ViewCell`, and related cell types as deprecated in .NET 10 with `CollectionView` as the replacement.
- The table listed `Compatibility.StackLayout` as deprecated and directed use of `VerticalStackLayout` / `HorizontalStackLayout`, which by extension confirmed that `StackLayout` with `Orientation="Horizontal"` should become `HorizontalStackLayout`.
- The skill's decision rule "Never generate `using Xamarin.Forms`" and the namespace guidance confirmed the XML namespace replacement.
- Without the skill, a model might have retained `ListView`, `ViewCell`, or `StackLayout` patterns that are deprecated in .NET MAUI 10.
