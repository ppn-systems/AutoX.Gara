// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Customers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class InvoicesPage : ContentPage
{
    private readonly InvoicesViewModel _vm;

    public InvoicesPage()
    {
        InitializeComponent();
        _vm = new InvoicesViewModel(
            new InvoiceService(
                InstanceManager.Instance.GetOrCreateInstance<InvoiceQueryCache>()));
        BindingContext = _vm;
    }

    public void Initialize(CustomerDto owner) => _vm.Initialize(owner);

    private async void OnBackClicked(System.Object sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }

        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
}
