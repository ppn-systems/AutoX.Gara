using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Customers;

using AutoX.Gara.Frontend.Controllers;

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

        if (TryShowFlyout(sender as VisualElement, "Ch?n lo?i kh�ch h�ng", vm.FilterTypeOptions, idx => vm.PickerFilterTypeIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Ch?n lo?i kh�ch h�ng", "H?y", null, vm.FilterTypeOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

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

        if (TryShowFlyout(sender as VisualElement, "Ch?n h?ng", vm.FilterMembershipOptions, idx => vm.PickerMembershipIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Ch?n h?ng", "H?y", null, vm.FilterMembershipOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

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

        if (TryShowFlyout(sender as VisualElement, "Lo?i kh�ch h�ng", vm.FormTypeOptions, idx => vm.FormPickerTypeIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Lo?i kh�ch h�ng", "H?y", null, vm.FormTypeOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

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

        if (TryShowFlyout(sender as VisualElement, "H?ng th�nh vi�n", vm.FormMembershipOptions, idx => vm.FormPickerMembershipIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("H?ng th�nh vi�n", "H?y", null, vm.FormMembershipOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

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

        if (TryShowFlyout(sender as VisualElement, "Gi?i t�nh", vm.FormGenderOptions, idx => vm.FormPickerGenderIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Gi?i t�nh", "H?y", null, vm.FormGenderOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

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

    private static Boolean TryShowFlyout(VisualElement? anchor, String title, IReadOnlyList<String> options, Action<Int32> onSelected)

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