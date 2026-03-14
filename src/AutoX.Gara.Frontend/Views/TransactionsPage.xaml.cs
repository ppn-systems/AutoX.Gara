// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.ViewModels;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Frontend.Views;

public partial class TransactionsPage : ContentPage
{
    private readonly TransactionsViewModel _vm;

    public TransactionsPage()
    {
        InitializeComponent();
        _vm = new TransactionsViewModel(
            new TransactionService(
                InstanceManager.Instance.GetOrCreateInstance<TransactionQueryCache>()));
        BindingContext = _vm;
    }

    public void Initialize(InvoiceDto invoice, bool autoOpenAddForm = false, decimal? prefillAmount = null)
        => _vm.Initialize(invoice, autoOpenAddForm, prefillAmount);

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
