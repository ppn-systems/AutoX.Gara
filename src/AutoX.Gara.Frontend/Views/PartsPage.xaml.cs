// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.Controllers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Unified page for managing both spare parts and replacement parts.
/// </summary>
public partial class PartsPage : ContentPage
{
    /// <summary>
    /// Initializes the PartsPage and sets up the unified ViewModel.
    /// </summary>
    public PartsPage()
    {
        InitializeComponent();
        BindingContext = new PartsViewModel(
            new PartService(
                InstanceManager.Instance.GetOrCreateInstance<PartQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PartsViewModel vm && vm.Parts.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }

    private async void OnCategoryFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn loại phụ tùng", vm.PartCategoryFilterOptions, idx => vm.PickerCategoryIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        // DisplayActionSheet wants an array of strings.
        String[] opts = new String[vm.PartCategoryFilterOptions.Count];
        for (Int32 i = 0; i < vm.PartCategoryFilterOptions.Count; i++)
        {
            opts[i] = vm.PartCategoryFilterOptions[i];
        }

        String pick = await page.DisplayActionSheetAsync("Chọn loại phụ tùng", "Hủy", null, opts);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = -1;
        for (Int32 i = 0; i < opts.Length; i++)
        {
            if (opts[i] == pick) { idx2 = i; break; }
        }

        if (idx2 >= 0)
        {
            vm.PickerCategoryIndex = idx2;
        }
    }

    private async void OnFormCategoryTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Loại phụ tùng", vm.PartCategoryFormOptions, idx => vm.FormPickerCategoryIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String[] opts = new String[vm.PartCategoryFormOptions.Count];
        for (Int32 i = 0; i < opts.Length; i++)
        {
            opts[i] = vm.PartCategoryFormOptions[i];
        }

        String pick = await page.DisplayActionSheetAsync("Loại phụ tùng", "Hủy", null, opts);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(opts, pick);
        if (idx2 >= 0)
        {
            vm.FormPickerCategoryIndex = idx2;
        }
    }

    private async void OnInStockFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Tồn kho", vm.InStockFilterOptions, idx => vm.PickerInStockIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Tồn kho", "Hủy", null, vm.InStockFilterOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.InStockFilterOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerInStockIndex = idx2;
        }
    }

    private async void OnDefectiveFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Tình trạng", vm.DefectiveFilterOptions, idx => vm.PickerDefectiveIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Tình trạng", "Hủy", null, vm.DefectiveFilterOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.DefectiveFilterOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerDefectiveIndex = idx2;
        }
    }

    private async void OnExpiredFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Hạn sử dụng", vm.ExpiredFilterOptions, idx => vm.PickerExpiredIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Hạn sử dụng", "Hủy", null, vm.ExpiredFilterOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.ExpiredFilterOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerExpiredIndex = idx2;
        }
    }

    private async void OnDiscontinuedFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Trạng thái bán", vm.DiscontinuedFilterOptions, idx => vm.PickerDiscontinuedIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Trạng thái bán", "Hủy", null, vm.DiscontinuedFilterOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.DiscontinuedFilterOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerDiscontinuedIndex = idx2;
        }
    }

#if WINDOWS
    private static Boolean TryShowFlyout(VisualElement? anchor, String title, System.Collections.Generic.IReadOnlyList<String> options, Action<Int32> onSelected)
    {
        try
        {
            if (anchor?.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
            {
                return false;
            }

            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout
            {
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
            };

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = title, IsEnabled = false });
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            for (Int32 i = 0; i < options.Count; i++)
            {
                Int32 idx = i;
                flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
                {
                    Text = options[i],
                    Command = new Command(() => onSelected(idx))
                });
            }

            flyout.ShowAt(fe);
            return true;
        }
        catch
        {
            return false;
        }
    }
#endif
}
