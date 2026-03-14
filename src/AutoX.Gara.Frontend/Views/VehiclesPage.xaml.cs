// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Customers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Code-behind của VehiclesPage.
/// <para>
/// CustomerDataPacket được truyền qua <see cref="Initialize"/> từ CustomersPage
/// thay vì Shell query parameter, tránh việc phải serialize object qua URL.
/// </para>
/// </summary>
public partial class VehiclesPage : ContentPage
{
    private readonly VehiclesViewModel _vm;

    public VehiclesPage()
    {
        InitializeComponent();
        _vm = new VehiclesViewModel(
            new VehicleService(
                InstanceManager.Instance.GetOrCreateInstance<VehicleQueryCache>()));

        BindingContext = _vm;
    }

    /// <summary>
    /// Gọi từ CustomersPage trước khi navigate, truyền customer context.
    /// </summary>
    public void Initialize(CustomerDto owner) => _vm.Initialize(owner);

    /// <summary>Back button — navigate về CustomersPage.</summary>
    private async void OnBackClicked(System.Object sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }

        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }

    private async void OnFormBrandTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Hãng xe", vm.FormBrandOptions, idx => vm.FormPickerBrandIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Hãng xe", "Hủy", null, vm.FormBrandOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.FormBrandOptions, pick);
        if (idx2 >= 0) vm.FormPickerBrandIndex = idx2;
    }

    private async void OnFormTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Loại xe", vm.FormTypeOptions, idx => vm.FormPickerTypeIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Loại xe", "Hủy", null, vm.FormTypeOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.FormTypeOptions, pick);
        if (idx2 >= 0) vm.FormPickerTypeIndex = idx2;
    }

    private async void OnFormColorTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not VehiclesViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Màu sắc", vm.FormColorOptions, idx => vm.FormPickerColorIndex = idx))
            return;
#endif

        var page = Application.Current?.MainPage;
        if (page is null) return;
        string pick = await page.DisplayActionSheet("Màu sắc", "Hủy", null, vm.FormColorOptions);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick)) return;
        int idx2 = Array.IndexOf(vm.FormColorOptions, pick);
        if (idx2 >= 0) vm.FormPickerColorIndex = idx2;
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
