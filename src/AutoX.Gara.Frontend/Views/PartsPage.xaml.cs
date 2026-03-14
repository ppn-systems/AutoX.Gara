// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels;
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

    private async void OnCategoryFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn loại phụ tùng", vm.PartCategoryFilterOptions, idx => vm.PickerCategoryIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;

        // DisplayActionSheet wants an array of strings.
        string[] opts = new string[vm.PartCategoryFilterOptions.Count];
        for (int i = 0; i < vm.PartCategoryFilterOptions.Count; i++)
            opts[i] = vm.PartCategoryFilterOptions[i];

        string pick = await page.DisplayActionSheet("Chọn loại phụ tùng", "Hủy", null, opts);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;

        int idx2 = -1;
        for (int i = 0; i < opts.Length; i++)
        {
            if (opts[i] == pick) { idx2 = i; break; }
        }

        if (idx2 >= 0) vm.PickerCategoryIndex = idx2;
    }

    private async void OnFormCategoryTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Loại phụ tùng", vm.PartCategoryFormOptions, idx => vm.FormPickerCategoryIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;

        string[] opts = new string[vm.PartCategoryFormOptions.Count];
        for (int i = 0; i < opts.Length; i++)
        {
            opts[i] = vm.PartCategoryFormOptions[i];
        }

        string pick = await page.DisplayActionSheet("Loại phụ tùng", "Hủy", null, opts);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;

        int idx2 = Array.IndexOf(opts, pick);
        if (idx2 >= 0) vm.FormPickerCategoryIndex = idx2;
    }

    private async void OnInStockFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Tồn kho", vm.InStockFilterOptions, idx => vm.PickerInStockIndex = idx))
            return;
#endif
        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Tồn kho", "Hủy", null, vm.InStockFilterOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.InStockFilterOptions, pick);
        if (idx2 >= 0) vm.PickerInStockIndex = idx2;
    }

    private async void OnDefectiveFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Tình trạng", vm.DefectiveFilterOptions, idx => vm.PickerDefectiveIndex = idx))
            return;
#endif
        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Tình trạng", "Hủy", null, vm.DefectiveFilterOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.DefectiveFilterOptions, pick);
        if (idx2 >= 0) vm.PickerDefectiveIndex = idx2;
    }

    private async void OnExpiredFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Hạn sử dụng", vm.ExpiredFilterOptions, idx => vm.PickerExpiredIndex = idx))
            return;
#endif
        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Hạn sử dụng", "Hủy", null, vm.ExpiredFilterOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.ExpiredFilterOptions, pick);
        if (idx2 >= 0) vm.PickerExpiredIndex = idx2;
    }

    private async void OnDiscontinuedFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not PartsViewModel vm) return;
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Trạng thái bán", vm.DiscontinuedFilterOptions, idx => vm.PickerDiscontinuedIndex = idx))
            return;
#endif
        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Trạng thái bán", "Hủy", null, vm.DiscontinuedFilterOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.DiscontinuedFilterOptions, pick);
        if (idx2 >= 0) vm.PickerDiscontinuedIndex = idx2;
    }

#if WINDOWS
    private static bool TryShowFlyout(VisualElement? anchor, string title, System.Collections.Generic.IReadOnlyList<string> options, Action<int> onSelected)
    {
        try
        {
            if (anchor?.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
                return false;

            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout
            {
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
            };

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = title, IsEnabled = false });
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            for (int i = 0; i < options.Count; i++)
            {
                int idx = i;
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
