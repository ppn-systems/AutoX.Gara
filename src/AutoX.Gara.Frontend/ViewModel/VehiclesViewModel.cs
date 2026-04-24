// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Vehicles;
using AutoX.Gara.Frontend.Services.Vehicles;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;
using System.Linq;
namespace AutoX.Gara.Frontend.Controllers;
/// <summary>
/// ViewModel cho VehiclesPage � hi?n th? danh s�ch xe + form th�m/s?a/x�a
/// c?a m?t customer c? th?. Nh?n <see cref="CustomerDto"/> t? CustomersViewModel
/// th�ng qua Shell query parameter.
/// </summary>
public sealed partial class VehiclesViewModel : ObservableObject, System.IDisposable
{
    private readonly VehicleService _vehicleService;
    private System.Threading.CancellationTokenSource? _cts;
    private const int DefaultPageSize = 2;
    // --- Customer context -----------------------------------------------------
    /// <summary>Customer dang được xem xe � set t? Shell navigation parameter.</summary>
    [ObservableProperty] public partial CustomerDto? Owner { get; set; }
    public string PageTitle => Owner is not null
        ? string.Format(System.Globalization.CultureInfo.CurrentCulture, UiTextConfiguration.Current.VehiclesPageTitleWithOwnerText, Owner.Name)
        : UiTextConfiguration.Current.VehiclesPageTitleText;
    // --- Pagination -----------------------------------------------------------
    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0
        ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize)
        : 0;
    // --- State ----------------------------------------------------------------
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    public bool IsEmpty => !IsLoading && Vehicles.Count == 0;
    // --- Popup l?i -----------------------------------------------------------
    [ObservableProperty] public partial bool IsPopupVisible { get; set; }
    [ObservableProperty] public partial bool IsPopupRetry { get; set; }
    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupButtonText { get; set; } = UiTextConfiguration.Current.PopupOkButtonText;
    public bool IsPopupNotRetry => !IsPopupRetry;
    // --- Form Add/Edit --------------------------------------------------------
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial VehicleDto? SelectedVehicle { get; set; }
    public string FormTitle => IsEditing
        ? UiTextConfiguration.Current.VehiclesFormTitleEditText
        : UiTextConfiguration.Current.VehiclesFormTitleCreateText;
    public string FormSaveText => IsEditing
        ? UiTextConfiguration.Current.CommonFormSaveChangesText
        : UiTextConfiguration.Current.VehiclesFormSaveCreateText;
    // Form fields
    [ObservableProperty] public partial string FormLicensePlate { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormModel { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormEngineNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormFrameNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial int FormYear { get; set; } = DateTime.Now.Year;
    [ObservableProperty] public partial double FormMileage { get; set; }
    [ObservableProperty] public partial DateTime FormRegistrationDate { get; set; } = DateTime.Today;
    [ObservableProperty] public partial DateTime? FormInsuranceExpiryDate { get; set; }
    // Enum values tuong ?ng v?i picker
    [ObservableProperty] public partial CarType FormType { get; set; } = CarType.None;
    [ObservableProperty] public partial CarColor FormColor { get; set; } = CarColor.None;
    [ObservableProperty] public partial CarBrand FormBrand { get; set; } = CarBrand.None;
    private static readonly CarBrand[] BrandValues =
    [
        CarBrand.None,
        CarBrand.Audi,
        CarBrand.Bentley,
        CarBrand.BMW,
        CarBrand.BYD,
        CarBrand.Bugatti,
        CarBrand.Buick,
        CarBrand.Cadillac,
        CarBrand.Chevrolet,
        CarBrand.Ford,
        CarBrand.Ferrari,
        CarBrand.Honda,
        CarBrand.Hyundai,
        CarBrand.Jaguar,
        CarBrand.Jeep,
        CarBrand.KIA,
        CarBrand.Lamborghini,
        CarBrand.LandRover,
        CarBrand.Lexus,
        CarBrand.Mazda,
        CarBrand.McLaren,
        CarBrand.MercedesBenz,
        CarBrand.Mitsubishi,
        CarBrand.Nissan,
        CarBrand.Porsche,
        CarBrand.RollsRoyce,
        CarBrand.Subaru,
        CarBrand.Suzuki,
        CarBrand.Tesla,
        CarBrand.Toyota,
        CarBrand.VinFast,
        CarBrand.Volvo,
        CarBrand.Volkswagen,
        CarBrand.Other
    ];
    private static readonly CarType[] TypeValues =
    [
        CarType.None,
        CarType.Sedan,
        CarType.SUV,
        CarType.Hatchback,
        CarType.Coupe,
        CarType.Convertible,
        CarType.Pickup,
        CarType.Minivan,
        CarType.Truck,
        CarType.Bus,
        CarType.Motorcycle,
        CarType.Other
    ];
    private static readonly CarColor[] ColorValues =
    [
        CarColor.None,
        CarColor.Black,
        CarColor.White,
        CarColor.Gray,
        CarColor.Silver,
        CarColor.Red,
        CarColor.Blue,
        CarColor.Green,
        CarColor.Yellow,
        CarColor.Brown,
        CarColor.Orange,
        CarColor.Purple,
        CarColor.Pink,
        CarColor.Cyan,
        CarColor.Other
    ];
    public string[] FormTypeOptions { get; } = [.. TypeValues.Select((v, idx) => idx == 0 ? UiTextConfiguration.Current.CommonSelectPlaceholderText : EnumText.Get(v))];
    public string[] FormBrandOptions { get; } = [.. BrandValues.Select((v, idx) => idx == 0 ? UiTextConfiguration.Current.CommonSelectPlaceholderText : EnumText.Get(v))];
    public string[] FormColorOptions { get; } = [.. ColorValues.Select((v, idx) => idx == 0 ? UiTextConfiguration.Current.CommonSelectPlaceholderText : EnumText.Get(v))];
    // Picker index cho form
    [ObservableProperty] public partial int FormPickerBrandIndex { get; set; } = 0;
    [ObservableProperty] public partial int FormPickerTypeIndex { get; set; } = 0;
    [ObservableProperty] public partial int FormPickerColorIndex { get; set; } = 0;
    public string SelectedFormBrandText =>
        FormBrandOptions[System.Math.Clamp(FormPickerBrandIndex, 0, FormBrandOptions.Length - 1)];
    public string SelectedFormTypeText =>
        FormTypeOptions[System.Math.Clamp(FormPickerTypeIndex, 0, FormTypeOptions.Length - 1)];
    public string SelectedFormColorText =>
        FormColorOptions[System.Math.Clamp(FormPickerColorIndex, 0, FormColorOptions.Length - 1)];
    // Form error
    [ObservableProperty] public partial bool HasFormError { get; set; }
    [ObservableProperty] public partial string? FormErrorMessage { get; set; }
    // --- Delete Confirm -------------------------------------------------------
    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }
    public string DeleteConfirmPlate => SelectedVehicle?.LicensePlate ?? string.Empty;
    // --- Collection -----------------------------------------------------------
    public System.Collections.ObjectModel.ObservableCollection<VehicleDto> Vehicles { get; } = [];
    // --- Constructor ---------------------------------------------------------
    public VehiclesViewModel(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
        Vehicles.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(SelectedFormBrandText));
            OnPropertyChanged(nameof(SelectedFormTypeText));
            OnPropertyChanged(nameof(SelectedFormColorText));
        });
    }
    /// <summary>
    /// �u?c g?i t? VehiclesPage.OnNavigatedTo sau khi Owner được set.
    /// </summary>
    public void Initialize(CustomerDto owner)
    {
        Owner = owner;
        OnPropertyChanged(nameof(PageTitle));
        _ = LoadAsync();
    }
    // --- Property Change Hooks ------------------------------------------------
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
    // Picker index ? enum (form)
    partial void OnFormPickerBrandIndexChanged(int value)
    {
        FormBrand = BrandValues[System.Math.Clamp(value, 0, BrandValues.Length - 1)];
        OnPropertyChanged(nameof(SelectedFormBrandText));
    }
    partial void OnFormPickerTypeIndexChanged(int value)
    {
        FormType = TypeValues[System.Math.Clamp(value, 0, TypeValues.Length - 1)];
        OnPropertyChanged(nameof(SelectedFormTypeText));
    }
    partial void OnFormPickerColorIndexChanged(int value)
    {
        FormColor = ColorValues[System.Math.Clamp(value, 0, ColorValues.Length - 1)];
        OnPropertyChanged(nameof(SelectedFormColorText));
    }
    // --- Commands -------------------------------------------------------------
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
                HandleError(UiTextConfiguration.Current.VehiclesErrorLoadFailedText, result.ErrorMessage!, result.Advice);
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
        FormLicensePlate = vehicle.LicensePlate ?? string.Empty;
        FormModel = vehicle.Model ?? string.Empty;
        FormEngineNumber = vehicle.EngineNumber ?? string.Empty;
        FormFrameNumber = vehicle.FrameNumber ?? string.Empty;
        FormYear = vehicle.Year;
        FormMileage = vehicle.Mileage;
        FormRegistrationDate = vehicle.RegistrationDate == default ? DateTime.Today : vehicle.RegistrationDate;
        FormInsuranceExpiryDate = vehicle.InsuranceExpiryDate;
        FormPickerBrandIndex = System.Array.IndexOf(BrandValues, vehicle.Brand);
        if (FormPickerBrandIndex < 0)
        {
            FormPickerBrandIndex = 0;
        }
        FormPickerTypeIndex = System.Array.IndexOf(TypeValues, vehicle.Type);
        if (FormPickerTypeIndex < 0)
        {
            FormPickerTypeIndex = 0;
        }
        FormPickerColorIndex = System.Array.IndexOf(ColorValues, vehicle.Color);
        if (FormPickerColorIndex < 0)
        {
            FormPickerColorIndex = 0;
        }
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
                        int idx = IndexOfVehicle(result.UpdatedEntity.VehicleId);
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
                SetFormError(result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText);
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
                HandleError(UiTextConfiguration.Current.VehiclesErrorDeleteFailedText, result.ErrorMessage!, result.Advice);
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
    // --- IDisposable ---------------------------------------------------------
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
    // --- Private Helpers -----------------------------------------------------
    private void ClearError() { HasError = false; ErrorMessage = null; }
    private void ClearForm()
    {
        FormLicensePlate = FormModel = FormEngineNumber = FormFrameNumber = string.Empty;
        FormYear = DateTime.Now.Year;
        FormMileage = 0;
        FormRegistrationDate = DateTime.Today;
        FormInsuranceExpiryDate = null;
        FormPickerBrandIndex = 0;
        FormPickerTypeIndex = 0;
        FormPickerColorIndex = 0;
        ClearFormError();
    }
    private void ClearFormError() { HasFormError = false; FormErrorMessage = null; }
    private void SetFormError(string message) { FormErrorMessage = message; HasFormError = true; }
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FormLicensePlate))
        { SetFormError(UiTextConfiguration.Current.VehiclesValidationPlateRequiredText); return false; }
        if (FormLicensePlate.Length > 20)
        { SetFormError(UiTextConfiguration.Current.VehiclesValidationPlateMaxLengthText); return false; }
        if (FormYear < 1900 || FormYear > DateTime.Now.Year + 1)
        { SetFormError(string.Format(System.Globalization.CultureInfo.CurrentCulture, UiTextConfiguration.Current.VehiclesValidationYearInvalidText, DateTime.Now.Year + 1)); return false; }
        if (FormMileage < 0)
        { SetFormError(UiTextConfiguration.Current.VehiclesValidationOdometerNonNegativeText); return false; }
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
    private int IndexOfVehicle(int? vehicleId)
    {
        if (vehicleId is null)
        {
            return -1;
        }
        for (int i = 0; i < Vehicles.Count; i++)
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
