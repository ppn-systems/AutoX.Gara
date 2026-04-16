using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Suppliers;

using AutoX.Gara.Frontend.Controllers;

using Microsoft.Maui.Controls;

using Nalix.Framework.Injection;

using System;

using System.Linq;

namespace AutoX.Gara.Frontend.Views;

public sealed partial class SuppliersPage : ContentPage

{
    public SuppliersPage()

    {
        InitializeComponent();

        BindingContext = new SuppliersViewModel(

            new SupplierService(

                InstanceManager.Instance.GetOrCreateInstance<SupplierQueryCache>()));

    }

    /// <summary>

    /// Triggers initial data load after the page UI is fully ready.

    /// Avoids fire-and-forget in constructor which can silently swallow exceptions.

    /// </summary>

    protected override void OnAppearing()

    {
        base.OnAppearing();

        if (BindingContext is SuppliersViewModel vm && vm.Suppliers.Count == 0)

        {
            _ = vm.LoadCommand.ExecuteAsync(null);

        }

    }

    private async void OnFilterStatusTapped(Object? sender, TappedEventArgs e)

    {
        if (BindingContext is not SuppliersViewModel vm)

        {
            return;

        }

#if WINDOWS

        if (TryShowFlyout(sender as VisualElement, "Tr?ng th�i", vm.FilterStatusOptions, idx => vm.PickerStatusIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String[] options = vm.FilterStatusOptions.ToArray();

        String pick = await page.DisplayActionSheetAsync("Tr?ng th�i", "H?y", null, options);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx2 = Array.IndexOf(options, pick);

        if (idx2 >= 0)

        {
            vm.PickerStatusIndex = idx2;

        }

    }

    private async void OnFilterPaymentTermsTapped(Object? sender, TappedEventArgs e)

    {
        if (BindingContext is not SuppliersViewModel vm)

        {
            return;

        }

#if WINDOWS

        if (TryShowFlyout(sender as VisualElement, "�i?u kho?n thanh to�n", vm.FilterPaymentTermsOptions, idx => vm.PickerPaymentTermsIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String[] options = vm.FilterPaymentTermsOptions.ToArray();

        String pick = await page.DisplayActionSheetAsync("�i?u kho?n thanh to�n", "H?y", null, options);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx2 = Array.IndexOf(options, pick);

        if (idx2 >= 0)

        {
            vm.PickerPaymentTermsIndex = idx2;

        }

    }

    private async void OnFormStatusTapped(Object? sender, TappedEventArgs e)

    {
        if (BindingContext is not SuppliersViewModel vm)

        {
            return;

        }

#if WINDOWS

        if (TryShowFlyout(sender as VisualElement, "Tr?ng th�i", vm.FormStatusOptions, idx => vm.FormStatusIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String[] options = vm.FormStatusOptions.ToArray();

        String pick = await page.DisplayActionSheetAsync("Tr?ng th�i", "H?y", null, options);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx2 = Array.IndexOf(options, pick);

        if (idx2 >= 0)

        {
            vm.FormStatusIndex = idx2;

        }

    }

    private async void OnFormPaymentTermsTapped(Object? sender, TappedEventArgs e)

    {
        if (BindingContext is not SuppliersViewModel vm)

        {
            return;

        }

#if WINDOWS

        if (TryShowFlyout(sender as VisualElement, "�i?u kho?n thanh to�n", vm.FormPaymentTermsOptions, idx => vm.FormPaymentTermsIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String[] options = vm.FormPaymentTermsOptions.ToArray();

        String pick = await page.DisplayActionSheetAsync("�i?u kho?n thanh to�n", "H?y", null, options);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx2 = Array.IndexOf(options, pick);

        if (idx2 >= 0)

        {
            vm.FormPaymentTermsIndex = idx2;

        }

    }

    private async void OnNewStatusTapped(Object? sender, TappedEventArgs e)

    {
        if (BindingContext is not SuppliersViewModel vm)

        {
            return;

        }

#if WINDOWS

        if (TryShowFlyout(sender as VisualElement, "Thay d?i tr?ng th�i", vm.FormStatusOptions, idx => vm.NewStatusIndex = idx))

        {
            return;

        }

#endif

        var page = Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String[] options = vm.FormStatusOptions.ToArray();

        String pick = await page.DisplayActionSheetAsync("Thay d?i tr?ng th�i", "H?y", null, options);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx2 = Array.IndexOf(options, pick);

        if (idx2 >= 0)

        {
            vm.NewStatusIndex = idx2;

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