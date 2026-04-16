using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Domain.Enums.Payments;

using AutoX.Gara.Frontend.Helpers;

using AutoX.Gara.Frontend.Messages;

using AutoX.Gara.Frontend.Results.Billings;

using AutoX.Gara.Frontend.Results.Parts;

using AutoX.Gara.Frontend.Results.ServiceItems;

using AutoX.Gara.Frontend.Services.Billings;

using AutoX.Gara.Frontend.Services.Inventory;

using AutoX.Gara.Frontend.Services.Invoices;

using AutoX.Gara.Frontend.Services.Repairs;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Billings;

using AutoX.Gara.Shared.Protocol.Customers;

using AutoX.Gara.Shared.Protocol.Inventory;

using AutoX.Gara.Shared.Protocol.Invoices;

using AutoX.Gara.Shared.Protocol.Repairs;

using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Maui.Controls;

using Microsoft.Extensions.Logging;

using Nalix.Common.Networking.Protocols;

using Nalix.Framework.Injection;

using System.Collections.ObjectModel;

using System.Linq;

namespace AutoX.Gara.Frontend.Controllers.Billings;

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

    private System.Threading.CancellationTokenSource? _moneyDetailsCts;

    private const int DefaultPageSize = 10;

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

        WeakReferenceMessenger.Default.Register<InvoiceTotalsChangedMessage>(this, (_, __) =>

        {
            // Invoice totals depend on Transactions; refresh when they change.

            _ = LoadAsync();

        });

        MoneyPopupServiceLines.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasMoneyPopupServiceLines));

        MoneyPopupPartLines.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasMoneyPopupPartLines));

    }

    public sealed record LookupOption(int Id, string Display);

    public sealed partial class InvoiceRow : ObservableObject

    {
        public InvoiceRow(InvoiceDto dto) => Dto = dto ?? throw new System.ArgumentNullException(nameof(dto));

        public InvoiceDto Dto { get; }

        public int? InvoiceId => Dto.InvoiceId;

        public string InvoiceNumber => Dto.InvoiceNumber;

        public DateTime InvoiceDate => Dto.InvoiceDate;

        public int RepairOrderId => Dto.RepairOrderId;

        public PaymentStatus PaymentStatus => Dto.PaymentStatus;

        public string PaymentStatusText => EnumText.Get(Dto.PaymentStatus);

        public decimal ServiceSubtotal => Dto.ServiceSubtotal;

        public decimal PartsSubtotal => Dto.PartsSubtotal;

        public decimal Subtotal => Dto.Subtotal;

        public DiscountType DiscountType => Dto.DiscountType;

        public decimal Discount => Dto.Discount;

        public decimal DiscountAmount => Dto.DiscountAmount;

        public TaxRateType TaxRate => Dto.TaxRate;

        public string TaxRateText => EnumText.Get(Dto.TaxRate);

        public decimal TaxAmount => Dto.TaxAmount;

        public decimal TotalAmount => Dto.TotalAmount;

        public decimal BalanceDue => Dto.BalanceDue;

        public decimal AmountPaid => Dto.TotalAmount - Dto.BalanceDue;

        public bool CanPay => BalanceDue > 0;

        public string PayNowText => CanPay ? "Thanh to�n" : "�ã thanh to�n";

        [ObservableProperty] public partial bool IsExpanded { get; set; }

    }

    public sealed record MoneyLineItemRow(

        string Title,

        string? Subtitle,

        int Quantity,

        decimal? UnitPrice,

        decimal? LineTotal);

    [ObservableProperty] public partial CustomerDto? Owner { get; set; }

    public string PageTitle => Owner is null ? "H�a don" : $"H�a don {Owner.Name}";

    public ObservableCollection<InvoiceRow> Invoices { get; } = [];

    // Money details popup

    [ObservableProperty] public partial bool IsMoneyPopupVisible { get; set; }

    [ObservableProperty] public partial InvoiceRow? MoneyPopupRow { get; set; }

    [ObservableProperty] public partial int MoneyPopupRepairOrderId { get; set; }

    public ObservableCollection<MoneyLineItemRow> MoneyPopupServiceLines { get; } = [];

    public ObservableCollection<MoneyLineItemRow> MoneyPopupPartLines { get; } = [];

    [ObservableProperty] public partial bool IsMoneyPopupLineItemsLoading { get; set; }

    [ObservableProperty] public partial string? MoneyPopupLineItemsError { get; set; }

    public bool HasMoneyPopupLineItemsError => !string.IsNullOrWhiteSpace(MoneyPopupLineItemsError);

    partial void OnMoneyPopupLineItemsErrorChanged(string? value) => OnPropertyChanged(nameof(HasMoneyPopupLineItemsError));

    public bool HasMoneyPopupServiceLines => MoneyPopupServiceLines.Count > 0;

    public bool HasMoneyPopupPartLines => MoneyPopupPartLines.Count > 0;

    public bool HasMoneyPopupRepairOrderLink => MoneyPopupRepairOrderId > 0;

    partial void OnMoneyPopupRepairOrderIdChanged(int value) => OnPropertyChanged(nameof(HasMoneyPopupRepairOrderLink));

    // Delete confirm popup

    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }

    [ObservableProperty] public partial InvoiceRow? DeleteConfirmRow { get; set; }

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;

    [ObservableProperty] public partial bool HasNextPage { get; set; }

    [ObservableProperty] public partial bool HasPreviousPage { get; set; }

    [ObservableProperty] public partial int TotalCount { get; set; }

    public int TotalPages => TotalCount > 0 ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize) : 0;

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty] public partial bool IsLookupLoading { get; set; }

    [ObservableProperty] public partial bool HasError { get; set; }

    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    // Form

    [ObservableProperty] public partial bool IsFormVisible { get; set; }

    [ObservableProperty] public partial bool IsEditing { get; set; }

    [ObservableProperty] public partial InvoiceDto? SelectedInvoice { get; set; }

    [ObservableProperty] public partial string FormInvoiceNumber { get; set; } = string.Empty;

    [ObservableProperty] public partial DateTime FormInvoiceDate { get; set; } = DateTime.Today;

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

    private readonly Dictionary<int, RepairOrderDto> _repairOrderById = [];

    // Preview totals (client-side estimate)

    [ObservableProperty] public partial decimal PreviewServiceSubtotal { get; set; }

    [ObservableProperty] public partial decimal PreviewPartsSubtotal { get; set; }

    [ObservableProperty] public partial decimal PreviewSubtotal { get; set; }

    [ObservableProperty] public partial decimal PreviewDiscountAmount { get; set; }

    [ObservableProperty] public partial decimal PreviewTaxAmount { get; set; }

    [ObservableProperty] public partial decimal PreviewTotalAmount { get; set; }

    public string[] TaxRateOptions { get; } = TaxRateValues.Select(EnumText.Get).ToArray();

    public string[] DiscountTypeOptions { get; } = EnumText.GetNames<DiscountType>();

    public string[] PaymentStatusOptions { get; } = EnumText.GetNames<PaymentStatus>();

    public string SelectedTaxRateText =>

        TaxRateOptions[System.Math.Clamp(PickerTaxRateIndex, 0, TaxRateOptions.Length - 1)];

    public string SelectedDiscountTypeText =>

        DiscountTypeOptions[System.Math.Clamp(PickerDiscountTypeIndex, 0, DiscountTypeOptions.Length - 1)];

    public string SelectedPaymentStatusText =>

        PaymentStatusOptions[System.Math.Clamp(PickerPaymentStatusIndex, 0, PaymentStatusOptions.Length - 1)];

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
                HandleError("Kh�ng t?i du?c h�a don", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);

                return;

            }

            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

            Invoices.Clear();

            for (int i = 0; i < result.Invoices.Count; i++)

            {
                var dto = result.Invoices[i];

                logger?.Info(

                    $"[FE.{nameof(InvoicesViewModel)}:{nameof(LoadAsync)}] invoiceId={dto.InvoiceId}");

                Invoices.Add(new InvoiceRow(dto));

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

        FormInvoiceDate = DateTime.Today;

        PickerTaxRateIndex = System.Array.IndexOf(TaxRateValues, TaxRateType.VAT10);

        PickerDiscountTypeIndex = (int)DiscountType.None;

        FormDiscount = 0;

        PickerPaymentStatusIndex = (int)PaymentStatus.Unpaid;

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

        ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

        logger?.Info(

            $"[FE.{nameof(InvoicesViewModel)}:{nameof(OpenEditForm)}] invoiceId={inv.InvoiceId}");

        IsEditing = true;

        SelectedInvoice = inv;

        FormInvoiceNumber = inv.InvoiceNumber;

        FormInvoiceDate = inv.InvoiceDate.ToLocalTime().Date;

        PickerTaxRateIndex = System.Math.Max(0, System.Array.IndexOf(TaxRateValues, inv.TaxRate));

        PickerDiscountTypeIndex = (int)inv.DiscountType;

        FormDiscount = inv.Discount;

        PickerPaymentStatusIndex = (int)inv.PaymentStatus;

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

        if (string.IsNullOrWhiteSpace(FormInvoiceNumber))

        {
            HasError = true;

            ErrorMessage = "S? h�a don kh�ng du?c d? tr?ng.";

            return;

        }

        IsLoading = true;

        ClearError();

        try

        {
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

            logger?.Info(

                $"[FE.{nameof(InvoicesViewModel)}:{nameof(SaveAsync)}] invoiceId={SelectedInvoice?.InvoiceId} editing={IsEditing}");

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
                HandleError("Luu th?t b?i", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);

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

                SelectedInvoice = result.UpdatedEntity;

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

    private async System.Threading.Tasks.Task OpenTransactionsAsync(InvoiceRow row)

    {
        try

        {
            if (row is null)

            {
                return;

            }

            var page = new Views.TransactionsPage();

            page.Initialize(row.Dto);

            await PushPageAsync(page);

        }

        catch (Exception ex)

        {
            LogException(ex);

            HandleError("Kh�ng th? m? giao d?ch", "Vui l�ng th? l?i.", ProtocolAdvice.BACKOFF_RETRY);

        }

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task PayNowAsync(InvoiceRow row)

    {
        try

        {
            if (row is null)

            {
                return;

            }

            // If already paid, still allow opening transactions (read-only flow).

            bool canQuickPay = row.BalanceDue > 0;

            var page = new Views.TransactionsPage();

            page.Initialize(row.Dto, autoOpenAddForm: canQuickPay, prefillAmount: canQuickPay ? row.BalanceDue : 0);

            await PushPageAsync(page);

        }

        catch (Exception ex)

        {
            LogException(ex);

            HandleError("Kh�ng th? thanh to�n", "Vui l�ng th? l?i.", ProtocolAdvice.BACKOFF_RETRY);

        }

    }

    [RelayCommand]

    private void RequestDelete(InvoiceRow row)

    {
        DeleteConfirmRow = row;

        IsDeleteConfirmVisible = true;

    }

    [RelayCommand]

    private void CancelDelete()

    {
        IsDeleteConfirmVisible = false;

        DeleteConfirmRow = null;

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task ConfirmDeleteAsync()

    {
        if (DeleteConfirmRow is null)

        {
            return;

        }

        InvoiceRow row = DeleteConfirmRow;

        IsDeleteConfirmVisible = false;

        DeleteConfirmRow = null;

        await DeleteInternalAsync(row);

    }

    private async System.Threading.Tasks.Task DeleteInternalAsync(InvoiceRow row)

    {
        IsLoading = true;

        ClearError();

        try

        {
            InvoiceWriteResult result = await _service.DeleteAsync(row.Dto);

            if (!result.IsSuccess)

            {
                HandleError("X�a th?t b?i", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);

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

        var moneyCts = System.Threading.Interlocked.Exchange(ref _moneyDetailsCts, null);

        if (lookupCts is not null)

        {
            try { lookupCts.Cancel(); } catch { }

            lookupCts.Dispose();

        }

        if (moneyCts is not null)

        {
            try { moneyCts.Cancel(); } catch { }

            moneyCts.Dispose();

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

    private void HandleError(string title, string message, ProtocolAdvice advice)

    {
        HasError = true;

        ErrorMessage = message;

    }

    private static string GenerateInvoiceNumber()

    {
        string date = DateTime.Now.ToString("yyyyMMdd");

        int rand = System.Random.Shared.Next(1000, 9999);

        return $"INV-{date}-{rand}";

    }

    private static void LogException(Exception ex)

    {
        try

        {
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

            logger?.Error(ex.ToString());

            if (ex.InnerException is not null)

            {
                logger?.Error("Inner: " + ex.InnerException);

            }

        }

        catch

        {
            // Swallow: logging must never crash UI.

        }

    }

    private static async System.Threading.Tasks.Task PushPageAsync(Page page)

    {
        INavigation? nav = Shell.Current?.Navigation ?? Application.Current?.Windows[0].Page?.Navigation;

        if (nav is null)

        {
            try

            {
                ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

                logger?.Error("[FE.Invoices] Navigation is null; cannot navigate.");

            }

            catch

            {
            }

            return;

        }

        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () => await nav.PushAsync(page));

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task ToggleDetails(InvoiceRow row)

    {
        MoneyPopupRow = row;

        IsMoneyPopupVisible = true;

        var prev = System.Threading.Interlocked.Exchange(ref _moneyDetailsCts, new System.Threading.CancellationTokenSource());

        if (prev is not null)

        {
            try { prev.Cancel(); } catch { }

            prev.Dispose();

        }

        MoneyPopupServiceLines.Clear();

        MoneyPopupPartLines.Clear();

        MoneyPopupLineItemsError = null;

        MoneyPopupRepairOrderId = 0;

        if (row is null)

        {
            return;

        }

        int roId = row.RepairOrderId;

        if (roId <= 0)

        {
            int invId = row.InvoiceId ?? 0;

            if (invId > 0)

            {
                RepairOrderListResult roRes = await _repairOrderService.GetListAsync(

                    page: 1,

                    pageSize: 5,

                    filterCustomerId: row.Dto.CustomerId,

                    filterVehicleId: 0,

                    searchTerm: null,

                    sortBy: RepairOrderSortField.OrderDate,

                    sortDescending: true,

                    filterInvoiceId: invId,

                    filterStatus: null,

                    ct: _moneyDetailsCts!.Token);

                if (roRes.IsSuccess && roRes.RepairOrders is not null)

                {
                    RepairOrderDto? found = roRes.RepairOrders.FirstOrDefault(x => (x.RepairOrderId ?? 0) > 0);

                    roId = found?.RepairOrderId ?? 0;

                }

            }

        }

        MoneyPopupRepairOrderId = roId;

        if (roId <= 0)

        {
            MoneyPopupLineItemsError = "Kh�ng x�c d?nh du?c l?nh li�n k?t c?a h�a don. B?n c� th? b?m 'Xem l?nh' d? ki?m tra.";

            return;

        }

        await LoadMoneyPopupLineItemsAsync(roId, _moneyDetailsCts!.Token);

    }

    [RelayCommand]

    private void CloseMoneyPopup()

    {
        try { _moneyDetailsCts?.Cancel(); } catch { }

        IsMoneyPopupVisible = false;

        MoneyPopupRow = null;

        MoneyPopupRepairOrderId = 0;

        MoneyPopupServiceLines.Clear();

        MoneyPopupPartLines.Clear();

        MoneyPopupLineItemsError = null;

    }

    private async System.Threading.Tasks.Task LoadMoneyPopupLineItemsAsync(int repairOrderId, System.Threading.CancellationToken ct)

    {
        if (repairOrderId <= 0)

        {
            return;

        }

        IsMoneyPopupLineItemsLoading = true;

        MoneyPopupLineItemsError = null;

        try

        {
            var tasks = new List<RepairTaskDto>();

            var items = new List<RepairOrderItemDto>();

            for (int page = 1; ; page++)

            {
                RepairTaskListResult res = await _repairTaskService.GetListAsync(

                    page: page,

                    pageSize: 200,

                    filterRepairOrderId: repairOrderId,

                    sortBy: RepairTaskSortField.Id,

                    sortDescending: true,

                    ct: ct);

                if (!res.IsSuccess)

                {
                    MoneyPopupLineItemsError = res.ErrorMessage ?? "Kh�ng t?i du?c danh s�ch d?ch v?.";

                    return;

                }

                tasks.AddRange(res.RepairTasks);

                if (!res.HasMore)

                {
                    break;

                }

            }

            for (int page = 1; ; page++)

            {
                RepairOrderItemListResult res = await _repairOrderItemService.GetListAsync(

                    page: page,

                    pageSize: 200,

                    filterRepairOrderId: repairOrderId,

                    sortBy: RepairOrderItemSortField.Id,

                    sortDescending: true,

                    ct: ct);

                if (!res.IsSuccess)

                {
                    MoneyPopupLineItemsError = res.ErrorMessage ?? "Kh�ng t?i du?c danh s�ch ph? t�ng.";

                    return;

                }

                items.AddRange(res.RepairOrderItems);

                if (!res.HasMore)

                {
                    break;

                }

            }

            HashSet<int> serviceItemIds = tasks

                .Select(t => t.ServiceItemId)

                .Where(id => id > 0)

                .ToHashSet();

            HashSet<int> partIds = items

                .Select(i => i.PartId)

                .Where(id => id > 0)

                .ToHashSet();

            Dictionary<int, ServiceItemDto> serviceItemById =

                await LoadServiceItemMapAsync(serviceItemIds, ct);

            Dictionary<int, PartDto> partById =

                await LoadPartMapAsync(partIds, ct);

            MoneyPopupServiceLines.Clear();

            MoneyPopupPartLines.Clear();

            foreach (var grp in tasks.GroupBy(t => t.ServiceItemId).OrderBy(g => g.Key))

            {
                int id = grp.Key;

                int qty = grp.Count();

                serviceItemById.TryGetValue(id, out ServiceItemDto? svc);

                string title = svc is null

                    ? $"D?ch v? #{id}"

                    : (string.IsNullOrWhiteSpace(svc.Description) ? $"D?ch v? #{id}" : svc.Description);

                string? subtitle = svc is null ? null : $"{EnumText.Get(svc.Type)}";

                decimal? unit = svc?.UnitPrice;

                decimal? total = unit is null ? null : unit.Value * qty;

                MoneyPopupServiceLines.Add(new MoneyLineItemRow(title, subtitle, qty, unit, total));

            }

            foreach (var grp in items.GroupBy(i => i.PartId).OrderBy(g => g.Key))

            {
                int id = grp.Key;

                int qty = grp.Sum(x => x.Quantity);

                partById.TryGetValue(id, out PartDto? part);

                string title = part is null

                    ? $"Ph? t�ng #{id}"

                    : (string.IsNullOrWhiteSpace(part.PartName) ? $"Ph? t�ng #{id}" : part.PartName);

                string? subtitle = part?.PartCode;

                decimal? unit = part?.SellingPrice;

                decimal? total = unit is null ? null : unit.Value * qty;

                MoneyPopupPartLines.Add(new MoneyLineItemRow(title, subtitle, qty, unit, total));

            }

        }

        catch (System.OperationCanceledException)

        {
        }

        catch (Exception ex)

        {
            LogException(ex);

            MoneyPopupLineItemsError = "Kh�ng t?i du?c chi ti?t d?ch v?/ph? t�ng.";

        }

        finally

        {
            IsMoneyPopupLineItemsLoading = false;

        }

    }

    private async System.Threading.Tasks.Task<Dictionary<int, ServiceItemDto>> LoadServiceItemMapAsync(

        HashSet<int> ids,

        System.Threading.CancellationToken ct)

    {
        var map = new Dictionary<int, ServiceItemDto>();

        if (ids.Count == 0)

        {
            return map;

        }

        for (int page = 1; page <= 30; page++)

        {
            ServiceItemListResult res = await _serviceItemService.GetListAsync(

                page: page,

                pageSize: 120,

                searchTerm: null,

                sortBy: ServiceItemSortField.Description,

                sortDescending: false,

                ct: ct);

            if (!res.IsSuccess)

            {
                break;

            }

            foreach (ServiceItemDto svc in res.ServiceItems)

            {
                int id = svc.ServiceItemId ?? 0;

                if (id > 0 && ids.Contains(id))

                {
                    map[id] = svc;

                }

            }

            if (map.Count >= ids.Count || !res.HasMore)

            {
                break;

            }

        }

        return map;

    }

    private async System.Threading.Tasks.Task<Dictionary<int, PartDto>> LoadPartMapAsync(

        HashSet<int> ids,

        System.Threading.CancellationToken ct)

    {
        var map = new Dictionary<int, PartDto>();

        if (ids.Count == 0)

        {
            return map;

        }

        for (int page = 1; page <= 30; page++)

        {
            PartListResult res = await _partService.GetListAsync(

                page: page,

                pageSize: 120,

                searchTerm: null,

                sortBy: PartSortField.PartName,

                sortDescending: false,

                ct: ct);

            if (!res.IsSuccess)

            {
                break;

            }

            foreach (PartDto p in res.Parts)

            {
                int id = p.PartId ?? 0;

                if (id > 0 && ids.Contains(id))

                {
                    map[id] = p;

                }

            }

            if (map.Count >= ids.Count || !res.HasMore)

            {
                break;

            }

        }

        return map;

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

    // --- RepairOrder Lookup + Preview ---------------------------------------─

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

        OnPropertyChanged(nameof(SelectedTaxRateText));

    }

    partial void OnPickerDiscountTypeIndexChanged(int value)

    {
        if (SelectedRepairOrderOption is not null)

        {
            ApplyDiscountAndTaxToPreview();

        }

        OnPropertyChanged(nameof(SelectedDiscountTypeText));

    }

    partial void OnFormDiscountChanged(decimal value)

    {
        if (SelectedRepairOrderOption is not null)

        {
            ApplyDiscountAndTaxToPreview();

        }

    }

    partial void OnPickerPaymentStatusIndexChanged(int value)

        => OnPropertyChanged(nameof(SelectedPaymentStatusText));

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

            if (term.Length == 0 || opt.Display.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)

            {
                FilteredRepairOrderOptions.Add(opt);

            }

        }

    }

    private async System.Threading.Tasks.Task LoadRepairOrderLookupsAsync(int? editingInvoiceId = null)

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

            void addOrders(List<RepairOrderDto> items)

            {
                for (int i = 0; i < items.Count; i++)

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

                    int id = ro.RepairOrderId.Value;

                    if (_repairOrderById.ContainsKey(id))

                    {
                        continue;

                    }

                    _repairOrderById[id] = ro;

                    string display = $"#{id} - {ro.OrderDate.ToLocalTime():dd/MM/yyyy} - {EnumText.Get(ro.Status)} - {ro.TotalRepairCost:N0}";

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
}