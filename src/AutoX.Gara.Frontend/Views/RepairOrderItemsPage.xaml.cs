// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers.Billings;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.Services.Repairs;
using AutoX.Gara.Shared.Protocol.Invoices;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
namespace AutoX.Gara.Frontend.Views;
public partial class RepairOrderItemsPage : ContentPage
{
    private readonly RepairOrderItemsViewModel _vm;
    public RepairOrderItemsPage()
    {
        InitializeComponent();
        _vm = new RepairOrderItemsViewModel(
            new RepairOrderItemService(
                InstanceManager.Instance.GetOrCreateInstance<RepairOrderItemQueryCache>()),
            new PartService(
                InstanceManager.Instance.GetOrCreateInstance<PartQueryCache>()));
        BindingContext = _vm;
    }
    public void Initialize(RepairOrderDto repairOrder) => _vm.Initialize(repairOrder);
    private async void OnBackClicked(object? sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }
        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
}
