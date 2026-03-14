// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Frontend.Results.Vehicles;
using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;
using System.Linq;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel cho VehiclesPage — hiển thị danh sách xe + form thêm/sửa/xóa
/// của một customer cụ thể. Nhận <see cref="CustomerDto"/> từ CustomersViewModel
/// thông qua Shell query parameter.
/// </summary>
public sealed partial class VehiclesViewModel : ObservableObject, System.IDisposable
{
    private readonly VehicleService _vehicleService;
    private System.Threading.CancellationTokenSource? _cts;

    private const System.Int32 DefaultPageSize = 2;

    // ─── Customer context ─────────────────────────────────────────────────────

    /// <summary>Customer đang được xem xe — set từ Shell navigation parameter.</summary>
    [ObservableProperty] public partial CustomerDto? Owner { get; set; }

    public System.String PageTitle => Owner is not null ? $"Xe của {Owner.Name}" : "Danh sách xe";

    // ─── Pagination ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }
    [ObservableProperty] public partial System.Int32 TotalCount { get; set; }

    public System.Int32 TotalPages => TotalCount > 0
        ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize)
        : 0;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Vehicles.Count == 0;

    // ─── Popup lỗi ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form Add/Edit ────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }
    [ObservableProperty] public partial VehicleDto? SelectedVehicle { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa xe" : "Thêm xe mới";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm xe";

    // Form fields
    [ObservableProperty] public partial System.String FormLicensePlate { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormModel { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormEngineNumber { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormFrameNumber { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.Int32 FormYear { get; set; } = System.DateTime.Now.Year;
    [ObservableProperty] public partial System.Double FormMileage { get; set; }
    [ObservableProperty] public partial System.DateTime FormRegistrationDate { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.DateTime? FormInsuranceExpiryDate { get; set; }

    // Enum values tương ứng với picker
    [ObservableProperty] public partial CarType FormType { get; set; } = CarType.None;
    [ObservableProperty] public partial CarColor FormColor { get; set; } = CarColor.None;
    [ObservableProperty] public partial CarBrand FormBrand { get; set; } = CarBrand.None;

    public System.Collections.Generic.IReadOnlyList<CarBrand> BrandOptions { get; }
        = System.Enum.GetValues<CarBrand>().ToList();
    public System.Collections.Generic.IReadOnlyList<CarType> TypeOptions { get; }
        = System.Enum.GetValues<CarType>().ToList();
    public System.Collections.Generic.IReadOnlyList<CarColor> ColorOptions { get; }
        = System.Enum.GetValues<CarColor>().ToList();

    // Form error
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Delete Confirm ───────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }
    public System.String DeleteConfirmPlate => SelectedVehicle?.LicensePlate ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<VehicleDto> Vehicles { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    public VehiclesViewModel(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
        Vehicles.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Được gọi từ VehiclesPage.OnNavigatedTo sau khi Owner được set.
    /// </summary>
    public void Initialize(CustomerDto owner)
    {
        Owner = owner;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedVehicleChanged(VehicleDto? value)
        => OnPropertyChanged(nameof(DeleteConfirmPlate));

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(FormSaveText));
    }

    partial void OnCurrentPageChanged(int value)
    {
        HasPreviousPage = value > 1;
        _ = LoadAsync();
    }

    // ─── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadAsync()
    {
        if (Owner is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        ClearError();
        IsLoading = true;

        try
        {
            VehicleListResult result = await _vehicleService.GetListAsync(
                customerId: Owner.CustomerId!.Value,
                page: CurrentPage,
                pageSize: DefaultPageSize,
                ct: ct);

            Debug.WriteLine($"[VehiclesVM] ok={result.IsSuccess} count={result.Vehicles.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Vehicles.Clear();
                foreach (VehicleDto v in result.Vehicles)
                {
                    Vehicles.Add(v);
                }

                TotalCount = result.TotalCount >= 0 ? result.TotalCount : TotalCount;
                HasNextPage = result.TotalCount >= 0
                    ? CurrentPage < TotalPages
                    : result.HasMore;
            }
            else
            {
                HandleError("Tải danh sách thất bại", result.ErrorMessage!, result.Advice);
            }
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // User navigated away or refreshed quickly; swallow cancellation.
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedVehicle = null;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(VehicleDto vehicle)
    {
        IsEditing = true;
        SelectedVehicle = vehicle;

        FormLicensePlate = vehicle.LicensePlate ?? System.String.Empty;
        FormModel = vehicle.Model ?? System.String.Empty;
        FormEngineNumber = vehicle.EngineNumber ?? System.String.Empty;
        FormFrameNumber = vehicle.FrameNumber ?? System.String.Empty;
        FormYear = vehicle.Year;
        FormMileage = vehicle.Mileage;
        FormRegistrationDate = vehicle.RegistrationDate == default ? System.DateTime.Today : vehicle.RegistrationDate;
        FormInsuranceExpiryDate = vehicle.InsuranceExpiryDate;

        FormType = vehicle.Type;
        FormColor = vehicle.Color;
        FormBrand = vehicle.Brand;

        ClearFormError();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormVisible = false;
        ClearForm();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveFormAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsLoading = true;

        try
        {
            VehicleDto data = BuildPacketFromForm();
            VehicleWriteResult result = IsEditing
                ? await _vehicleService.UpdateAsync(data, ct)
                : await _vehicleService.CreateAsync(data, ct);

            if (result.IsSuccess)
            {
                IsFormVisible = false;
                ClearForm();

                if (result.UpdatedEntity is not null)
                {
                    if (IsEditing)
                    {
                        System.Int32 idx = IndexOfVehicle(result.UpdatedEntity.VehicleId);
                        if (idx >= 0)
                        {
                            Vehicles[idx] = result.UpdatedEntity;
                        }
                        else
                        {
                            await LoadAsync();
                        }
                    }
                    else
                    {
                        Vehicles.Insert(0, result.UpdatedEntity);
                        TotalCount++;
                        OnPropertyChanged(nameof(TotalPages));
                        HasNextPage = CurrentPage < TotalPages;
                    }
                }
                else
                {
                    await LoadAsync();
                }
            }
            else
            {
                SetFormError(result.ErrorMessage ?? "Thao tác thất bại.");
            }
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RequestDelete(VehicleDto vehicle)
    {
        SelectedVehicle = vehicle;
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelDelete() => IsDeleteConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsDeleteConfirmVisible = false;
        IsLoading = true;

        VehicleDto toDelete = SelectedVehicle;

        try
        {
            VehicleWriteResult result = await _vehicleService.DeleteAsync(toDelete, ct);

            if (result.IsSuccess)
            {
                Vehicles.Remove(toDelete);
                TotalCount = System.Math.Max(0, TotalCount - 1);
                SelectedVehicle = null;
                OnPropertyChanged(nameof(TotalPages));
                HasNextPage = CurrentPage < TotalPages;

                if (Vehicles.Count == 0 && CurrentPage > 1)
                {
                    CurrentPage--;
                }
            }
            else
            {
                HandleError("Xóa thất bại", result.ErrorMessage!, result.Advice);
            }
        }
        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            IsLoading = false;
        }
    }

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

    [RelayCommand] private void ClosePopup() => IsPopupVisible = false;

    [RelayCommand]
    private void RetryLoad()
    {
        IsPopupVisible = false;
        _ = LoadAsync();
    }

    // ─── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        // Dispose can be called multiple times (toolbar back, physical back, etc.).
        // Make it idempotent to avoid ObjectDisposedException from calling Cancel()
        // on a disposed CancellationTokenSource.
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
            // Ignore: CTS may already be disposed.
        }

        cts.Dispose();
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private void ClearError() { HasError = false; ErrorMessage = null; }

    private void ClearForm()
    {
        FormLicensePlate = FormModel = FormEngineNumber = FormFrameNumber = System.String.Empty;
        FormYear = System.DateTime.Now.Year;
        FormMileage = 0;
        FormRegistrationDate = System.DateTime.Today;
        FormInsuranceExpiryDate = null;
        FormType = CarType.None;
        FormColor = CarColor.None;
        FormBrand = CarBrand.None;
        ClearFormError();
    }

    private void ClearFormError() { HasFormError = false; FormErrorMessage = null; }

    private void SetFormError(System.String message) { FormErrorMessage = message; HasFormError = true; }

    private System.Boolean ValidateForm()
    {
        if (System.String.IsNullOrWhiteSpace(FormLicensePlate))
        { SetFormError("Biển số xe không được để trống."); return false; }

        if (FormLicensePlate.Length > 20)
        { SetFormError("Biển số xe không được vượt quá 20 ký tự."); return false; }

        if (FormYear < 1900 || FormYear > System.DateTime.Now.Year + 1)
        { SetFormError($"Năm sản xuất không hợp lệ (1900 – {System.DateTime.Now.Year + 1})."); return false; }

        if (FormMileage < 0)
        { SetFormError("Số km không được âm."); return false; }

        return true;
    }

    private VehicleDto BuildPacketFromForm() => new()
    {
        CustomerId = Owner!.CustomerId!.Value,
        VehicleId = IsEditing ? SelectedVehicle?.VehicleId : null,
        LicensePlate = FormLicensePlate,
        Model = FormModel,
        EngineNumber = FormEngineNumber,
        FrameNumber = FormFrameNumber,
        Year = FormYear,
        Mileage = FormMileage,
        Type = FormType,
        Color = FormColor,
        Brand = FormBrand,
        RegistrationDate = FormRegistrationDate,
        InsuranceExpiryDate = FormInsuranceExpiryDate
    };

    private System.Int32 IndexOfVehicle(System.Int32? vehicleId)
    {
        if (vehicleId is null)
        {
            return -1;
        }

        for (System.Int32 i = 0; i < Vehicles.Count; i++)
        {
            if (Vehicles[i].VehicleId == vehicleId)
            {
                return i;
            }
        }
        return -1;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task OpenRepairOrdersAsync(VehicleDto vehicle)
    {
        if (Owner is null)
        {
            return;
        }

        var page = new Views.RepairOrdersPage();
        page.Initialize(Owner, vehicle);
        await Shell.Current.Navigation.PushAsync(page);
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
