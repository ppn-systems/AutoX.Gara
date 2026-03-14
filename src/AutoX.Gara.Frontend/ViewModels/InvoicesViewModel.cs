// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Messages;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class InvoicesViewModel : ObservableObject, System.IDisposable
{
    private readonly InvoiceService _service;
    private readonly RepairOrderService _repairOrderService;
    private System.Threading.CancellationTokenSource? _cts;
    private System.Threading.CancellationTokenSource? _lookupCts;

    private const System.Int32 DefaultPageSize = 10;

    public InvoicesViewModel(
        InvoiceService service,
        RepairOrderService repairOrderService)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
        _repairOrderService = repairOrderService ?? throw new System.ArgumentNullException(nameof(repairOrderService));

        WeakReferenceMessenger.Default.Register<InvoiceTotalsChangedMessage>(this, (_, __) =>
        {
            // Invoice totals depend on Transactions; refresh when they change.
            _ = LoadAsync();
        });
    }

    public sealed record LookupOption(System.Int32 Id, System.String Display);

    public sealed partial class InvoiceRow : ObservableObject
    {
        public InvoiceRow(InvoiceDto dto) => Dto = dto ?? throw new System.ArgumentNullException(nameof(dto));

        public InvoiceDto Dto { get; }

        public System.Int32? InvoiceId => Dto.InvoiceId;
        public System.String InvoiceNumber => Dto.InvoiceNumber;
        public System.DateTime InvoiceDate => Dto.InvoiceDate;

        public PaymentStatus PaymentStatus => Dto.PaymentStatus;
        public System.String PaymentStatusText => EnumText.Get(Dto.PaymentStatus);

        public System.Decimal ServiceSubtotal => Dto.ServiceSubtotal;
        public System.Decimal PartsSubtotal => Dto.PartsSubtotal;
        public System.Decimal Subtotal => Dto.Subtotal;

        public DiscountType DiscountType => Dto.DiscountType;
        public System.Decimal Discount => Dto.Discount;
        public System.Decimal DiscountAmount => Dto.DiscountAmount;

        public TaxRateType TaxRate => Dto.TaxRate;
        public System.String TaxRateText => EnumText.Get(Dto.TaxRate);
        public System.Decimal TaxAmount => Dto.TaxAmount;

        public System.Decimal TotalAmount => Dto.TotalAmount;
        public System.Decimal BalanceDue => Dto.BalanceDue;
        public System.Decimal AmountPaid => Dto.TotalAmount - Dto.BalanceDue;

        [ObservableProperty] public partial System.Boolean IsExpanded { get; set; }
    }

    [ObservableProperty] public partial CustomerDto? Owner { get; set; }

    public System.String PageTitle => Owner is null ? "Hóa đơn" : $"Hóa đơn {Owner.Name}";

    public ObservableCollection<InvoiceRow> Invoices { get; } = [];

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public System.Int32 TotalPages => TotalCount > 0 ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize) : 0;

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool IsLookupLoading { get; set; }
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

    // RepairOrder selector
    public ObservableCollection<LookupOption> RepairOrderOptions { get; } = [];
    public ObservableCollection<LookupOption> FilteredRepairOrderOptions { get; } = [];
    [ObservableProperty] public partial LookupOption? SelectedRepairOrderOption { get; set; }
    [ObservableProperty] public partial bool IsRepairOrderSelectorVisible { get; set; }
    [ObservableProperty] public partial string RepairOrderSearchTerm { get; set; } = string.Empty;

    private readonly System.Collections.Generic.Dictionary<System.Int32, RepairOrderDto> _repairOrderById = [];

    // Preview totals (client-side estimate)
    [ObservableProperty] public partial decimal PreviewServiceSubtotal { get; set; }
    [ObservableProperty] public partial decimal PreviewPartsSubtotal { get; set; }
    [ObservableProperty] public partial decimal PreviewSubtotal { get; set; }
    [ObservableProperty] public partial decimal PreviewDiscountAmount { get; set; }
    [ObservableProperty] public partial decimal PreviewTaxAmount { get; set; }
    [ObservableProperty] public partial decimal PreviewTotalAmount { get; set; }

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
                Invoices.Add(new InvoiceRow(result.Invoices[i]));
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
        SelectedRepairOrderOption = null;
        PreviewServiceSubtotal = 0;
        PreviewPartsSubtotal = 0;
        PreviewSubtotal = 0;
        PreviewDiscountAmount = 0;
        PreviewTaxAmount = 0;
        PreviewTotalAmount = 0;
        IsFormVisible = true;
        _ = LoadRepairOrderLookupsAsync();
    }

    [RelayCommand]
    private void OpenEditForm(InvoiceRow row)
    {
        InvoiceDto inv = row.Dto;
        IsEditing = true;
        SelectedInvoice = inv;
        FormInvoiceNumber = inv.InvoiceNumber;
        FormInvoiceDate = inv.InvoiceDate.ToLocalTime().Date;
        PickerTaxRateIndex = System.Math.Max(0, System.Array.IndexOf(TaxRateValues, inv.TaxRate));
        PickerDiscountTypeIndex = (System.Int32)inv.DiscountType;
        FormDiscount = inv.Discount;
        PickerPaymentStatusIndex = (System.Int32)inv.PaymentStatus;
        SelectedRepairOrderOption = null;
        PreviewServiceSubtotal = inv.ServiceSubtotal;
        PreviewPartsSubtotal = inv.PartsSubtotal;
        PreviewSubtotal = inv.Subtotal;
        PreviewDiscountAmount = inv.DiscountAmount;
        PreviewTaxAmount = inv.TaxAmount;
        PreviewTotalAmount = inv.TotalAmount;
        IsFormVisible = true;
        _ = LoadRepairOrderLookupsAsync(inv.InvoiceId);
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
                Discount = FormDiscount,
                RepairOrderId = SelectedRepairOrderOption?.Id ?? 0
            };

            InvoiceWriteResult result = IsEditing
                ? await _service.UpdateAsync(packet)
                : await _service.CreateAsync(packet);

            if (!result.IsSuccess)
            {
                HandleError("Lưu thất bại", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            // Server now links RepairOrderId (if provided) and recalculates totals.
            if (result.UpdatedEntity is not null)
            {
                PreviewServiceSubtotal = result.UpdatedEntity.ServiceSubtotal;
                PreviewPartsSubtotal = result.UpdatedEntity.PartsSubtotal;
                PreviewSubtotal = result.UpdatedEntity.Subtotal;
                PreviewDiscountAmount = result.UpdatedEntity.DiscountAmount;
                PreviewTaxAmount = result.UpdatedEntity.TaxAmount;
                PreviewTotalAmount = result.UpdatedEntity.TotalAmount;
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
    private static async System.Threading.Tasks.Task OpenTransactionsAsync(InvoiceRow row)
    {
        var page = new Views.TransactionsPage();
        page.Initialize(row.Dto);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private static async System.Threading.Tasks.Task PayNowAsync(InvoiceRow row)
    {
        var page = new Views.TransactionsPage();
        page.Initialize(row.Dto, autoOpenAddForm: true, prefillAmount: row.BalanceDue);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task DeleteAsync(InvoiceRow row)
    {
        IsLoading = true;
        ClearError();
        try
        {
            InvoiceWriteResult result = await _service.DeleteAsync(row.Dto);
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
        WeakReferenceMessenger.Default.UnregisterAll(this);

        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);
        var lookupCts = System.Threading.Interlocked.Exchange(ref _lookupCts, null);
        if (lookupCts is not null)
        {
            try { lookupCts.Cancel(); } catch { }
            lookupCts.Dispose();
        }
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

    [RelayCommand]
    private void ToggleDetails(InvoiceRow row)
    {
        if (row is null)
        {
            return;
        }

        bool next = !row.IsExpanded;
        for (int i = 0; i < Invoices.Count; i++)
        {
            Invoices[i].IsExpanded = false;
        }
        row.IsExpanded = next;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task OpenRepairOrdersAsync(InvoiceRow row)
    {
        if (Owner is null || row?.Dto?.InvoiceId is null)
        {
            return;
        }

        var page = new Views.RepairOrdersPage();
        page.Initialize(Owner, row.Dto);
        await Shell.Current.Navigation.PushAsync(page);
    }

    // ─── RepairOrder Lookup + Preview ────────────────────────────────────────

    partial void OnRepairOrderSearchTermChanged(string value) => RefreshRepairOrderFilter();

    partial void OnSelectedRepairOrderOptionChanged(LookupOption? value)
    {
        if (value is null)
        {
            PreviewServiceSubtotal = 0;
            PreviewPartsSubtotal = 0;
            PreviewSubtotal = 0;
            PreviewDiscountAmount = 0;
            PreviewTaxAmount = 0;
            PreviewTotalAmount = 0;
            return;
        }

        // Fast path: server already provides TotalRepairCost per RepairOrder.
        if (_repairOrderById.TryGetValue(value.Id, out RepairOrderDto? ro))
        {
            PreviewServiceSubtotal = 0;
            PreviewPartsSubtotal = 0;
            PreviewSubtotal = ro.TotalRepairCost;
        }
        else
        {
            PreviewServiceSubtotal = 0;
            PreviewPartsSubtotal = 0;
            PreviewSubtotal = 0;
        }

        ApplyDiscountAndTaxToPreview();
    }

    partial void OnPickerTaxRateIndexChanged(int value)
    {
        if (SelectedRepairOrderOption is not null)
        {
            ApplyDiscountAndTaxToPreview();
        }
    }

    partial void OnPickerDiscountTypeIndexChanged(int value)
    {
        if (SelectedRepairOrderOption is not null)
        {
            ApplyDiscountAndTaxToPreview();
        }
    }

    partial void OnFormDiscountChanged(decimal value)
    {
        if (SelectedRepairOrderOption is not null)
        {
            ApplyDiscountAndTaxToPreview();
        }
    }

    [RelayCommand]
    private void OpenRepairOrderSelector()
    {
        RepairOrderSearchTerm = System.String.Empty;
        RefreshRepairOrderFilter();
        IsRepairOrderSelectorVisible = true;
        if (RepairOrderOptions.Count == 0 && !IsLookupLoading)
        {
            _ = LoadRepairOrderLookupsAsync();
        }
    }

    [RelayCommand] private void CloseRepairOrderSelector() => IsRepairOrderSelectorVisible = false;

    [RelayCommand]
    private void SelectRepairOrder(LookupOption opt)
    {
        SelectedRepairOrderOption = opt;
        IsRepairOrderSelectorVisible = false;
    }

    [RelayCommand]
    private System.Threading.Tasks.Task LoadRepairOrdersAsync()
        => LoadRepairOrderLookupsAsync(IsEditing ? SelectedInvoice?.InvoiceId : null);

    private void RefreshRepairOrderFilter()
    {
        FilteredRepairOrderOptions.Clear();
        System.String term = RepairOrderSearchTerm?.Trim() ?? System.String.Empty;
        for (System.Int32 i = 0; i < RepairOrderOptions.Count; i++)
        {
            var opt = RepairOrderOptions[i];
            if (term.Length == 0 || opt.Display.IndexOf(term, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                FilteredRepairOrderOptions.Add(opt);
            }
        }
    }

    private async System.Threading.Tasks.Task LoadRepairOrderLookupsAsync(System.Int32? editingInvoiceId = null)
    {
        if (Owner?.CustomerId is null)
        {
            return;
        }

        _lookupCts?.Cancel();
        _lookupCts?.Dispose();
        _lookupCts = new System.Threading.CancellationTokenSource();
        var ct = _lookupCts.Token;

        IsLookupLoading = true;
        try
        {
            // Show both "not invoiced" and (when editing) "linked to this invoice" orders.
            var notInvoiced = await _repairOrderService.GetListAsync(
                page: 1,
                pageSize: 100,
                filterCustomerId: Owner.CustomerId.Value,
                filterVehicleId: 0,
                filterInvoiceId: 0,
                ct: ct);

            RepairOrderListResult? linked = null;
            if (editingInvoiceId.HasValue)
            {
                linked = await _repairOrderService.GetListAsync(
                    page: 1,
                    pageSize: 100,
                    filterCustomerId: Owner.CustomerId.Value,
                    filterVehicleId: 0,
                    filterInvoiceId: editingInvoiceId.Value,
                    ct: ct);
            }

            RepairOrderOptions.Clear();
            FilteredRepairOrderOptions.Clear();
            _repairOrderById.Clear();

            void addOrders(System.Collections.Generic.List<RepairOrderDto> items)
            {
                for (System.Int32 i = 0; i < items.Count; i++)
                {
                    var ro = items[i];
                    if (ro.RepairOrderId is null)
                    {
                        continue;
                    }

                    // In the invoice selector we only want "not invoiced" orders.
                    // When editing an invoice, we additionally allow orders already linked to that invoice.
                    if (ro.InvoiceId.HasValue && (!editingInvoiceId.HasValue || ro.InvoiceId.Value != editingInvoiceId.Value))
                    {
                        continue;
                    }

                    System.Int32 id = ro.RepairOrderId.Value;
                    if (_repairOrderById.ContainsKey(id))
                    {
                        continue;
                    }

                    _repairOrderById[id] = ro;
                    System.String display = $"#{id} - {ro.OrderDate.ToLocalTime():dd/MM/yyyy} - {EnumText.Get(ro.Status)} - {ro.TotalRepairCost:N0}";
                    if (ro.InvoiceId.HasValue)
                    {
                        display += $" (INV:{ro.InvoiceId.Value})";
                    }
                    RepairOrderOptions.Add(new LookupOption(id, display));
                }
            }

            if (notInvoiced.IsSuccess && notInvoiced.RepairOrders is not null)
            {
                addOrders(notInvoiced.RepairOrders);
            }
            if (linked?.IsSuccess == true && linked.RepairOrders is not null)
            {
                addOrders(linked.RepairOrders);

                // Preselect linked order for edit.
                var linkedOrder = linked.RepairOrders.FirstOrDefault();
                if (linkedOrder?.RepairOrderId is not null)
                {
                    SelectedRepairOrderOption = RepairOrderOptions.FirstOrDefault(o => o.Id == linkedOrder.RepairOrderId.Value);
                }
            }

            RefreshRepairOrderFilter();
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLookupLoading = false;
        }
    }

    // NOTE: previously we attempted to compute a full breakdown (services/parts) client-side by calling multiple APIs.
    // That caused noticeable lag. We now use RepairOrderDto.TotalRepairCost (provided by server) for instant preview.

    private void ApplyDiscountAndTaxToPreview()
    {
        System.Decimal subtotal = PreviewSubtotal;
        DiscountType dt = (DiscountType)PickerDiscountTypeIndex;
        System.Decimal discountValue = FormDiscount;

        System.Decimal discountAmount = dt == DiscountType.Percentage
            ? subtotal * discountValue / 100m
            : discountValue;

        if (discountAmount < 0)
        {
            discountAmount = 0;
        }
        if (discountAmount > subtotal)
        {
            discountAmount = subtotal;
        }

        TaxRateType tax = TaxRateValues[System.Math.Clamp(PickerTaxRateIndex, 0, TaxRateValues.Length - 1)];
        System.Decimal taxAmount = (subtotal - discountAmount) * ((System.Decimal)tax / 100m);

        PreviewDiscountAmount = discountAmount;
        PreviewTaxAmount = taxAmount;
        PreviewTotalAmount = subtotal - discountAmount + taxAmount;
    }
}
