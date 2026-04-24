// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.ServiceItems;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



namespace AutoX.Gara.Frontend.Controllers.Billings;



/// <summary>



/// ViewModel for service item management.



/// </summary>



public sealed partial class ServiceItemsViewModel : ObservableObject, System.IDisposable



{

    private readonly ServiceItemService _service;



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



    [ObservableProperty] public partial ServiceItemSortField SortBy { get; set; } = ServiceItemSortField.Description;



    [ObservableProperty] public partial bool SortDescending { get; set; } = false;



    // --- Filter ---------------------------------------------------------------



    [ObservableProperty] public partial ServiceType? FilterType { get; set; } = null;



    [ObservableProperty] public partial decimal? FilterMinPrice { get; set; } = null;



    [ObservableProperty] public partial decimal? FilterMaxPrice { get; set; } = null;



    [ObservableProperty] public partial int PickerTypeIndex { get; set; } = 0;



    // --- Service Type Names (Enum Display) -------------------------------------



    public IReadOnlyList<string> ServiceTypeFilterOptions { get; }



    public IReadOnlyList<string> ServiceTypeFormOptions { get; }



    private readonly ServiceType?[] _serviceTypeFilterValues;



    private readonly ServiceType[] _serviceTypeFormValues;



    public bool HasActiveFilters



        => FilterType.HasValue || FilterMinPrice.HasValue || FilterMaxPrice.HasValue;



    public string SelectedFilterTypeText =>



        ServiceTypeFilterOptions[System.Math.Clamp(PickerTypeIndex, 0, ServiceTypeFilterOptions.Count - 1)];



    // --- State ----------------------------------------------------------------



    [ObservableProperty] public partial bool IsLoading { get; set; }



    [ObservableProperty] public partial bool HasError { get; set; }



    [ObservableProperty] public partial string? ErrorMessage { get; set; }



    public bool IsEmpty => !IsLoading && ServiceItems.Count == 0;



    // --- Popup ----------------------------------------------------------------



    [ObservableProperty] public partial bool IsPopupVisible { get; set; }



    [ObservableProperty] public partial bool IsPopupRetry { get; set; }



    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupButtonText { get; set; } = "OK";



    public bool IsPopupNotRetry => !IsPopupRetry;



    // --- Form ------------------------------------------------------------------



    [ObservableProperty] public partial bool IsFormVisible { get; set; }



    [ObservableProperty] public partial bool IsEditing { get; set; }



    [ObservableProperty] public partial ServiceItemDto? SelectedServiceItem { get; set; }



    public string FormTitle => IsEditing ? "S?a d?ch v?" : "Th�m d?ch v?";



    public string FormSaveText => IsEditing ? "Luu thay d?i" : "Th�m d?ch v?";



    [ObservableProperty] public partial string FormDescription { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormUnitPrice { get; set; } = string.Empty;



    [ObservableProperty] public partial int FormTypeIndex { get; set; } = 0;



    [ObservableProperty] public partial bool HasFormError { get; set; }



    [ObservableProperty] public partial string? FormErrorMessage { get; set; }



    public string SelectedFormTypeText =>



        ServiceTypeFormOptions[System.Math.Clamp(FormTypeIndex, 0, ServiceTypeFormOptions.Count - 1)];



    // --- Delete Confirm -------------------------------------------------------



    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }



    public string DeleteConfirmName => SelectedServiceItem?.Description ?? string.Empty;



    // --- Collection -----------------------------------------------------------



    public System.Collections.ObjectModel.ObservableCollection<ServiceItemDto> ServiceItems { get; } = [];



    // --- Constructor -----------------------------------------------------------



    public ServiceItemsViewModel(ServiceItemService service)



