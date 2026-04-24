// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers.Billings;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Services.Invoices;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
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
    public void Initialize(InvoiceDto invoice, Boolean autoOpenAddForm = false, Decimal? prefillAmount = null)
        => _vm.Initialize(invoice, autoOpenAddForm, prefillAmount);
    private async void OnBackClicked(object? sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }
        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
    private async void OnPaymentMethodTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not TransactionsViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.TransactionsPickerPaymentMethodText, vm.PaymentMethodOptions, idx => vm.PickerPaymentMethodIndex = idx);
    }
    private async void OnTypeTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not TransactionsViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.TransactionsPickerTypeText, vm.TypeOptions, idx => vm.PickerTypeIndex = idx);
    }
    private async void OnStatusTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not TransactionsViewModel vm)
        {
            return;
        }
        await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.TransactionsPickerStatusText, vm.StatusOptions, idx => vm.PickerStatusIndex = idx);
    }
}
