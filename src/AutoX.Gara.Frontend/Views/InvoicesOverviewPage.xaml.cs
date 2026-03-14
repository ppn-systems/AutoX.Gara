// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class InvoicesOverviewPage : ContentPage
{
    private readonly InvoicesOverviewViewModel _vm;

    public InvoicesOverviewPage()
    {
        InitializeComponent();
        _vm = new InvoicesOverviewViewModel(
            new InvoiceService(
                InstanceManager.Instance.GetOrCreateInstance<InvoiceQueryCache>()));
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Keep it simple: cancel pending loads when leaving the page.
        _vm.Dispose();
    }
}

