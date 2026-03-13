// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class ReplacementPartsPage : ContentPage
{
    public ReplacementPartsPage()
    {
        InitializeComponent();
        BindingContext = new ReplacementPartsViewModel(
            new ReplacementPartService(
                InstanceManager.Instance.GetOrCreateInstance<ReplacementPartQueryCache>()));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReplacementPartsViewModel vm && vm.Parts.Count == 0)
        {
            _ = vm.LoadCommand.ExecuteAsync(null);
        }
    }
}
