// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Customers;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;

namespace AutoX.Gara.Frontend.Views;

public partial class CustomersPage : ContentPage
{
    public CustomersPage()
    {
        InitializeComponent();
        BindingContext = new CustomersViewModel(new CustomerService(InstanceManager.Instance.GetOrCreateInstance<CustomerQueryCache>()));
    }

    private async void OnFilterTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn loại khách hàng", vm.FilterTypeOptions, idx => vm.PickerFilterTypeIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Chọn loại khách hàng", "Hủy", null, vm.FilterTypeOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.FilterTypeOptions, pick);
        if (idx2 >= 0) vm.PickerFilterTypeIndex = idx2;
    }

    private async void OnMembershipTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn hạng", vm.FilterMembershipOptions, idx => vm.PickerMembershipIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Chọn hạng", "Hủy", null, vm.FilterMembershipOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.FilterMembershipOptions, pick);
        if (idx2 >= 0) vm.PickerMembershipIndex = idx2;
    }

#if WINDOWS
    private static bool TryShowFlyout(VisualElement? anchor, string title, string[] options, Action<int> onSelected)
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

            for (int i = 0; i < options.Length; i++)
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
