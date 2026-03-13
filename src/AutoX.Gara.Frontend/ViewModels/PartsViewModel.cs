// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Results.Parts;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Inventory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// Unified ViewModel for managing parts (both spare parts and replacement parts).
/// Combines functionality from ReplacementPartsViewModel and SparePartsViewModel.
/// </summary>
public sealed partial class PartsViewModel : ObservableObject, System.IDisposable
{
    private readonly PartService _service;
    private System.Threading.CancellationTokenSource? _loadCts;
    private System.Threading.CancellationTokenSource? _writeCts;
    private System.Threading.CancellationTokenSource? _searchCts;

    private const System.Int32 DefaultPageSize = 20;
    private const System.Int32 SearchDebounceMs = 400;

    // ─── Pagination ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }
    [ObservableProperty] public partial System.Int32 TotalCount { get; set; }

    public System.Int32 TotalPages => TotalCount > 0
        ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize)
        : 0;

    // ─── Search / Sort ────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.String SearchTerm { get; set; } = System.String.Empty;
    [ObservableProperty] public partial PartSortField SortBy { get; set; } = PartSortField.PartName;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = false;

    // ─── Filter ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 FilterSupplierId { get; set; } = 0;
    [ObservableProperty] public partial PartCategory? FilterCategory { get; set; } = null;
    [ObservableProperty] public partial System.Boolean? FilterInStock { get; set; } = null;
    [ObservableProperty] public partial System.Boolean? FilterDefective { get; set; } = null;
    [ObservableProperty] public partial System.Boolean? FilterExpired { get; set; } = null;
    [ObservableProperty] public partial System.Boolean? FilterDiscontinued { get; set; } = null;

    // Picker indices for filter UI
    [ObservableProperty] public partial System.Int32 PickerCategoryIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerInStockIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerDefectiveIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerExpiredIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerDiscontinuedIndex { get; set; } = 0;

    public System.Boolean HasActiveFilters
        => FilterSupplierId != 0 || FilterCategory.HasValue || FilterInStock.HasValue ||
           FilterDefective.HasValue || FilterExpired.HasValue || FilterDiscontinued.HasValue;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Parts.Count == 0;

    // ─── Popup Error ──────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form Add/Edit ────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }
    [ObservableProperty] public partial PartDto? SelectedPart { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa phụ tùng" : "Thêm phụ tùng";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm phụ tùng";

    [ObservableProperty] public partial System.String FormPartCode { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPartName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormManufacturer { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPurchasePrice { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormSellingPrice { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormInventoryQuantity { get; set; } = "0";
    [ObservableProperty] public partial System.Int32 FormSupplierId { get; set; }
    [ObservableProperty] public partial System.Int32 FormPickerCategoryIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Boolean FormIsDefective { get; set; } = false;
    [ObservableProperty] public partial System.Boolean FormIsDiscontinued { get; set; } = false;
    [ObservableProperty] public partial System.DateTime FormDateAdded { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.DateTime? FormExpiryDate { get; set; } = null;
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Delete/Discontinue Confirm ───────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }
    public System.String DeleteConfirmName => SelectedPart?.PartName ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<PartDto> Parts { get; } = [];

    // ─── Constructor ───────────────────────────────────────────────────────────

    public PartsViewModel(PartService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Parts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedPartChanged(PartDto? value) => OnPropertyChanged(nameof(DeleteConfirmName));
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
    partial void OnSearchTermChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new System.Threading.CancellationTokenSource();
        var token = _searchCts.Token;
        _ = DebounceSearchAsync(token);
    }
    private async System.Threading.Tasks.Task DebounceSearchAsync(System.Threading.CancellationToken token)
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(SearchDebounceMs, token).ConfigureAwait(false);
            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (CurrentPage != 1)
                {
                    CurrentPage = 1;
                }
                else
                {
                    _ = LoadAsync();
                }
            });
        }
        catch (System.OperationCanceledException) { }
    }
    partial void OnSortByChanged(PartSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();

    partial void OnFilterSupplierIdChanged(int value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterCategoryChanged(PartCategory? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterInStockChanged(System.Boolean? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterDefectiveChanged(System.Boolean? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterExpiredChanged(System.Boolean? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterDiscontinuedChanged(System.Boolean? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnPickerCategoryIndexChanged(int value)
    {
        FilterCategory = value switch
        {
            1 => PartCategory.Engine,
            2 => PartCategory.Brake,
            3 => PartCategory.Suspension,
            4 => PartCategory.Electrical,
            5 => PartCategory.Body,
            6 => PartCategory.Transmission,
            7 => PartCategory.Cooling,
            8 => PartCategory.Exhaust,
            _ => null
        };
    }
    partial void OnPickerInStockIndexChanged(int value)
    {
        FilterInStock = value switch { 1 => true, 2 => false, _ => null };
    }
    partial void OnPickerDefectiveIndexChanged(int value)
    {
        FilterDefective = value switch { 1 => false, 2 => true, _ => null };
    }
    partial void OnPickerExpiredIndexChanged(int value)
    {
        FilterExpired = value switch { 1 => false, 2 => true, _ => null };
    }
    partial void OnPickerDiscontinuedIndexChanged(int value)
    {
        FilterDiscontinued = value switch { 1 => false, 2 => true, _ => null };
    }
    partial void OnFormPickerCategoryIndexChanged(int value)
    {
        FormCategory = value switch
        {
            1 => PartCategory.Engine,
            2 => PartCategory.Brake,
            3 => PartCategory.Suspension,
            4 => PartCategory.Electrical,
            5 => PartCategory.Body,
            6 => PartCategory.Transmission,
            7 => PartCategory.Cooling,
            8 => PartCategory.Exhaust,
            _ => PartCategory.Other
        };
    }
    private PartCategory FormCategory { get; set; } = PartCategory.Other;

    // ─── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new System.Threading.CancellationTokenSource();
        var ct = _loadCts.Token;

        ClearError();
        IsLoading = true;

        try
        {
            PartListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterSupplierId: FilterSupplierId == 0 ? null : FilterSupplierId,
                filterCategory: FilterCategory,
                filterInStock: FilterInStock,
                filterDefective: FilterDefective,
                filterExpired: FilterExpired,
                filterDiscontinued: FilterDiscontinued,
                ct: ct);

            Debug.WriteLine($"[PartsVM] Load ok={result.IsSuccess} count={result.Parts.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Parts.Clear();
                foreach (PartDto p in result.Parts)
                {
                    Parts.Add(p);
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
    private void ClearFilters()
    {
        PickerCategoryIndex = 0;
        PickerInStockIndex = 0;
        PickerDefectiveIndex = 0;
        PickerExpiredIndex = 0;
        PickerDiscontinuedIndex = 0;
        FilterSupplierId = 0;
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedPart = null;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(PartDto part)
    {
        IsEditing = true;
        SelectedPart = part;

        FormPartCode = part.PartCode ?? System.String.Empty;
        FormPartName = part.PartName ?? System.String.Empty;
        FormManufacturer = part.Manufacturer ?? System.String.Empty;
        FormPurchasePrice = part.PurchasePrice.ToString("0.##");
        FormSellingPrice = part.SellingPrice.ToString("0.##");
        FormInventoryQuantity = part.InventoryQuantity.ToString();
        FormSupplierId = part.SupplierId;
        FormIsDefective = part.IsDefective;
        FormIsDiscontinued = part.IsDiscontinued;
        FormDateAdded = part.DateAdded.ToDateTime(System.TimeOnly.MinValue);
        FormExpiryDate = part.ExpiryDate.HasValue
            ? part.ExpiryDate.Value.ToDateTime(System.TimeOnly.MinValue)
            : null;

        FormPickerCategoryIndex = (part.PartCategory ?? PartCategory.Other) switch
        {
            PartCategory.Engine => 1,
            PartCategory.Brake => 2,
            PartCategory.Suspension => 3,
            PartCategory.Electrical => 4,
            PartCategory.Body => 5,
            PartCategory.Transmission => 6,
            PartCategory.Cooling => 7,
            PartCategory.Exhaust => 8,
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

        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new System.Threading.CancellationTokenSource();
        var ct = _writeCts.Token;

        IsLoading = true;

        try
        {
            PartDto data = BuildPacketFromForm();
            PartWriteResult result = IsEditing
                ? await _service.UpdateAsync(data, ct)
                : await _service.CreateAsync(data, ct);

            if (result.IsSuccess)
            {
                IsFormVisible = false;
                ClearForm();

                if (result.UpdatedEntity is not null)
                {
                    if (IsEditing)
                    {
                        System.Int32 idx = IndexOfPart(result.UpdatedEntity.PartId);
                        if (idx >= 0)
                        {
                            Parts[idx] = result.UpdatedEntity;
                        }
                        else
                        {
                            await LoadAsync();
                        }
                    }
                    else
                    {
                        Parts.Insert(0, result.UpdatedEntity);
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
    private void RequestDelete(PartDto part)
    {
        SelectedPart = part;
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelDelete() => IsDeleteConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedPart is null)
        {
            return;
        }

        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new System.Threading.CancellationTokenSource();
        var ct = _writeCts.Token;

        IsDeleteConfirmVisible = false;
        IsLoading = true;

        PartDto toDelete = SelectedPart;

        try
        {
            PartWriteResult result = await _service.DeleteAsync(toDelete, ct);

            if (result.IsSuccess)
            {
                Parts.Remove(toDelete);
                TotalCount = System.Math.Max(0, TotalCount - 1);
                SelectedPart = null;
                OnPropertyChanged(nameof(TotalPages));
                HasNextPage = CurrentPage < TotalPages;

                if (Parts.Count == 0 && CurrentPage > 1)
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

    [RelayCommand]
    private void ClosePopup() => IsPopupVisible = false;

    [RelayCommand]
    private void RetryLoad()
    {
        IsPopupVisible = false;
        _ = LoadAsync();
    }

    public void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private void ResetPageAndLoad()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
        }
        else
        {
            _ = LoadAsync();
        }
    }

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    private void ClearForm()
    {
        FormPartCode = System.String.Empty;
        FormPartName = System.String.Empty;
        FormManufacturer = System.String.Empty;
        FormPurchasePrice = System.String.Empty;
        FormSellingPrice = System.String.Empty;
        FormInventoryQuantity = "0";
        FormSupplierId = 0;
        FormPickerCategoryIndex = 0;
        FormIsDefective = false;
        FormIsDiscontinued = false;
        FormDateAdded = System.DateTime.Today;
        FormExpiryDate = null;
        ClearFormError();
    }

    private void ClearFormError()
    {
        HasFormError = false;
        FormErrorMessage = null;
    }

    private void SetFormError(System.String message)
    {
        FormErrorMessage = message;
        HasFormError = true;
    }

    private System.Boolean ValidateForm()
    {
        if (IsEditing && System.String.IsNullOrWhiteSpace(FormPartCode))
        { SetFormError("Mã SKU không được để trống."); return false; }

        if (!IsEditing)
        {
            if (System.String.IsNullOrWhiteSpace(FormPartCode))
            { SetFormError("Mã SKU không được để trống."); return false; }

            if (FormPartCode.Length > 12 || !System.Text.RegularExpressions.Regex.IsMatch(FormPartCode, @"^[A-Za-z0-9]+$"))
            { SetFormError("Mã SKU tối đa 12 ký tự, chỉ gồm chữ và số."); return false; }
        }

        if (System.String.IsNullOrWhiteSpace(FormPartName))
        { SetFormError("Tên phụ tùng không được để trống."); return false; }

        if (FormPartName.Length > 100)
        { SetFormError("Tên không được vượt quá 100 ký tự."); return false; }

        if (!System.Decimal.TryParse(FormPurchasePrice, out System.Decimal purchase) || purchase <= 0)
        { SetFormError("Giá nhập không hợp lệ."); return false; }

        if (!System.Decimal.TryParse(FormSellingPrice, out System.Decimal selling) || selling <= 0)
        { SetFormError("Giá bán không hợp lệ."); return false; }

        if (selling < purchase)
        { SetFormError("Giá bán không được thấp hơn giá nhập."); return false; }

        if (!System.Int32.TryParse(FormInventoryQuantity, out System.Int32 qty) || qty < 0)
        { SetFormError("Số lượng phải là số nguyên không âm."); return false; }

        if (FormSupplierId <= 0)
        { SetFormError("Vui lòng chọn nhà cung cấp."); return false; }

        if (FormExpiryDate.HasValue && FormExpiryDate.Value.Date < FormDateAdded.Date)
        { SetFormError("Ngày hết hạn phải sau ngày nhập kho."); return false; }

        return true;
    }

    private PartDto BuildPacketFromForm()
    {
        _ = System.Decimal.TryParse(FormPurchasePrice, out System.Decimal purchase);
        _ = System.Decimal.TryParse(FormSellingPrice, out System.Decimal selling);
        _ = System.Int32.TryParse(FormInventoryQuantity, out System.Int32 qty);

        return new PartDto
        {
            PartId = IsEditing ? SelectedPart?.PartId : null,
            PartCode = FormPartCode,
            PartName = FormPartName,
            Manufacturer = FormManufacturer,
            PurchasePrice = purchase,
            SellingPrice = selling,
            InventoryQuantity = qty,
            SupplierId = FormSupplierId,
            PartCategory = FormCategory,
            IsDefective = FormIsDefective,
            IsDiscontinued = FormIsDiscontinued,
            DateAdded = System.DateOnly.FromDateTime(FormDateAdded),
            ExpiryDate = FormExpiryDate.HasValue
                ? System.DateOnly.FromDateTime(FormExpiryDate.Value)
                : null
        };
    }

    private System.Int32 IndexOfPart(System.Object? partId)
    {
        if (partId is null)
        {
            return -1;
        }

        for (System.Int32 i = 0; i < Parts.Count; i++)
        {
            if (Parts[i].PartId is System.Object id && id.Equals(partId))
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
                ShowPopup(title, message, isRetry: false);
                break;
            case ProtocolAdvice.BACKOFF_RETRY:
                ShowPopup(title, message, isRetry: true);
                break;
            default:
                HasError = true;
                ErrorMessage = message;
                break;
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