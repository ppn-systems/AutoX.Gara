// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

/// <summary>
/// Unified page for managing both spare parts and replacement parts.
/// </summary>
public partial class PartsPage : ContentPage
{
    /// <summary>
    /// Initializes the PartsPage and sets up the unified ViewModel.
    /// </summary>
    public PartsPage()
    {
        InitializeComponent();
        BindingContext = new PartsViewModel(
            new PartService(
                InstanceManager.Instance.GetOrCreateInstance<PartQueryCache>()));
    }

    /// <summary>
    /// Loads data when page appears if not already loaded.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PartsViewModel vm && vm.Parts.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }
}