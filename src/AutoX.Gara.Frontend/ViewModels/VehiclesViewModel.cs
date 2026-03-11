// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Packets.Customers;
using AutoX.Gara.Shared.Packets.Vehicles;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel cho VehiclesPage — hiển thị danh sách xe + form thêm/sửa/xóa
/// của một customer cụ thể. Nhận <see cref="CustomerDataPacket"/> từ CustomersViewModel
/// thông qua Shell query parameter.
/// </summary>
public sealed partial class VehiclesViewModel : ObservableObject, System.IDisposable
{
    private readonly VehicleService _vehicleService;
    private System.Threading.CancellationTokenSource? _cts;

    private const System.Int32 DefaultPageSize = 10;

    // ─── Customer context ─────────────────────────────────────────────────────

    /// <summary>Customer đang được xem xe — set từ Shell navigation parameter.</summary>
    [ObservableProperty] public partial CustomerDataPacket? Owner { get; set; }

    public System.String PageTitle => Owner is not null
        ? $"Xe của {Owner.Name}"
        : "Danh sách xe";

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
    [ObservableProperty] public partial VehicleDataPacket? SelectedVehicle { get; set; }

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

    // Picker index
    [ObservableProperty] public partial System.Int32 FormPickerTypeIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormPickerColorIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormPickerBrandIndex { get; set; } = 0;

    // Enum values tương ứng với picker
    [ObservableProperty] public partial CarType FormType { get; set; } = CarType.None;
    [ObservableProperty] public partial CarColor FormColor { get; set; } = CarColor.None;
    [ObservableProperty] public partial CarBrand FormBrand { get; set; } = CarBrand.None;

