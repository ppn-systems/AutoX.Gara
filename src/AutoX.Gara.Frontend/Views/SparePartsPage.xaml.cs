// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class SparePartsPage : ContentPage
{
    public SparePartsPage()
    {
        InitializeComponent();
        BindingContext = new SparePartsViewModel(
            new SparePartService(
                InstanceManager.Instance.GetOrCreateInstance<SparePartQueryCache>()));
    }
}
