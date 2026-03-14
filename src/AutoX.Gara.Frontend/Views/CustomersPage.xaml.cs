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

    private async void OnFilterTypeTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn loại khách hàng", vm.FilterTypeOptions, idx => vm.PickerFilterTypeIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Chọn loại khách hàng", "Hủy", null, vm.FilterTypeOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.FilterTypeOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerFilterTypeIndex = idx2;
        }
    }

    private async void OnMembershipTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Chọn hạng", vm.FilterMembershipOptions, idx => vm.PickerMembershipIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Chọn hạng", "Hủy", null, vm.FilterMembershipOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.FilterMembershipOptions, pick);
        if (idx2 >= 0)
        {
            vm.PickerMembershipIndex = idx2;
        }
    }

    private async void OnFormTypeTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Loại khách hàng", vm.FormTypeOptions, idx => vm.FormPickerTypeIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Loại khách hàng", "Hủy", null, vm.FormTypeOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.FormTypeOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormPickerTypeIndex = idx2;
        }
    }

    private async void OnFormMembershipTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Hạng thành viên", vm.FormMembershipOptions, idx => vm.FormPickerMembershipIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Hạng thành viên", "Hủy", null, vm.FormMembershipOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.FormMembershipOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormPickerMembershipIndex = idx2;
        }
    }

    private async void OnFormGenderTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not CustomersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Giới tính", vm.FormGenderOptions, idx => vm.FormPickerGenderIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync("Giới tính", "Hủy", null, vm.FormGenderOptions);
        if (pick == "Hủy" || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        Int32 idx2 = Array.IndexOf(vm.FormGenderOptions, pick);
        if (idx2 >= 0)
        {
            vm.FormPickerGenderIndex = idx2;
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
