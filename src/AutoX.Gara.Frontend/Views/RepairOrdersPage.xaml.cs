// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
using Microsoft.Maui.Controls;
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

    private async void OnBackClicked(System.Object sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }

        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
}
