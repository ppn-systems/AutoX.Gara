// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers.Billings;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Services.Repairs;
using AutoX.Gara.Contracts.Protocol.Billings;
using AutoX.Gara.Contracts.Protocol.Customers;
using AutoX.Gara.Contracts.Protocol.Vehicles;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
using System.Collections.Generic;
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
    private async void OnBackClicked(object? sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }
        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
    private async void OnStatusFilterTapped(Object? sender, TappedEventArgs e)
    {
        if (BindingContext is not RepairOrdersViewModel vm)
        {
            return;
        }
#if WINDOWS
        if (TryShowFlyout(sender as VisualElement, UiTextConfiguration.Current.RepairOrdersPickerStatusText, vm.StatusOptions, idx => vm.PickerStatusIndex = idx))
        {
            return;
        }
#endif
        var page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }
        String[] options = vm.StatusOptions;
        String pick = await page.DisplayActionSheetAsync(UiTextConfiguration.Current.RepairOrdersPickerStatusText, UiTextConfiguration.Current.CommonActionCancelText, null, options);
        if (pick == UiTextConfiguration.Current.CommonActionCancelText || String.IsNullOrWhiteSpace(pick))
        {
            return;
        }
        Int32 idx2 = Array.IndexOf(options, pick);
        if (idx2 >= 0)
        {
            vm.PickerStatusIndex = idx2;
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

