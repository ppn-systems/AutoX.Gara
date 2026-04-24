// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Frontend.Services.Repairs;
using AutoX.Gara.Contracts.Billings;
using AutoX.Gara.Contracts.Customers;
using AutoX.Gara.Contracts.Invoices;
using AutoX.Gara.Contracts.Vehicles;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.ObjectModel;
namespace AutoX.Gara.Frontend.Controllers.Billings;
public sealed partial class RepairOrdersViewModel(RepairOrderService service) : ObservableObject, System.IDisposable
{
    private readonly RepairOrderService _service = service ?? throw new System.ArgumentNullException(nameof(service));
    private System.Threading.CancellationTokenSource? _cts;
    private const int DefaultPageSize = 5;
    // --- Context ------------------------------------------------------------─
    [ObservableProperty] public partial CustomerDto? Owner { get; set; }
    [ObservableProperty] public partial VehicleDto? Vehicle { get; set; }
    [ObservableProperty] public partial InvoiceDto? Invoice { get; set; }
    public string PageTitle => Vehicle is not null
        ? string.Format(System.Globalization.CultureInfo.CurrentCulture, UiTextConfiguration.Current.RepairOrdersPageTitleVehicleText, Vehicle.LicensePlate)
        : Invoice?.InvoiceNumber is not null
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, UiTextConfiguration.Current.RepairOrdersPageTitleInvoiceText, Invoice.InvoiceNumber)
            : UiTextConfiguration.Current.RepairOrdersPageTitleText;
    public bool CanAdd => Vehicle?.VehicleId is not null;
    // --- List ---------------------------------------------------------------
    public ObservableCollection<RepairOrderDto> RepairOrders { get; } = [];
    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0
        ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize)
        : 0;
    // --- State ------------------------------------------------------------──
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    // --- Popup ------------------------------------------------------------──
    [ObservableProperty] public partial bool IsPopupVisible { get; set; }
    [ObservableProperty] public partial bool IsPopupRetry { get; set; }
    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupButtonText { get; set; } = UiTextConfiguration.Current.PopupOkButtonText;
    public bool IsPopupNotRetry => !IsPopupRetry;
    // --- Form ---------------------------------------------------------------
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial RepairOrderDto? SelectedRepairOrder { get; set; }
    public string FormTitle => IsEditing
        ? UiTextConfiguration.Current.RepairOrdersFormTitleEditText
        : UiTextConfiguration.Current.RepairOrdersFormTitleCreateText;
    public string FormSaveText => IsEditing
        ? UiTextConfiguration.Current.CommonFormSaveText
        : UiTextConfiguration.Current.CommonFormCreateText;
    [ObservableProperty] public partial DateTime FormOrderDate { get; set; } = DateTime.Today;
    [ObservableProperty] public partial bool HasCompletionDate { get; set; }
    [ObservableProperty] public partial DateTime FormCompletionDateValue { get; set; } = DateTime.Today;
    [ObservableProperty] public partial int PickerStatusIndex { get; set; } = 1; // Pending
    public string[] StatusOptions { get; } = EnumText.GetNames<RepairOrderStatus>();
    public string SelectedStatusText =>
        StatusOptions[System.Math.Clamp(PickerStatusIndex, 0, StatusOptions.Length - 1)];
    public RepairOrderStatus FormStatus
    {
        get
        {
            RepairOrderStatus[] values = System.Enum.GetValues<RepairOrderStatus>();
            return PickerStatusIndex < 0 || PickerStatusIndex >= values.Length ? RepairOrderStatus.Pending : values[PickerStatusIndex];
        }
    }
    // --- Delete confirm ---------------------------------------------------──
    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }
    public string DeleteConfirmTitle => SelectedRepairOrder?.RepairOrderId is null
        ? UiTextConfiguration.Current.RepairOrdersDeleteConfirmTitleText
        : string.Format(System.Globalization.CultureInfo.CurrentCulture, UiTextConfiguration.Current.RepairOrdersDeleteConfirmMessageWithIdText, SelectedRepairOrder.RepairOrderId);
    // --- Public API ---------------------------------------------------------
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
    // --- Commands ---------------------------------------------------------──
    [RelayCommand]
    public async System.Threading.Tasks.Task LoadAsync()
    {
        if (Owner?.CustomerId is null)
        {
            return;
        }
        int vehicleId = Vehicle?.VehicleId ?? 0;
        int invoiceId = Invoice?.InvoiceId ?? 0;
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
                HandleError(
                    UiTextConfiguration.Current.RepairOrdersErrorLoadFailedText,
                    result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText,
                    result.Advice);
                return;
            }
            RepairOrders.Clear();
            for (int i = 0; i < result.RepairOrders.Count; i++)
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
    partial void OnPickerStatusIndexChanged(int value)
        => OnPropertyChanged(nameof(SelectedStatusText));
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
        FormOrderDate = DateTime.Today;
        HasCompletionDate = false;
        FormCompletionDateValue = DateTime.Today;
        PickerStatusIndex = (int)RepairOrderStatus.Pending;
        IsFormVisible = true;
    }
    [RelayCommand]
    private void OpenEditForm(RepairOrderDto ro)
    {
        IsEditing = true;
        SelectedRepairOrder = ro;
        FormOrderDate = ro.OrderDate.ToLocalTime().Date;
        HasCompletionDate = ro.CompletionDate.HasValue;
        FormCompletionDateValue = ro.CompletionDate?.ToLocalTime().Date ?? DateTime.Today;
        PickerStatusIndex = (int)ro.Status;
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
        int vehicleId = Vehicle?.VehicleId
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
                HandleError(
                    UiTextConfiguration.Current.RepairOrdersErrorSaveFailedText,
                    result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText,
                    result.Advice);
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
                HandleError(
                    UiTextConfiguration.Current.RepairOrdersErrorDeleteFailedText,
                    result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText,
                    result.Advice);
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
    private void HandleError(string title, string message, ProtocolAdvice advice)
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
    private void ShowPopup(string title, string message, bool isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry
            ? UiTextConfiguration.Current.PopupRetryButtonText
            : UiTextConfiguration.Current.PopupOkButtonText;
        IsPopupVisible = true;
    }
}


