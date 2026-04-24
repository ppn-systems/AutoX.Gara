// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Contracts.Protocol.Billings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
namespace AutoX.Gara.Frontend.Controllers.Billings;
public sealed partial class InvoicesOverviewViewModel : ObservableObject, IDisposable
{
    private readonly InvoiceService _service;
    private CancellationTokenSource? _cts;
    private const Int32 DefaultPageSize = 20;
    private static readonly PaymentStatus[] PaymentStatusValues = Enum.GetValues<PaymentStatus>();
    public InvoicesOverviewViewModel(InvoiceService service)
        => _service = service ?? throw new ArgumentNullException(nameof(service));
    public String PageTitle => UiTextConfiguration.Current.InvoicesOverviewPageTitleText;
    public ObservableCollection<InvoiceRow> Invoices { get; } = [];
    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial int TotalCount { get; set; }
    public Int32 TotalPages => TotalCount > 0 ? (Int32)Math.Ceiling((Double)TotalCount / DefaultPageSize) : 0;
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }
    // Filters
    [ObservableProperty] public partial string SearchTerm { get; set; } = string.Empty;
    public String[] PaymentStatusOptions { get; } =
        [UiTextConfiguration.Current.CommonFilterAllText, .. PaymentStatusValues.Select(EnumText.Get)];
    [ObservableProperty] public partial int PickerPaymentStatusIndex { get; set; } = 0;
    public String SelectedPaymentStatusText =>
        PaymentStatusOptions[Math.Clamp(PickerPaymentStatusIndex, 0, PaymentStatusOptions.Length - 1)];
    [ObservableProperty] public partial bool UseDateFilter { get; set; } = false;
    [ObservableProperty] public partial DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);
    [ObservableProperty] public partial DateTime ToDate { get; set; } = DateTime.Today;
    private PaymentStatus? SelectedPaymentStatus =>
        PickerPaymentStatusIndex <= 0
            ? null
            : PaymentStatusValues[Math.Clamp(PickerPaymentStatusIndex - 1, 0, PaymentStatusValues.Length - 1)];
    partial void OnPickerPaymentStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedPaymentStatusText));
    public void Start()
    {
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();
    }
    partial void OnCurrentPageChanged(int value) => _ = LoadAsync();
    [RelayCommand]
    public async System.Threading.Tasks.Task ApplyFilterAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }
    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        IsLoading = true;
        ClearError();
        try
        {
            DateTime? from = null;
            DateTime? to = null;
            if (UseDateFilter)
            {
                // Normalize in case user picks From > To.
                DateTime a = FromDate.Date;
                DateTime b = ToDate.Date;
                if (a <= b) { from = a; to = b; }
                else { from = b; to = a; }
            }
            InvoiceListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                filterCustomerId: 0, // 0 = global
                searchTerm: SearchTerm,
                sortBy: AutoX.Gara.Contracts.Enums.InvoiceSortField.InvoiceDate,
                sortDescending: true,
                filterPaymentStatus: SelectedPaymentStatus,
                filterFromDate: from,
                filterToDate: to,
                ct: ct);
            if (!result.IsSuccess)
            {
                HandleError(
                    UiTextConfiguration.Current.InvoicesOverviewErrorLoadFailedText,
                    result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText,
                    result.Advice);
                return;
            }
            Invoices.Clear();
            foreach (InvoiceDto inv in result.Invoices)
            {
                Invoices.Add(new InvoiceRow(inv));
            }
            TotalCount = result.TotalCount;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async System.Threading.Tasks.Task PrevPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }
        CurrentPage--;
        await System.Threading.Tasks.Task.CompletedTask;
    }
    [RelayCommand]
    private async System.Threading.Tasks.Task NextPageAsync()
    {
        if (TotalPages > 0 && CurrentPage >= TotalPages)
        {
            return;
        }
        CurrentPage++;
        await System.Threading.Tasks.Task.CompletedTask;
    }
    [RelayCommand]
    private async System.Threading.Tasks.Task OpenTransactionsAsync(InvoiceRow row)
    {
        if (row?.Dto?.InvoiceId is null)
        {
            return;
        }
        INavigation? nav = Shell.Current?.Navigation ?? Application.Current?.Windows[0].Page?.Navigation;
        if (nav is null)
        {
            return;
        }
        var page = new Views.TransactionsPage();
        page.Initialize(row.Dto);
        await nav.PushAsync(page);
    }
    public void Dispose()
    {
        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);
        if (cts is null)
        {
            return;
        }
        try { cts.Cancel(); } catch (ObjectDisposedException) { }
        cts.Dispose();
    }
    private void ClearError() { HasError = false; ErrorMessage = null; }
    private void HandleError(String title, String message, ProtocolAdvice advice)
    {
        HasError = true;
        ErrorMessage = message;
    }
    public sealed class InvoiceRow
    {
        public InvoiceDto Dto { get; }
        public InvoiceRow(InvoiceDto dto) => Dto = dto;
        public String InvoiceNumber => Dto.InvoiceNumber ?? String.Empty;
        public DateTime InvoiceDate => Dto.InvoiceDate;
        public PaymentStatus PaymentStatus => Dto.PaymentStatus;
        public String PaymentStatusText => EnumText.Get(Dto.PaymentStatus);
        public Decimal TotalAmount => Dto.TotalAmount;
        public Decimal BalanceDue => Dto.BalanceDue;
        public Decimal Subtotal => Dto.Subtotal;
        public Decimal ServiceSubtotal => Dto.ServiceSubtotal;
        public Decimal PartsSubtotal => Dto.PartsSubtotal;
        public Boolean IsFullyPaid => Dto.IsFullyPaid || Dto.PaymentStatus == PaymentStatus.Paid;
    }
}

