// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Page for service item management.
/// </summary>
public partial class ServiceItemsPage : ContentPage
{
    /// <summary>
    /// Initializes the ServiceItemsPage and sets up the ViewModel.
    /// </summary>
    public ServiceItemsPage()
    {
        InitializeComponent();
        BindingContext = new ServiceItemsViewModel(
            new ServiceItemService(
                InstanceManager.Instance.GetOrCreateInstance<ServiceItemQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ServiceItemsViewModel vm && vm.ServiceItems.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }
}