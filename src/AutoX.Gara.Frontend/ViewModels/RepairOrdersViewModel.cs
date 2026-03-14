// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System.Collections.ObjectModel;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class RepairOrdersViewModel(RepairOrderService service) : ObservableObject, System.IDisposable
{
    private readonly RepairOrderService _service = service ?? throw new System.ArgumentNullException(nameof(service));
    private System.Threading.CancellationTokenSource? _cts;

    private const System.Int32 DefaultPageSize = 5;

    // ─── Context ─────────────────────────────────────────────────────────────

    [ObservableProperty] public partial CustomerDto? Owner { get; set; }
    [ObservableProperty] public partial VehicleDto? Vehicle { get; set; }
    [ObservableProperty] public partial InvoiceDto? Invoice { get; set; }

    public System.String PageTitle => Vehicle is not null
        ? $"Sửa chữa {Vehicle.LicensePlate}"
        : Invoice?.InvoiceNumber is not null
            ? $"Lệnh sửa chữa ({Invoice.InvoiceNumber})"
            : "Lệnh sửa chữa";

    public System.Boolean CanAdd => Vehicle?.VehicleId is not null;

    // ─── List ───────────────────────────────────────────────────────────────

    public ObservableCollection<RepairOrderDto> RepairOrders { get; } = [];

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }
    [ObservableProperty] public partial System.Int32 TotalCount { get; set; }

    public System.Int32 TotalPages => TotalCount > 0
        ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize)
        : 0;

    // ─── State ──────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    // ─── Popup ──────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";
    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }
    [ObservableProperty] public partial RepairOrderDto? SelectedRepairOrder { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa lệnh" : "Tạo lệnh";
    public System.String FormSaveText => IsEditing ? "Lưu" : "Tạo";

    [ObservableProperty] public partial System.DateTime FormOrderDate { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.Boolean HasCompletionDate { get; set; }
    [ObservableProperty] public partial System.DateTime FormCompletionDateValue { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.Int32 PickerStatusIndex { get; set; } = 1; // Pending

    public System.String[] StatusOptions { get; } = EnumText.GetNames<RepairOrderStatus>();

    public RepairOrderStatus FormStatus
    {
        get
        {
            RepairOrderStatus[] values = System.Enum.GetValues<RepairOrderStatus>();
            return PickerStatusIndex < 0 || PickerStatusIndex >= values.Length ? RepairOrderStatus.Pending : values[PickerStatusIndex];
        }
    }

    // ─── Delete confirm ─────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }
    public System.String DeleteConfirmTitle => SelectedRepairOrder?.RepairOrderId is null
        ? "Xác nhận xóa"
        : $"Xóa lệnh #{SelectedRepairOrder.RepairOrderId}";

    // ─── Public API ─────────────────────────────────────────────────────────

    public void Initialize(CustomerDto owner, VehicleDto vehicle)
    {
        Owner = owner;
        Vehicle = vehicle;
        Invoice = null;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(CanAdd));
        _ = LoadAsync();
    }

    public void Initialize(CustomerDto owner, InvoiceDto invoice)
    {
        Owner = owner;
        Vehicle = null;
        Invoice = invoice;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(CanAdd));
        _ = LoadAsync();
    }

    // ─── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        if (Owner?.CustomerId is null)
        {
            return;
        }

        System.Int32 vehicleId = Vehicle?.VehicleId ?? 0;
        System.Int32 invoiceId = Invoice?.InvoiceId ?? 0;
        if (vehicleId <= 0 && invoiceId <= 0)
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
            RepairOrderListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                filterCustomerId: Owner.CustomerId.Value,
                filterVehicleId: vehicleId,
                filterInvoiceId: invoiceId,
                ct: ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (!result.IsSuccess)
            {
                HandleError("Không tải được lệnh sửa chữa", result.ErrorMessage ?? "Thao tác thất bại.", result.Advice);
                return;
            }

            RepairOrders.Clear();
            for (System.Int32 i = 0; i < result.RepairOrders.Count; i++)
            {
                RepairOrders.Add(result.RepairOrders[i]);
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
        SelectedRepairOrder = null;
        FormOrderDate = System.DateTime.Today;
        HasCompletionDate = false;
        FormCompletionDateValue = System.DateTime.Today;
        PickerStatusIndex = (System.Int32)RepairOrderStatus.Pending;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(RepairOrderDto ro)
    {
        IsEditing = true;
        SelectedRepairOrder = ro;
        FormOrderDate = ro.OrderDate.ToLocalTime().Date;
        HasCompletionDate = ro.CompletionDate.HasValue;
        FormCompletionDateValue = ro.CompletionDate?.ToLocalTime().Date ?? System.DateTime.Today;
        PickerStatusIndex = (System.Int32)ro.Status;
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

        // Create requires vehicle context. Update can fall back to the selected row's VehicleId.
        System.Int32 vehicleId = Vehicle?.VehicleId
            ?? (IsEditing ? (SelectedRepairOrder?.VehicleId ?? 0) : 0);

        if (vehicleId <= 0)
        {
            return;
        }

        IsLoading = true;
        ClearError();

        try
        {
            RepairOrderDto packet = new()
            {
                RepairOrderId = IsEditing ? SelectedRepairOrder?.RepairOrderId : null,
                CustomerId = Owner.CustomerId.Value,
                VehicleId = vehicleId,
                InvoiceId = IsEditing ? SelectedRepairOrder?.InvoiceId : null,
                OrderDate = FormOrderDate.ToUniversalTime(),
                CompletionDate = HasCompletionDate ? FormCompletionDateValue.ToUniversalTime() : null,
                Status = FormStatus
            };

            RepairOrderWriteResult result = IsEditing
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
    private void RequestDelete(RepairOrderDto ro)
    {
        SelectedRepairOrder = ro;
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand] private void CancelDelete() => IsDeleteConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedRepairOrder?.RepairOrderId is null)
        {
            return;
        }

        IsDeleteConfirmVisible = false;
        IsLoading = true;
        ClearError();

        try
        {
            RepairOrderWriteResult result = await _service.DeleteAsync(SelectedRepairOrder);
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

    [RelayCommand]
    private static async System.Threading.Tasks.Task OpenTasksAsync(RepairOrderDto ro)
    {
        var page = new Views.RepairTasksPage();
        page.Initialize(ro);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private static async System.Threading.Tasks.Task OpenPartsAsync(RepairOrderDto ro)
    {
        var page = new Views.RepairOrderItemsPage();
        page.Initialize(ro);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand] private void ClosePopup() => IsPopupVisible = false;

    [RelayCommand]
    private void RetryLoad()
    {
        IsPopupVisible = false;
        _ = LoadAsync();
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

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    private void HandleError(System.String title, System.String message, ProtocolAdvice advice)
    {
        switch (advice)
        {
            case ProtocolAdvice.DO_NOT_RETRY:
                ShowPopup(title, message, isRetry: false); break;
            case ProtocolAdvice.BACKOFF_RETRY:
                ShowPopup(title, message, isRetry: true); break;
            default:
                HasError = true;
                ErrorMessage = message; break;
        }
    }

    private void ShowPopup(System.String title, System.String message, System.Boolean isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? "Thử lại" : "OK";
        IsPopupVisible = true;
    }
}
