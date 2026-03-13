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

        _viewModel = new SuppliersViewModel(new SupplierService(InstanceManager.Instance.GetOrCreateInstance<SupplierQueryCache>()));
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
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () => await _viewModel.LoadCommand.ExecuteAsync(null));
        }
    }
}