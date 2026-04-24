// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers.Billings;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Frontend.Services.Invoices;
using AutoX.Gara.Contracts.Invoices;
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
    private async void OnBackClicked(object? sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }
        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
    private async void OnFormStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not RepairTasksViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.RepairTasksPickerStatusText, vm.StatusOptions, idx => vm.PickerStatusIndex = idx);
    }
}


