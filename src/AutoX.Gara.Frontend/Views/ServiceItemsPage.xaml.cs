// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
using System.Linq;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Page for service item management.
/// </summary>
public partial class ServiceItemsPage : ContentPage
{
    /// <summary>
    /// Initializes the ServiceItemsPage and sets up the ViewModel.
    /// </summary>
    public ServiceItemsPage()
    {
        InitializeComponent();
        BindingContext = new ServiceItemsViewModel(
            new ServiceItemService(
                InstanceManager.Instance.GetOrCreateInstance<ServiceItemQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ServiceItemsViewModel vm && vm.ServiceItems.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }

    private async void OnFilterTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not ServiceItemsViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn loại dịch vụ", vm.ServiceTypeFilterOptions, idx => vm.PickerTypeIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.ServiceTypeFilterOptions.ToArray();
        string pick = await page.DisplayActionSheet("Chọn loại dịch vụ", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.PickerTypeIndex = idx2;
    }

    private async void OnFormTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not ServiceItemsViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Loại dịch vụ", vm.ServiceTypeFormOptions, idx => vm.FormTypeIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.ServiceTypeFormOptions.ToArray();
        string pick = await page.DisplayActionSheet("Loại dịch vụ", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.FormTypeIndex = idx2;
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
