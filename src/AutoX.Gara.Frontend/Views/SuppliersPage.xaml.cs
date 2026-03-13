// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

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
}