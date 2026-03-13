// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class InvoicesViewModel : ObservableObject, System.IDisposable
{
    private readonly InvoiceService _service;
    private System.Threading.CancellationTokenSource? _cts;

    private const System.Int32 DefaultPageSize = 10;

    public InvoicesViewModel(InvoiceService service)
        => _service = service ?? throw new System.ArgumentNullException(nameof(service));

    [ObservableProperty] public partial CustomerDto? Owner { get; set; }

    public System.String PageTitle => Owner is null ? "Hóa đơn" : $"Hóa đơn {Owner.Name}";

    public ObservableCollection<InvoiceDto> Invoices { get; } = [];

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public System.Int32 TotalPages => TotalCount > 0 ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize) : 0;

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    // Form
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial InvoiceDto? SelectedInvoice { get; set; }

    [ObservableProperty] public partial string FormInvoiceNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial System.DateTime FormInvoiceDate { get; set; } = System.DateTime.Today;

    private static readonly TaxRateType[] TaxRateValues = System.Enum.GetValues<TaxRateType>();
    [ObservableProperty] public partial int PickerTaxRateIndex { get; set; } = System.Array.IndexOf(TaxRateValues, TaxRateType.VAT10);
    [ObservableProperty] public partial int PickerDiscountTypeIndex { get; set; } = 0; // None
    [ObservableProperty] public partial decimal FormDiscount { get; set; }
    [ObservableProperty] public partial int PickerPaymentStatusIndex { get; set; } = 0; // Unpaid

    public System.String[] TaxRateOptions { get; } = TaxRateValues.Select(EnumText.Get).ToArray();
    public System.String[] DiscountTypeOptions { get; } = EnumText.GetNames<DiscountType>();
    public System.String[] PaymentStatusOptions { get; } = EnumText.GetNames<PaymentStatus>();

    public void Initialize(CustomerDto owner)
    {
        Owner = owner;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        if (Owner?.CustomerId is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsLoading = true;
        ClearError();

        try
        {
            InvoiceListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                filterCustomerId: Owner.CustomerId.Value,
                ct: ct);

            if (!result.IsSuccess)
            {
                HandleError("Không tải được hóa đơn", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            Invoices.Clear();
            for (System.Int32 i = 0; i < result.Invoices.Count; i++)
            {
                Invoices.Add(result.Invoices[i]);
            }

            TotalCount = result.TotalCount;
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = CurrentPage < TotalPages;
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnCurrentPageChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private void NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
        }
    }
    [RelayCommand]
    private void PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
        }
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        IsEditing = false;
        SelectedInvoice = null;
        FormInvoiceNumber = GenerateInvoiceNumber();
        FormInvoiceDate = System.DateTime.Today;
        PickerTaxRateIndex = System.Array.IndexOf(TaxRateValues, TaxRateType.VAT10);
        PickerDiscountTypeIndex = (System.Int32)DiscountType.None;
        FormDiscount = 0;
        PickerPaymentStatusIndex = (System.Int32)PaymentStatus.Unpaid;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(InvoiceDto inv)
    {
        IsEditing = true;
        SelectedInvoice = inv;
        FormInvoiceNumber = inv.InvoiceNumber;
        FormInvoiceDate = inv.InvoiceDate.ToLocalTime().Date;
        PickerTaxRateIndex = System.Math.Max(0, System.Array.IndexOf(TaxRateValues, inv.TaxRate));
        PickerDiscountTypeIndex = (System.Int32)inv.DiscountType;
        FormDiscount = inv.Discount;
        PickerPaymentStatusIndex = (System.Int32)inv.PaymentStatus;
        IsFormVisible = true;
    }

    [RelayCommand] private void CancelForm() => IsFormVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveAsync()
    {
        if (Owner?.CustomerId is null)
        {
            return;
        }

        if (System.String.IsNullOrWhiteSpace(FormInvoiceNumber))
        {
            HasError = true;
            ErrorMessage = "Số hóa đơn không được để trống.";
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            InvoiceDto packet = new()
            {
                InvoiceId = IsEditing ? SelectedInvoice?.InvoiceId : null,
                CustomerId = Owner.CustomerId.Value,
                InvoiceNumber = FormInvoiceNumber.Trim(),
                InvoiceDate = FormInvoiceDate.ToUniversalTime(),
                PaymentStatus = (PaymentStatus)PickerPaymentStatusIndex,
                TaxRate = TaxRateValues[System.Math.Clamp(PickerTaxRateIndex, 0, TaxRateValues.Length - 1)],
                DiscountType = (DiscountType)PickerDiscountTypeIndex,
                Discount = FormDiscount
            };

            InvoiceWriteResult result = IsEditing
                ? await _service.UpdateAsync(packet)
                : await _service.CreateAsync(packet);

            if (!result.IsSuccess)
            {
                HandleError("Lưu thất bại", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            IsFormVisible = false;
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private static async System.Threading.Tasks.Task OpenTransactionsAsync(InvoiceDto invoice)
    {
        var page = new Views.TransactionsPage();
        page.Initialize(invoice);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task DeleteAsync(InvoiceDto invoice)
    {
        IsLoading = true;
        ClearError();
        try
        {
            InvoiceWriteResult result = await _service.DeleteAsync(invoice);
            if (!result.IsSuccess)
            {
                HandleError("Xóa thất bại", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);
        if (cts is null)
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        catch (System.ObjectDisposedException)
        {
        }

        cts.Dispose();
    }

    private void ClearError() { HasError = false; ErrorMessage = null; }

    private void HandleError(System.String title, System.String message, ProtocolAdvice advice)
    {
        HasError = true;
        ErrorMessage = message;
    }

    private static System.String GenerateInvoiceNumber()
    {
        System.String date = System.DateTime.Now.ToString("yyyyMMdd");
        System.Int32 rand = System.Random.Shared.Next(1000, 9999);
        return $"INV-{date}-{rand}";
    }
}
