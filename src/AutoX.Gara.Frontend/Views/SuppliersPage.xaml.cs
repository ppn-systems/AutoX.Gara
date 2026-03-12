// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public sealed partial class SuppliersPage : ContentPage
{
    private readonly SuppliersViewModel _viewModel;

    public SuppliersPage()
    {
        InitializeComponent();

        if (InstanceManager.Instance.GetExistingInstance<SuppliersViewModel>() == null)
        {
            SupplierService service = new(InstanceManager.Instance.GetOrCreateInstance<SupplierQueryCache>());
            InstanceManager.Instance.Register<SuppliersViewModel>(new SuppliersViewModel(service));
        }

        _viewModel = InstanceManager.Instance.GetOrCreateInstance<SuppliersViewModel>();
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Triggers initial data load after the page UI is fully ready.
    /// Avoids fire-and-forget in constructor which can silently swallow exceptions.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Only load if collection is empty (avoid reload on back-navigation)
        if (_viewModel.Suppliers.Count == 0)
        {
            _ = _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}