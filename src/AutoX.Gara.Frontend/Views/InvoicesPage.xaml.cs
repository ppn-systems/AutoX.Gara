// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Controllers.Billings;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.Services.Invoices;
using AutoX.Gara.Frontend.Services.Repairs;
using AutoX.Gara.Contracts.Protocol.Customers;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace AutoX.Gara.Frontend.Views;
public partial class InvoicesPage : ContentPage
{
    private readonly InvoicesViewModel _vm;
    private CancellationTokenSource? _deleteHoldCts;
    private const Int32 DeleteHoldMs = 1200;
    public InvoicesPage()
    {
        InitializeComponent();
        _vm = new InvoicesViewModel(
            new InvoiceService(
                InstanceManager.Instance.GetOrCreateInstance<InvoiceQueryCache>()),
            new RepairOrderService(
                InstanceManager.Instance.GetOrCreateInstance<RepairOrderQueryCache>()),
            new RepairTaskService(
                InstanceManager.Instance.GetOrCreateInstance<RepairTaskQueryCache>()),
            new RepairOrderItemService(
                InstanceManager.Instance.GetOrCreateInstance<RepairOrderItemQueryCache>()),
            new ServiceItemService(
                InstanceManager.Instance.GetOrCreateInstance<ServiceItemQueryCache>()),
            new PartService(
                InstanceManager.Instance.GetOrCreateInstance<PartQueryCache>()));
        BindingContext = _vm;
    }
    public void Initialize(CustomerDto owner) => _vm.Initialize(owner);
    private async void OnBackClicked(object? sender, System.EventArgs e)
    {
        if (Shell.Current?.Navigation is null)
        {
            return;
        }
        try { _deleteHoldCts?.Cancel(); } catch { }
        _deleteHoldCts?.Dispose();
        _deleteHoldCts = null;
        _vm.Dispose();
        await Shell.Current.Navigation.PopAsync();
    }
    private async void OnDeletePressed(object? sender, EventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }
        // DataTemplate button context is the row itself.
        if (btn.BindingContext is not InvoicesViewModel.InvoiceRow row)
        {
            return;
        }
        // Reset any previous hold.
        try { _deleteHoldCts?.Cancel(); } catch { }
        _deleteHoldCts?.Dispose();
        _deleteHoldCts = new CancellationTokenSource();
        CancellationToken token = _deleteHoldCts.Token;
        // Tiny feedback so users know they need to keep holding.
        btn.Text = UiTextConfiguration.Current.InvoicesButtonLoadingText;
        btn.Opacity = 0.92;
        try
        {
            await Task.Delay(DeleteHoldMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch
        {
            return;
        }
        finally
        {
            btn.Text = UiTextConfiguration.Current.InvoicesButtonHoldToDeleteText;
            btn.Opacity = 1;
        }
        if (_vm.RequestDeleteCommand.CanExecute(row))
        {
            _vm.RequestDeleteCommand.Execute(row);
        }
    }
    private void OnDeleteReleased(object? sender, EventArgs e)
    {
        try { _deleteHoldCts?.Cancel(); } catch { }
        if (sender is Button btn)
        {
            btn.Text = UiTextConfiguration.Current.InvoicesButtonHoldToDeleteText;
            btn.Opacity = 1;
        }
    }
    private async void OnTaxRateTapped(Object? sender, TappedEventArgs e)
        => await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.InvoicesPickerTaxRateText, _vm.TaxRateOptions, idx => _vm.PickerTaxRateIndex = idx);
    private async void OnDiscountTypeTapped(Object? sender, TappedEventArgs e)
        => await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.InvoicesPickerDiscountTypeText, _vm.DiscountTypeOptions, idx => _vm.PickerDiscountTypeIndex = idx);
    private async void OnPaymentStatusTapped(Object? sender, TappedEventArgs e)
        => await PickerActionSheetHelper.ShowAsync(sender as VisualElement, UiTextConfiguration.Current.InvoicesPickerPaymentStatusText, _vm.PaymentStatusOptions, idx => _vm.PickerPaymentStatusIndex = idx);
}

