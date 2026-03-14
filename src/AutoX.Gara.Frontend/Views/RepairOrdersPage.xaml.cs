// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
using Microsoft.Maui.Controls;
using System;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class RepairOrdersPage : ContentPage
{
    private readonly RepairOrdersViewModel _vm;

    public RepairOrdersPage()
    {
        InitializeComponent();
        _vm = new RepairOrdersViewModel(
            new RepairOrderService(
                InstanceManager.Instance.GetOrCreateInstance<RepairOrderQueryCache>()));
        BindingContext = _vm;
    }

    public void Initialize(CustomerDto owner, VehicleDto vehicle) => _vm.Initialize(owner, vehicle);
    public void Initialize(CustomerDto owner, InvoiceDto invoice) => _vm.Initialize(owner, invoice);

    private async void OnBackClicked(System.Object sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }

        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }

    private async void OnStatusFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not RepairOrdersViewModel vm)
        {
            return;
        }

#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, "Trạng thái", vm.StatusOptions, idx => vm.PickerStatusIndex = idx))
        {
            return;
        }
#endif

        var page = Application.Current?.MainPage;
        if (page is null)
        {
            return;
        }

        string[] options = vm.StatusOptions;
        string pick = await page.DisplayActionSheet("Trạng thái", "Hủy", null, options);
        if (pick == "Hủy" || string.IsNullOrWhiteSpace(pick))
        {
            return;
        }

        int idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0)
        {
            vm.PickerStatusIndex = idx2;
        }
    }

#if WINDOWS
    private static bool TryShowFlyout(VisualElement? anchor, string title, System.Collections.Generic.IReadOnlyList<string> options, Action<int> onSelected)
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