    // Form error
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Delete Confirm ───────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }
    public System.String DeleteConfirmPlate => SelectedVehicle?.LicensePlate ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<VehicleDataPacket> Vehicles { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    public VehiclesViewModel(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
        Vehicles.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Được gọi từ VehiclesPage.OnNavigatedTo sau khi Owner được set.
    /// </summary>
    public void Initialize(CustomerDataPacket owner)
    {
        Owner = owner;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedVehicleChanged(VehicleDataPacket? value)
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

    // Picker → Enum
    partial void OnFormPickerTypeIndexChanged(int value)
    {
        FormType = value switch
        {
            1 => CarType.Sedan,
            2 => CarType.SUV,
            3 => CarType.Hatchback,
            4 => CarType.Coupe,
            5 => CarType.Convertible,
            6 => CarType.Pickup,
            7 => CarType.Minivan,
            8 => CarType.Truck,
            9 => CarType.Bus,
            10 => CarType.Motorcycle,
            11 => CarType.Other,
            _ => CarType.None
        };
    }

    partial void OnFormPickerColorIndexChanged(int value)
    {
        FormColor = value switch
        {
            1 => CarColor.Black,
            2 => CarColor.White,
            3 => CarColor.Gray,
            4 => CarColor.Silver,
            5 => CarColor.Red,
            6 => CarColor.Blue,
            7 => CarColor.Green,
            8 => CarColor.Yellow,
            9 => CarColor.Brown,
            10 => CarColor.Orange,
            11 => CarColor.Purple,
            12 => CarColor.Pink,
            13 => CarColor.Cyan,
            14 => CarColor.Other,
            _ => CarColor.None
        };
    }

    partial void OnFormPickerBrandIndexChanged(int value)
    {
        FormBrand = value switch
        {
            1 => CarBrand.Audi,
            2 => CarBrand.Bentley,
            3 => CarBrand.BMW,
            4 => CarBrand.BYD,
            5 => CarBrand.Bugatti,
            6 => CarBrand.Buick,
            7 => CarBrand.Cadillac,
            8 => CarBrand.Chevrolet,
            9 => CarBrand.Ford,
            10 => CarBrand.Ferrari,
            11 => CarBrand.Honda,
            12 => CarBrand.Hyundai,
            13 => CarBrand.Jaguar,
            14 => CarBrand.Jeep,
            15 => CarBrand.KIA,
            16 => CarBrand.Lamborghini,
            17 => CarBrand.LandRover,
            18 => CarBrand.Lexus,
            19 => CarBrand.Mazda,
            20 => CarBrand.McLaren,
            21 => CarBrand.MercedesBenz,
            22 => CarBrand.Mitsubishi,
            23 => CarBrand.Nissan,
            24 => CarBrand.Porsche,
            25 => CarBrand.RollsRoyce,
            26 => CarBrand.Subaru,
            27 => CarBrand.Suzuki,
            28 => CarBrand.Tesla,
            29 => CarBrand.Toyota,
            30 => CarBrand.VinFast,
            31 => CarBrand.Volvo,
            32 => CarBrand.Volkswagen,
            33 => CarBrand.Other,
            _ => CarBrand.None
        };
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
                foreach (VehicleDataPacket v in result.Vehicles)
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
    private void OpenEditForm(VehicleDataPacket vehicle)
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

        FormPickerTypeIndex = vehicle.Type switch
        {
            CarType.Sedan => 1,
            CarType.SUV => 2,
            CarType.Hatchback => 3,
            CarType.Coupe => 4,
            CarType.Convertible => 5,
            CarType.Pickup => 6,
            CarType.Minivan => 7,
            CarType.Truck => 8,
            CarType.Bus => 9,
            CarType.Motorcycle => 10,
            CarType.Other => 11,
            _ => 0
        };
        FormPickerColorIndex = vehicle.Color switch
        {
            CarColor.Black => 1,
            CarColor.White => 2,
            CarColor.Gray => 3,
            CarColor.Silver => 4,
            CarColor.Red => 5,
            CarColor.Blue => 6,
            CarColor.Green => 7,
            CarColor.Yellow => 8,
            CarColor.Brown => 9,
            CarColor.Orange => 10,
            CarColor.Purple => 11,
            CarColor.Pink => 12,
            CarColor.Cyan => 13,
            CarColor.Other => 14,
            _ => 0
        };
        FormPickerBrandIndex = vehicle.Brand switch
        {
            CarBrand.Audi => 1,
            CarBrand.Bentley => 2,
            CarBrand.BMW => 3,
            CarBrand.BYD => 4,
            CarBrand.Bugatti => 5,
            CarBrand.Buick => 6,
            CarBrand.Cadillac => 7,
            CarBrand.Chevrolet => 8,
            CarBrand.Ford => 9,
            CarBrand.Ferrari => 10,
            CarBrand.Honda => 11,
            CarBrand.Hyundai => 12,
            CarBrand.Jaguar => 13,
            CarBrand.Jeep => 14,
            CarBrand.KIA => 15,
            CarBrand.Lamborghini => 16,
            CarBrand.LandRover => 17,
            CarBrand.Lexus => 18,
            CarBrand.Mazda => 19,
            CarBrand.McLaren => 20,
            CarBrand.MercedesBenz => 21,
            CarBrand.Mitsubishi => 22,
            CarBrand.Nissan => 23,
            CarBrand.Porsche => 24,
            CarBrand.RollsRoyce => 25,
            CarBrand.Subaru => 26,
            CarBrand.Suzuki => 27,
            CarBrand.Tesla => 28,
            CarBrand.Toyota => 29,
            CarBrand.VinFast => 30,
            CarBrand.Volvo => 31,
            CarBrand.Volkswagen => 32,
            CarBrand.Other => 33,
            _ => 0
        };

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
            VehicleDataPacket data = BuildPacketFromForm();
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
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RequestDelete(VehicleDataPacket vehicle)
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

        VehicleDataPacket toDelete = SelectedVehicle;

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
        _cts?.Cancel();
        _cts?.Dispose();
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
        FormPickerTypeIndex = FormPickerColorIndex = FormPickerBrandIndex = 0;
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

    private VehicleDataPacket BuildPacketFromForm() => new()
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