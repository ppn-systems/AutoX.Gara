// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Results.Parts;
using AutoX.Gara.Frontend.Results.ServiceItems;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Inventory;
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
    private readonly RepairOrderService _repairOrderService;
    private readonly RepairTaskService _repairTaskService;
    private readonly RepairOrderItemService _repairOrderItemService;
    private readonly ServiceItemService _serviceItemService;
    private readonly PartService _partService;
    private System.Threading.CancellationTokenSource? _cts;
    private System.Threading.CancellationTokenSource? _lookupCts;

    private const System.Int32 DefaultPageSize = 10;

    public InvoicesViewModel(
        InvoiceService service,
        RepairOrderService repairOrderService,
        RepairTaskService repairTaskService,
        RepairOrderItemService repairOrderItemService,
        ServiceItemService serviceItemService,
        PartService partService)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
        _repairOrderService = repairOrderService ?? throw new System.ArgumentNullException(nameof(repairOrderService));
        _repairTaskService = repairTaskService ?? throw new System.ArgumentNullException(nameof(repairTaskService));
        _repairOrderItemService = repairOrderItemService ?? throw new System.ArgumentNullException(nameof(repairOrderItemService));
        _serviceItemService = serviceItemService ?? throw new System.ArgumentNullException(nameof(serviceItemService));
        _partService = partService ?? throw new System.ArgumentNullException(nameof(partService));
    }

    public sealed record LookupOption(System.Int32 Id, System.String Display);

    [ObservableProperty] public partial CustomerDto? Owner { get; set; }

    public System.String PageTitle => Owner is null ? "Hóa đơn" : $"Hóa đơn {Owner.Name}";

    public ObservableCollection<InvoiceDto> Invoices { get; } = [];

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
    private readonly System.Collections.Generic.Dictionary<System.Int32, System.Decimal> _servicePriceById = [];
    private readonly System.Collections.Generic.Dictionary<System.Int32, System.Decimal> _partPriceById = [];

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

            // Link to RepairOrder if user selected one, then trigger server-side Recalculate()
            InvoiceDto? confirmed = result.UpdatedEntity;
            System.Int32? invoiceId = confirmed?.InvoiceId ?? packet.InvoiceId;
            if (invoiceId.HasValue && SelectedRepairOrderOption is not null)
            {
                await LinkInvoiceToRepairOrderAsync(invoiceId.Value, SelectedRepairOrderOption.Id).ConfigureAwait(false);

                // Trigger a recalculation now that the RepairOrder has InvoiceId set.
                InvoiceDto recalcPacket = new()
                {
                    InvoiceId = invoiceId.Value,
                    CustomerId = Owner.CustomerId.Value,
                    InvoiceNumber = packet.InvoiceNumber,
                    InvoiceDate = packet.InvoiceDate,
                    PaymentStatus = packet.PaymentStatus,
                    TaxRate = packet.TaxRate,
                    DiscountType = packet.DiscountType,
                    Discount = packet.Discount
                };

                InvoiceWriteResult recalc = await _service.UpdateAsync(recalcPacket).ConfigureAwait(false);
                if (recalc.IsSuccess && recalc.UpdatedEntity is not null)
                {
                    PreviewServiceSubtotal = recalc.UpdatedEntity.ServiceSubtotal;
                    PreviewPartsSubtotal = recalc.UpdatedEntity.PartsSubtotal;
                    PreviewSubtotal = recalc.UpdatedEntity.Subtotal;
                    PreviewDiscountAmount = recalc.UpdatedEntity.DiscountAmount;
                    PreviewTaxAmount = recalc.UpdatedEntity.TaxAmount;
                    PreviewTotalAmount = recalc.UpdatedEntity.TotalAmount;
                }
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

        _ = RecalculatePreviewAsync(value.Id);
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
        RepairOrderSearchTerm = string.Empty;
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
        string term = RepairOrderSearchTerm?.Trim() ?? string.Empty;
        for (int i = 0; i < RepairOrderOptions.Count; i++)
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
                for (int i = 0; i < items.Count; i++)
                {
                    var ro = items[i];
                    if (ro.RepairOrderId is null)
                    {
                        continue;
                    }

                    int id = ro.RepairOrderId.Value;
                    if (_repairOrderById.ContainsKey(id))
                    {
                        continue;
                    }

                    _repairOrderById[id] = ro;
                    string display = $"#{id} - {ro.OrderDate.ToLocalTime():dd/MM/yyyy} - {EnumText.Get(ro.Status)}";
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

    private async System.Threading.Tasks.Task EnsurePriceLookupsAsync(System.Threading.CancellationToken ct)
    {
        if (_servicePriceById.Count == 0)
        {
            ServiceItemListResult services = await _serviceItemService.GetListAsync(page: 1, pageSize: 200, ct: ct).ConfigureAwait(false);
            if (services.IsSuccess)
            {
                for (int i = 0; i < services.ServiceItems.Count; i++)
                {
                    var s = services.ServiceItems[i];
                    if (s.ServiceItemId.HasValue && s.ServiceItemId.Value > 0)
                    {
                        _servicePriceById[s.ServiceItemId.Value] = s.UnitPrice;
                    }
                }
            }
        }

        if (_partPriceById.Count == 0)
        {
            PartListResult parts = await _partService.GetListAsync(page: 1, pageSize: 200, ct: ct).ConfigureAwait(false);
            if (parts.IsSuccess)
            {
                for (int i = 0; i < parts.Parts.Count; i++)
                {
                    PartDto p = parts.Parts[i];
                    if (p.PartId.HasValue && p.PartId.Value > 0)
                    {
                        _partPriceById[p.PartId.Value] = p.SellingPrice;
                    }
                }
            }
        }
    }

    private async System.Threading.Tasks.Task RecalculatePreviewAsync(int repairOrderId)
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
            await EnsurePriceLookupsAsync(ct).ConfigureAwait(false);

            // Tasks
            decimal serviceSubtotal = 0;
            int page = 1;
            while (true)
            {
                var tasks = await _repairTaskService.GetListAsync(
                    page: page,
                    pageSize: 200,
                    filterRepairOrderId: repairOrderId,
                    ct: ct).ConfigureAwait(false);

                if (!tasks.IsSuccess)
                {
                    break;
                }

                for (int i = 0; i < tasks.RepairTasks.Count; i++)
                {
                    var t = tasks.RepairTasks[i];
                    if (_servicePriceById.TryGetValue(t.ServiceItemId, out var price))
                    {
                        serviceSubtotal += price;
                    }
                }

                if (!tasks.HasMore)
                {
                    break;
                }

                page++;
            }

            // Parts
            decimal partsSubtotal = 0;
            page = 1;
            while (true)
            {
                var items = await _repairOrderItemService.GetListAsync(
                    page: page,
                    pageSize: 200,
                    filterRepairOrderId: repairOrderId,
                    ct: ct).ConfigureAwait(false);

                if (!items.IsSuccess)
                {
                    break;
                }

                for (int i = 0; i < items.RepairOrderItems.Count; i++)
                {
                    var it = items.RepairOrderItems[i];
                    if (_partPriceById.TryGetValue(it.PartId, out var price))
                    {
                        partsSubtotal += price * it.Quantity;
                    }
                }

                if (!items.HasMore)
                {
                    break;
                }

                page++;
            }

            PreviewServiceSubtotal = serviceSubtotal;
            PreviewPartsSubtotal = partsSubtotal;
            PreviewSubtotal = serviceSubtotal + partsSubtotal;

            ApplyDiscountAndTaxToPreview();
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLookupLoading = false;
        }
    }

    private void ApplyDiscountAndTaxToPreview()
    {
        decimal subtotal = PreviewSubtotal;
        DiscountType dt = (DiscountType)PickerDiscountTypeIndex;
        decimal discountValue = FormDiscount;

        decimal discountAmount = dt == DiscountType.Percentage
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
        decimal taxAmount = (subtotal - discountAmount) * ((decimal)tax / 100m);

        PreviewDiscountAmount = discountAmount;
        PreviewTaxAmount = taxAmount;
        PreviewTotalAmount = subtotal - discountAmount + taxAmount;
    }

    private async System.Threading.Tasks.Task LinkInvoiceToRepairOrderAsync(int invoiceId, int repairOrderId)
    {
        if (!_repairOrderById.TryGetValue(repairOrderId, out var ro))
        {
            return;
        }

        // Ensure only 1 RepairOrder linked to this invoice (optional but avoids surprising totals).
        if (IsEditing && SelectedInvoice?.InvoiceId is not null)
        {
            var linked = await _repairOrderService.GetListAsync(
                page: 1,
                pageSize: 100,
                filterCustomerId: Owner!.CustomerId!.Value,
                filterVehicleId: 0,
                filterInvoiceId: invoiceId).ConfigureAwait(false);

            if (linked.IsSuccess && linked.RepairOrders is not null)
            {
                for (int i = 0; i < linked.RepairOrders.Count; i++)
                {
                    var old = linked.RepairOrders[i];
                    if (old.RepairOrderId.HasValue && old.RepairOrderId.Value != repairOrderId)
                    {
                        old.InvoiceId = null;
                        await _repairOrderService.UpdateAsync(old).ConfigureAwait(false);
                    }
                }
            }
        }

        ro.InvoiceId = invoiceId;
        RepairOrderWriteResult linkedResult = await _repairOrderService.UpdateAsync(ro).ConfigureAwait(false);
        if (!linkedResult.IsSuccess)
        {
            HasError = true;
            ErrorMessage = linkedResult.ErrorMessage ?? "Không liên kết được hóa đơn với lệnh.";
        }
    }
}