    {

        _service = service ?? throw new ArgumentNullException(nameof(service));



        ServiceItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));



        // ServiceType enum values are not contiguous (1..6, 10.., 255), so we must not map SelectedIndex == (int)enum.



        ServiceType[] all = System.Enum.GetValues<ServiceType>()



            .Where(v => v != ServiceType.None)



            .ToArray();



        _serviceTypeFilterValues = new ServiceType?[all.Length + 1];



        _serviceTypeFilterValues[0] = null;



        for (Int32 i = 0; i < all.Length; i++)



        {

            _serviceTypeFilterValues[i + 1] = all[i];



        }



        ServiceTypeFilterOptions = new[] { "T?t c? lo?i" }



            .Concat(all.Select(EnumText.Get))



            .ToArray();



        _serviceTypeFormValues = all;



        ServiceTypeFormOptions = new[] { "� Ch?n lo?i �" }



            .Concat(all.Select(EnumText.Get))



            .ToArray();



        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>



        {

            OnPropertyChanged(nameof(SelectedFilterTypeText));



            OnPropertyChanged(nameof(SelectedFormTypeText));



        });



    }



    // --- Property Hooks -------------------------------------------------------



    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));



    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));



    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));



    partial void OnSelectedServiceItemChanged(ServiceItemDto? value) => OnPropertyChanged(nameof(DeleteConfirmName));



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



    partial void OnSortByChanged(ServiceItemSortField value) => _ = LoadAsync();



    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();



    partial void OnFilterTypeChanged(ServiceType? value)



    {

        OnPropertyChanged(nameof(HasActiveFilters));



        ResetPageAndLoad();



    }



    partial void OnFilterMinPriceChanged(decimal? value)



    {

        OnPropertyChanged(nameof(HasActiveFilters));



        ResetPageAndLoad();



    }



    partial void OnFilterMaxPriceChanged(decimal? value)



    {

        OnPropertyChanged(nameof(HasActiveFilters));



        ResetPageAndLoad();



    }



    partial void OnPickerTypeIndexChanged(int value)



    {

        if (value < 0 || value >= _serviceTypeFilterValues.Length)



        {

            FilterType = null;



            return;



        }



        FilterType = _serviceTypeFilterValues[value];



        OnPropertyChanged(nameof(SelectedFilterTypeText));



    }



    partial void OnFormTypeIndexChanged(int value)



    {

        OnPropertyChanged(nameof(SelectedFormTypeText));



    }



    // --- Commands --------------------------------------------------------------



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

            ServiceItemListResult result = await _service.GetListAsync(



                page: CurrentPage,



                pageSize: DefaultPageSize,



                searchTerm: SearchTerm,



                sortBy: SortBy,



                sortDescending: SortDescending,



                filterType: FilterType,



                filterMinUnitPrice: FilterMinPrice,



                filterMaxUnitPrice: FilterMaxPrice,



                ct: ct);



            Debug.WriteLine($"[ServiceItemsVM] Load ok={result.IsSuccess} count={result.ServiceItems.Count} total={result.TotalCount}");



            if (ct.IsCancellationRequested)



            {

                return;



            }



            if (result.IsSuccess)



            {

                ServiceItems.Clear();



                foreach (ServiceItemDto item in result.ServiceItems)



                {

                    ServiceItems.Add(item);



                }



                TotalCount = result.TotalCount >= 0 ? result.TotalCount : TotalCount;



                HasNextPage = result.TotalCount >= 0



                    ? CurrentPage < TotalPages



                    : result.HasMore;



            }



            else



            {

                HandleError("T?i danh s�ch th?t b?i", result.ErrorMessage!, result.Advice);



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

        PickerTypeIndex = 0;



        FilterMinPrice = null;



        FilterMaxPrice = null;



    }



    [RelayCommand]



    private void OpenCreateForm()



    {

        IsEditing = false;



        SelectedServiceItem = null;



        ClearForm();



        IsFormVisible = true;



    }



    [RelayCommand]



    private void OpenEditForm(ServiceItemDto item)



    {

        IsEditing = true;



        SelectedServiceItem = item;



        FormDescription = item.Description ?? string.Empty;



        FormUnitPrice = item.UnitPrice.ToString("0.##");



        Int32 idx = System.Array.IndexOf(_serviceTypeFormValues, item.Type);



        FormTypeIndex = idx >= 0 ? idx + 1 : 0;



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

            ServiceItemDto data = BuildPacketFromForm();



            ServiceItemWriteResult result = IsEditing



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

                        int idx = IndexOfServiceItem(result.UpdatedEntity.ServiceItemId);



                        if (idx >= 0)



                        {

                            ServiceItems[idx] = result.UpdatedEntity;



                        }



                        else



                        {

                            await LoadAsync();



                        }



                    }



                    else



                    {

                        ServiceItems.Insert(0, result.UpdatedEntity);



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

                SetFormError(result.ErrorMessage ?? "Thao t�c th?t b?i.");



            }



        }



        finally



        {

            IsLoading = false;



        }



    }



    [RelayCommand]



    private void RequestDelete(ServiceItemDto item)



    {

        SelectedServiceItem = item;



        IsDeleteConfirmVisible = true;



    }



    [RelayCommand]



    private void CancelDelete() => IsDeleteConfirmVisible = false;



    [RelayCommand]



    private async System.Threading.Tasks.Task ConfirmDeleteAsync()



    {

        if (SelectedServiceItem is null)



        {

            return;



        }



        _writeCts?.Cancel();



        _writeCts?.Dispose();



        _writeCts = new System.Threading.CancellationTokenSource();



        var ct = _writeCts.Token;



        IsDeleteConfirmVisible = false;



        IsLoading = true;



        ServiceItemDto toDelete = SelectedServiceItem;



        try



        {

            ServiceItemWriteResult result = await _service.DeleteAsync(toDelete, ct);



            if (result.IsSuccess)



            {

                ServiceItems.Remove(toDelete);



                TotalCount = System.Math.Max(0, TotalCount - 1);



                SelectedServiceItem = null;



                OnPropertyChanged(nameof(TotalPages));



                HasNextPage = CurrentPage < TotalPages;



                if (ServiceItems.Count == 0 && CurrentPage > 1)



                {

                    CurrentPage--;



                }



            }



            else



            {

                HandleError("X�a th?t b?i", result.ErrorMessage!, result.Advice);



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



    // --- Private Helpers -------------------------------------------------------



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

        FormDescription = string.Empty;



        FormUnitPrice = string.Empty;



        FormTypeIndex = 0;



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



    private bool ValidateForm()



    {

        if (string.IsNullOrWhiteSpace(FormDescription))



        { SetFormError("M� t? d?ch v? kh�ng được d? tr?ng."); return false; }



        if (FormDescription.Length > 255)



        { SetFormError("M� t? kh�ng được vượt qu� 255 k� t?."); return false; }



        if (!decimal.TryParse(FormUnitPrice, out decimal price) || price <= 0)



        { SetFormError("�on gi� kh�ng h?p l? (ph?i l� s? duong)."); return false; }



        if (FormTypeIndex == 0)



        { SetFormError("Vui l�ng ch?n lo?i d?ch v?."); return false; }



        return true;



    }



    private ServiceItemDto BuildPacketFromForm()



    {

        decimal.TryParse(FormUnitPrice, out decimal price);



        ServiceType type = FormTypeIndex <= 0 || FormTypeIndex > _serviceTypeFormValues.Length



            ? ServiceType.None



            : _serviceTypeFormValues[FormTypeIndex - 1];



        return new ServiceItemDto



        {

            ServiceItemId = IsEditing ? SelectedServiceItem?.ServiceItemId : null,



            Description = FormDescription,



            UnitPrice = price,



            Type = type,



            SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()



        };



    }



    private int IndexOfServiceItem(System.Object? id)



    {

        if (id is null)



        {

            return -1;



        }



        for (int i = 0; i < ServiceItems.Count; i++)



        {

            if (ServiceItems[i].ServiceItemId is System.Object itemId && itemId.Equals(id))



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



        PopupButtonText = isRetry ? "Th? l?i" : "OK";



        IsPopupVisible = true;



    }

}


