// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Frontend.ViewModels;
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

    private async void OnFilterStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SuppliersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Trạng thái", vm.FilterStatusOptions, idx => vm.PickerStatusIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.FilterStatusOptions.ToArray();
        string pick = await page.DisplayActionSheet("Trạng thái", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.PickerStatusIndex = idx2;
    }

    private async void OnFilterPaymentTermsTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SuppliersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Điều khoản thanh toán", vm.FilterPaymentTermsOptions, idx => vm.PickerPaymentTermsIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.FilterPaymentTermsOptions.ToArray();
        string pick = await page.DisplayActionSheet("Điều khoản thanh toán", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.PickerPaymentTermsIndex = idx2;
    }

    private async void OnFormStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SuppliersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Trạng thái", vm.FormStatusOptions, idx => vm.FormStatusIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.FormStatusOptions.ToArray();
        string pick = await page.DisplayActionSheet("Trạng thái", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.FormStatusIndex = idx2;
    }

    private async void OnFormPaymentTermsTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SuppliersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Điều khoản thanh toán", vm.FormPaymentTermsOptions, idx => vm.FormPaymentTermsIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.FormPaymentTermsOptions.ToArray();
        string pick = await page.DisplayActionSheet("Điều khoản thanh toán", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.FormPaymentTermsIndex = idx2;
    }

    private async void OnNewStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SuppliersViewModel vm)
            return;

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Thay đổi trạng thái", vm.FormStatusOptions, idx => vm.NewStatusIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string[] options = vm.FormStatusOptions.ToArray();
        string pick = await page.DisplayActionSheet("Thay đổi trạng thái", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0) vm.NewStatusIndex = idx2;
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
