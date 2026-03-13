// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class RepairTasksPage : ContentPage
{
    private readonly RepairTasksViewModel _vm;

    public RepairTasksPage()
    {
        InitializeComponent();
        _vm = new RepairTasksViewModel(
            new RepairTaskService(
                InstanceManager.Instance.GetOrCreateInstance<RepairTaskQueryCache>()),
            new EmployeeService(
                InstanceManager.Instance.GetOrCreateInstance<EmployeeQueryCache>()),
            new ServiceItemService(
                InstanceManager.Instance.GetOrCreateInstance<ServiceItemQueryCache>()));
        BindingContext = _vm;
    }

    public void Initialize(RepairOrderDto repairOrder) => _vm.Initialize(repairOrder);

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
