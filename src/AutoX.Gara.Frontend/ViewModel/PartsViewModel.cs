// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Parts;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Inventory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace AutoX.Gara.Frontend.Controllers;
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
    private const int DefaultPageSize = 20;
    private const int SearchDebounceMs = 400;
    // --- Pagination -----------------------------------------------------------
    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0
        ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize)
        : 0;
    // --- Search / Sort --------------------------------------------------------
    [ObservableProperty] public partial string SearchTerm { get; set; } = string.Empty;
    [ObservableProperty] public partial PartSortField SortBy { get; set; } = PartSortField.PartName;
    [ObservableProperty] public partial bool SortDescending { get; set; } = false;
    // --- Filter ---------------------------------------------------------------
    [ObservableProperty] public partial int FilterSupplierId { get; set; } = 0;
    [ObservableProperty] public partial PartCategory? FilterCategory { get; set; } = null;
    [ObservableProperty] public partial bool? FilterInStock { get; set; } = null;
    [ObservableProperty] public partial bool? FilterDefective { get; set; } = null;
    [ObservableProperty] public partial bool? FilterExpired { get; set; } = null;
    [ObservableProperty] public partial bool? FilterDiscontinued { get; set; } = null;
    // Picker indices for filter UI
    [ObservableProperty] public partial int PickerCategoryIndex { get; set; } = 0;
    [ObservableProperty] public partial int PickerInStockIndex { get; set; } = 0;
    [ObservableProperty] public partial int PickerDefectiveIndex { get; set; } = 0;
    [ObservableProperty] public partial int PickerExpiredIndex { get; set; } = 0;
    [ObservableProperty] public partial int PickerDiscontinuedIndex { get; set; } = 0;
    public string[] InStockFilterOptions { get; } =
    [
        UiTextConfiguration.Current.CommonFilterAllText,
        UiTextConfiguration.Current.PartsFilterInStockText,
        UiTextConfiguration.Current.PartsFilterOutOfStockText
    ];
    public string[] DefectiveFilterOptions { get; } =
    [
        UiTextConfiguration.Current.CommonFilterAllText,
        UiTextConfiguration.Current.PartsFilterNormalText,
        UiTextConfiguration.Current.PartsFilterDefectiveText
    ];
    public string[] ExpiredFilterOptions { get; } =
    [
        UiTextConfiguration.Current.CommonFilterAllText,
        UiTextConfiguration.Current.PartsFilterNotExpiredText,
        UiTextConfiguration.Current.PartsFilterExpiredText
    ];
    public string[] DiscontinuedFilterOptions { get; } =
    [
        UiTextConfiguration.Current.CommonFilterAllText,
        UiTextConfiguration.Current.PartsFilterActiveText,
        UiTextConfiguration.Current.PartsFilterDiscontinuedText
    ];
    public string SelectedCategoryText =>
        PartCategoryFilterOptions[System.Math.Clamp(PickerCategoryIndex, 0, PartCategoryFilterOptions.Count - 1)];
    public string SelectedInStockText =>
        InStockFilterOptions[System.Math.Clamp(PickerInStockIndex, 0, InStockFilterOptions.Length - 1)];
    public string SelectedDefectiveText =>
        DefectiveFilterOptions[System.Math.Clamp(PickerDefectiveIndex, 0, DefectiveFilterOptions.Length - 1)];
    public string SelectedExpiredText =>
        ExpiredFilterOptions[System.Math.Clamp(PickerExpiredIndex, 0, ExpiredFilterOptions.Length - 1)];
    public string SelectedDiscontinuedText =>
        DiscontinuedFilterOptions[System.Math.Clamp(PickerDiscontinuedIndex, 0, DiscontinuedFilterOptions.Length - 1)];
    public string SelectedFormCategoryText =>
        PartCategoryFormOptions[System.Math.Clamp(FormPickerCategoryIndex, 0, PartCategoryFormOptions.Count - 1)];
    // Picker options (auto from enum Display attributes)
    public IReadOnlyList<string> PartCategoryFilterOptions { get; }
    public IReadOnlyList<string> PartCategoryFormOptions { get; }
    private readonly PartCategory?[] _partCategoryFilterValues;
    private readonly PartCategory[] _partCategoryFormValues;
    public bool HasActiveFilters
        => FilterSupplierId != 0 || FilterCategory.HasValue || FilterInStock.HasValue ||
           FilterDefective.HasValue || FilterExpired.HasValue || FilterDiscontinued.HasValue;
    // --- State ----------------------------------------------------------------
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    public bool IsEmpty => !IsLoading && Parts.Count == 0;
    // --- Popup Error ----------------------------------------------------------
    [ObservableProperty] public partial bool IsPopupVisible { get; set; }
    [ObservableProperty] public partial bool IsPopupRetry { get; set; }
    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupButtonText { get; set; } = UiTextConfiguration.Current.PopupOkButtonText;
    public bool IsPopupNotRetry => !IsPopupRetry;
    // --- Form Add/Edit --------------------------------------------------------
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial PartDto? SelectedPart { get; set; }
    public string FormTitle =>
        IsEditing
            ? UiTextConfiguration.Current.PartsFormTitleEditText
            : UiTextConfiguration.Current.PartsFormTitleCreateText;
    public string FormSaveText =>
        IsEditing
            ? UiTextConfiguration.Current.CommonFormSaveChangesText
            : UiTextConfiguration.Current.PartsFormSaveCreateText;
    [ObservableProperty] public partial string FormPartCode { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormPartName { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormManufacturer { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormPurchasePrice { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormSellingPrice { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormInventoryQuantity { get; set; } = "0";
    [ObservableProperty] public partial int FormSupplierId { get; set; }
    [ObservableProperty] public partial int FormPickerCategoryIndex { get; set; } = 0;
    [ObservableProperty] public partial bool FormIsDefective { get; set; } = false;
    [ObservableProperty] public partial bool FormIsDiscontinued { get; set; } = false;
    [ObservableProperty] public partial DateTime FormDateAdded { get; set; } = DateTime.Today;
    [ObservableProperty] public partial DateTime? FormExpiryDate { get; set; } = null;
    [ObservableProperty] public partial bool HasFormError { get; set; }
    [ObservableProperty] public partial string? FormErrorMessage { get; set; }
    // --- Delete/Discontinue Confirm -------------------------------------------
    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }
    public string DeleteConfirmName => SelectedPart?.PartName ?? string.Empty;
    // --- Collection -----------------------------------------------------------
    public System.Collections.ObjectModel.ObservableCollection<PartDto> Parts { get; } = [];
    // --- Constructor -----------------------------------------------------------
    public PartsViewModel(PartService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Parts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
        // Build category pickers from enum so UI stays in sync when enum grows.
        PartCategory[] allCategories = [.. System.Enum.GetValues<PartCategory>().Where(c => c != PartCategory.None)];
        // Filter: "T?t c? lo?i" + all enum values (excluding None)
        _partCategoryFilterValues = new PartCategory?[allCategories.Length + 1];
        _partCategoryFilterValues[0] = null;
        for (Int32 i = 0; i < allCategories.Length; i++)
        {
            _partCategoryFilterValues[i + 1] = allCategories[i];
        }
        PartCategoryFilterOptions = [UiTextConfiguration.Current.PartsFilterCategoryAllText, .. allCategories.Select(EnumText.Get)];
        // Form: put "Other" first for convenience, then remaining values (excluding None/Other).
        PartCategory[] rest = [.. allCategories.Where(c => c != PartCategory.Other)];
        _partCategoryFormValues = [PartCategory.Other, .. rest];
        PartCategoryFormOptions = [.. _partCategoryFormValues.Select(EnumText.Get)];
        // Force initial "filter button" texts to render (MAUI/WinUI can show blank until changed).
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(SelectedCategoryText));
            OnPropertyChanged(nameof(SelectedInStockText));
            OnPropertyChanged(nameof(SelectedDefectiveText));
            OnPropertyChanged(nameof(SelectedExpiredText));
            OnPropertyChanged(nameof(SelectedDiscontinuedText));
            OnPropertyChanged(nameof(SelectedFormCategoryText));
        });
    }
    // --- Property Change Hooks ------------------------------------------------
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
    partial void OnFilterInStockChanged(bool? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterDefectiveChanged(bool? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterExpiredChanged(bool? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterDiscontinuedChanged(bool? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnPickerCategoryIndexChanged(int value)
    {
        if (value < 0 || value >= _partCategoryFilterValues.Length)
        {
            FilterCategory = null;
            return;
        }
        FilterCategory = _partCategoryFilterValues[value];
        OnPropertyChanged(nameof(SelectedCategoryText));
    }
    partial void OnPickerInStockIndexChanged(int value)
    {
        FilterInStock = value switch { 1 => true, 2 => false, _ => null };
        OnPropertyChanged(nameof(SelectedInStockText));
    }
    partial void OnPickerDefectiveIndexChanged(int value)
    {
        FilterDefective = value switch { 1 => false, 2 => true, _ => null };
        OnPropertyChanged(nameof(SelectedDefectiveText));
    }
    partial void OnPickerExpiredIndexChanged(int value)
    {
        FilterExpired = value switch { 1 => false, 2 => true, _ => null };
        OnPropertyChanged(nameof(SelectedExpiredText));
    }
    partial void OnPickerDiscontinuedIndexChanged(int value)
    {
        FilterDiscontinued = value switch { 1 => false, 2 => true, _ => null };
        OnPropertyChanged(nameof(SelectedDiscontinuedText));
    }
    partial void OnFormPickerCategoryIndexChanged(int value)
    {
        if (value < 0 || value >= _partCategoryFormValues.Length)
        {
            FormCategory = PartCategory.Other;
            return;
        }
        FormCategory = _partCategoryFormValues[value];
        OnPropertyChanged(nameof(SelectedFormCategoryText));
    }
    private PartCategory FormCategory { get; set; } = PartCategory.Other;
    // --- Commands -------------------------------------------------------------
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
                HandleError(UiTextConfiguration.Current.PartsErrorLoadFailedText, result.ErrorMessage!, result.Advice);
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
        FormPartCode = part.PartCode ?? string.Empty;
        FormPartName = part.PartName ?? string.Empty;
        FormManufacturer = part.Manufacturer ?? string.Empty;
        FormPurchasePrice = part.PurchasePrice.ToString("0.##");
        FormSellingPrice = part.SellingPrice.ToString("0.##");
        FormInventoryQuantity = part.InventoryQuantity.ToString();
        FormSupplierId = part.SupplierId;
        FormIsDefective = part.IsDefective;
        FormIsDiscontinued = part.IsDiscontinued;
        FormDateAdded = part.DateAdded.ToDateTime(TimeOnly.MinValue);
        FormExpiryDate = part.ExpiryDate.HasValue ? part.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        PartCategory cat = part.PartCategory ?? PartCategory.Other;
        Int32 idx = System.Array.IndexOf(_partCategoryFormValues, cat);
        FormPickerCategoryIndex = idx >= 0 ? idx : 0;
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
                        int idx = IndexOfPart(result.UpdatedEntity.PartId);
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
                SetFormError(result.ErrorMessage ?? UiTextConfiguration.Current.CommonErrorActionFailedText);
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
                HandleError(UiTextConfiguration.Current.PartsErrorDeleteFailedText, result.ErrorMessage!, result.Advice);
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
    // --- Private Helpers ------------------------------------------------------
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
        FormPartCode = string.Empty;
        FormPartName = string.Empty;
        FormManufacturer = string.Empty;
        FormPurchasePrice = string.Empty;
        FormSellingPrice = string.Empty;
        FormInventoryQuantity = "0";
        FormSupplierId = 0;
        FormPickerCategoryIndex = 0;
        FormIsDefective = false;
        FormIsDiscontinued = false;
        FormDateAdded = DateTime.Today;
        FormExpiryDate = null;
        ClearFormError();
    }
    private void ClearFormError()
    {
        HasFormError = false;
        FormErrorMessage = null;
    }
    private void SetFormError(string message)
    {
        FormErrorMessage = message;
        HasFormError = true;
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "<Pending>")]
    private bool ValidateForm()
    {
        if (IsEditing && string.IsNullOrWhiteSpace(FormPartCode))
        { SetFormError(UiTextConfiguration.Current.PartsValidationSkuRequiredText); return false; }
        if (!IsEditing)
        {
            if (string.IsNullOrWhiteSpace(FormPartCode))
            { SetFormError(UiTextConfiguration.Current.PartsValidationSkuRequiredText); return false; }
            if (FormPartCode.Length > 12 || !System.Text.RegularExpressions.Regex.IsMatch(FormPartCode, @"^[A-Za-z0-9]+$"))
            { SetFormError(UiTextConfiguration.Current.PartsValidationSkuFormatText); return false; }
        }
        if (string.IsNullOrWhiteSpace(FormPartName))
        { SetFormError(UiTextConfiguration.Current.PartsValidationNameRequiredText); return false; }
        if (FormPartName.Length > 100)
        { SetFormError(UiTextConfiguration.Current.PartsValidationNameMaxLengthText); return false; }
        if (!decimal.TryParse(FormPurchasePrice, out decimal purchase) || purchase <= 0)
        { SetFormError(UiTextConfiguration.Current.PartsValidationCostInvalidText); return false; }
        if (!decimal.TryParse(FormSellingPrice, out decimal selling) || selling <= 0)
        { SetFormError(UiTextConfiguration.Current.PartsValidationPriceInvalidText); return false; }
        if (selling < purchase)
        { SetFormError(UiTextConfiguration.Current.PartsValidationPriceNotLessThanCostText); return false; }
        if (!int.TryParse(FormInventoryQuantity, out int qty) || qty < 0)
        { SetFormError(UiTextConfiguration.Current.PartsValidationQuantityNonNegativeText); return false; }
        if (FormSupplierId <= 0)
        { SetFormError(UiTextConfiguration.Current.PartsValidationSupplierRequiredText); return false; }
        if (FormExpiryDate.HasValue && FormExpiryDate.Value.Date < FormDateAdded.Date)
        { SetFormError(UiTextConfiguration.Current.PartsValidationExpiryAfterReceivedText); return false; }
        return true;
    }
    private PartDto BuildPacketFromForm()
    {
        _ = decimal.TryParse(FormPurchasePrice, out decimal purchase);
        _ = decimal.TryParse(FormSellingPrice, out decimal selling);
        _ = int.TryParse(FormInventoryQuantity, out int qty);
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
            DateAdded = DateOnly.FromDateTime(FormDateAdded),
            ExpiryDate = FormExpiryDate.HasValue
                ? DateOnly.FromDateTime(FormExpiryDate.Value)
                : null
        };
    }
    private int IndexOfPart(System.Object? partId)
    {
        if (partId is null)
        {
            return -1;
        }
        for (int i = 0; i < Parts.Count; i++)
        {
            if (Parts[i].PartId is System.Object id && id.Equals(partId))
            {
                return i;
            }
        }
        return -1;
    }
    private void HandleError(string title, string message, ProtocolAdvice advice)
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
