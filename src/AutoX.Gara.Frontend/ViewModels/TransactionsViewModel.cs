// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Messages;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Shared.Protocol.Billings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nalix.Common.Networking.Protocols;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class TransactionsViewModel : ObservableObject, System.IDisposable
{
    private readonly TransactionService _service;
    private System.Threading.CancellationTokenSource? _cts;

    private const int DefaultPageSize = 20;

    public TransactionsViewModel(TransactionService service)
        => _service = service ?? throw new System.ArgumentNullException(nameof(service));

    [ObservableProperty] public partial InvoiceDto? Invoice { get; set; }

    public string PageTitle => Invoice?.InvoiceNumber is null ? "Thanh toán" : $"Thanh toán {Invoice.InvoiceNumber}";

    public ObservableCollection<TransactionDto> Transactions { get; } = [];

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0 ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize) : 0;

    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; }

    // Form
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial decimal FormAmount { get; set; }
    [ObservableProperty] public partial string FormDescription { get; set; } = string.Empty;
    private static readonly PaymentMethod[] PaymentMethodValues = System.Enum.GetValues<PaymentMethod>();
    private static readonly TransactionStatus[] StatusValues = System.Enum.GetValues<TransactionStatus>();
    private static readonly TransactionType[] TypeValues = System.Enum.GetValues<TransactionType>();

    [ObservableProperty] public partial int PickerPaymentMethodIndex { get; set; } =
        System.Array.IndexOf(PaymentMethodValues, PaymentMethod.Cash);

    [ObservableProperty] public partial int PickerStatusIndex { get; set; } =
        System.Array.IndexOf(StatusValues, TransactionStatus.Completed);

    [ObservableProperty] public partial int PickerTypeIndex { get; set; } =
        System.Array.IndexOf(TypeValues, TransactionType.Revenue);

    public string[] PaymentMethodOptions { get; } = PaymentMethodValues.Select(EnumText.Get).ToArray();
    public string[] StatusOptions { get; } = StatusValues.Select(EnumText.Get).ToArray();
    public string[] TypeOptions { get; } = TypeValues.Select(EnumText.Get).ToArray();

    public void Initialize(InvoiceDto invoice, bool autoOpenAddForm = false, decimal? prefillAmount = null)
    {
        Invoice = invoice;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();

        if (autoOpenAddForm)
        {
            OpenAddForm();
            if (prefillAmount.HasValue && prefillAmount.Value > 0)
            {
                FormAmount = prefillAmount.Value;
            }
        }
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        if (Invoice?.InvoiceId is null)
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
            TransactionListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                filterInvoiceId: Invoice.InvoiceId.Value,
                ct: ct);

            if (!result.IsSuccess)
            {
                HandleError("Không tải được giao dịch", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            Transactions.Clear();
            for (int i = 0; i < result.Transactions.Count; i++)
            {
                Transactions.Add(result.Transactions[i]);
            }

            TotalCount = result.TotalCount;
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
    private void OpenAddForm()
    {
        FormAmount = 0;
        FormDescription = string.Empty;
        PickerPaymentMethodIndex = System.Array.IndexOf(PaymentMethodValues, PaymentMethod.Cash);
        PickerStatusIndex = System.Array.IndexOf(StatusValues, TransactionStatus.Completed);
        PickerTypeIndex = System.Array.IndexOf(TypeValues, TransactionType.Revenue);
        IsFormVisible = true;
    }

    [RelayCommand] private void CancelForm() => IsFormVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveAsync()
    {
        if (Invoice?.InvoiceId is null)
        {
            return;
        }

        if (FormAmount <= 0)
        {
            HasError = true;
            ErrorMessage = "Số tiền phải > 0.";
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            TransactionDto packet = new()
            {
                TransactionId = null,
                InvoiceId = Invoice.InvoiceId.Value,
                Amount = FormAmount,
                Description = FormDescription ?? string.Empty,
                PaymentMethod = PaymentMethodValues[System.Math.Clamp(PickerPaymentMethodIndex, 0, PaymentMethodValues.Length - 1)],
                Status = StatusValues[System.Math.Clamp(PickerStatusIndex, 0, StatusValues.Length - 1)],
                Type = TypeValues[System.Math.Clamp(PickerTypeIndex, 0, TypeValues.Length - 1)],
                TransactionDate = System.DateTime.UtcNow,
                CreatedBy = 0
            };

            TransactionWriteResult result = await _service.CreateAsync(packet);
            if (!result.IsSuccess)
            {
                HandleError("Tạo giao dịch thất bại", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            IsFormVisible = false;

            // Invoice totals changed on server after transaction write.
            WeakReferenceMessenger.Default.Send(new InvoiceTotalsChangedMessage(Invoice.InvoiceId.Value));
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task DeleteAsync(TransactionDto t)
    {
        IsLoading = true;
        ClearError();
        try
        {
            TransactionWriteResult result = await _service.DeleteAsync(t);
            if (!result.IsSuccess)
            {
                HandleError("Xóa thất bại", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            if (Invoice?.InvoiceId is not null)
            {
                WeakReferenceMessenger.Default.Send(new InvoiceTotalsChangedMessage(Invoice.InvoiceId.Value));
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

    private void HandleError(string title, string message, ProtocolAdvice advice)
    {
        HasError = true;
        ErrorMessage = message;
    }
}
