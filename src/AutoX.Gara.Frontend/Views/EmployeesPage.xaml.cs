// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Page for employee management.
/// </summary>
public partial class EmployeesPage : ContentPage
{
    /// <summary>
    /// Initializes the EmployeesPage and sets up the ViewModel.
    /// </summary>
    public EmployeesPage()
    {
        InitializeComponent();
        BindingContext = new EmployeesViewModel(
            new EmployeeService(
                InstanceManager.Instance.GetOrCreateInstance<EmployeeQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EmployeesViewModel vm && vm.Employees.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }
}