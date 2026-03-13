// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Protocol.Inventory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class SparePartsViewModel : ObservableObject, System.IDisposable
{
    private readonly SparePartService _service;
    private System.Threading.CancellationTokenSource? _cts;
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
    [ObservableProperty] public partial SparePartSortField SortBy { get; set; } = SparePartSortField.PartName;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = false;

    // ─── Filter ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 FilterSupplierId { get; set; } = 0;
    [ObservableProperty] public partial PartCategory? FilterCategory { get; set; } = null;
    [ObservableProperty] public partial System.Boolean? FilterDiscontinued { get; set; } = null;

    // FIX: Thêm PickerCategoryIndex cho filter bar (XAML bind PickerCategoryIndex)
    [ObservableProperty] public partial System.Int32 PickerCategoryIndex { get; set; } = 0;

    // Picker index cho filter Discontinued: 0=Tất cả, 1=Đang bán, 2=Ngừng bán
    [ObservableProperty] public partial System.Int32 PickerDiscontinuedIndex { get; set; } = 0;

    public System.Boolean HasActiveFilters
        => FilterSupplierId != 0 || FilterCategory.HasValue || FilterDiscontinued.HasValue;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Parts.Count == 0;

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
    [ObservableProperty] public partial SparePartDto? SelectedPart { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa phụ tùng" : "Thêm phụ tùng";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm phụ tùng";

    [ObservableProperty] public partial System.String FormPartName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPurchasePrice { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormSellingPrice { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormInventoryQuantity { get; set; } = "0";
    [ObservableProperty] public partial System.Int32 FormSupplierId { get; set; }
    [ObservableProperty] public partial PartCategory FormCategory { get; set; } = PartCategory.Other;
    [ObservableProperty] public partial System.Int32 FormPickerCategoryIndex { get; set; } = 0;

    // FIX: Thêm FormIsDiscontinued để Switch trong form edit hoạt động
    [ObservableProperty] public partial System.Boolean FormIsDiscontinued { get; set; } = false;

    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Discontinue Confirm ─────────────────────────────────────────────────

    // FIX: Đổi tên thành IsDiscontinueConfirmVisible (XAML trước đó dùng IsDeleteConfirmVisible - SAI)
    [ObservableProperty] public partial System.Boolean IsDiscontinueConfirmVisible { get; set; }

    // FIX: Đổi tên thành DiscontinueConfirmName (XAML trước đó dùng DeleteConfirmName - SAI)
    public System.String DiscontinueConfirmName => SelectedPart?.PartName ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<SparePartDto> Parts { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    // OPT: Bỏ _ = LoadAsync() khỏi constructor.
    // Load sẽ được trigger từ OnAppearing() trong code-behind để tránh
    // race condition và popup lỗi xuất hiện ngay khi page chưa render xong.
    public SparePartsViewModel(SparePartService service)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
        Parts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnSelectedPartChanged(SparePartDto? value)
        => OnPropertyChanged(nameof(DiscontinueConfirmName));

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

        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await System.Threading.Tasks.Task.Delay(SearchDebounceMs, token);
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (CurrentPage != 1) CurrentPage = 1;
                    else _ = LoadAsync();
                });
            }
            catch (System.OperationCanceledException) { }
        }, token);
    }

    partial void OnSortByChanged(SparePartSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();

    // FIX: Thêm hook cho PickerCategoryIndex để map sang FilterCategory
    // Thứ tự items trong XAML: Tất cả, Động cơ, Phanh, Hệ thống treo, Điện, Thân xe, Hộp số, Làm mát, Xả, Khác
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
            9 => PartCategory.Other,
            _ => null  // 0 = Tất cả
        };
    }

    partial void OnFilterCategoryChanged(PartCategory? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnFilterDiscontinuedChanged(System.Boolean? value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnFilterSupplierIdChanged(int value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnPickerDiscontinuedIndexChanged(int value)
    {
        FilterDiscontinued = value switch
        {
            1 => false,  // Đang bán
            2 => true,   // Ngừng bán
            _ => null    // Tất cả
        };
    }

    // Form category picker: Khác, Động cơ, Phanh, Hệ thống treo, Điện, Thân xe, Hộp số, Làm mát, Xả
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
            _ => PartCategory.Other  // 0 = Khác
        };
    }

    // ─── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        ClearError();
        IsLoading = true;

        try
        {
            SparePartListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterSupplierId: FilterSupplierId == 0 ? null : FilterSupplierId,
                filterCategory: FilterCategory,
                filterDiscontinued: FilterDiscontinued,
                ct: ct);

            Debug.WriteLine(
                $"[SparePartVM] Load ok={result.IsSuccess} count={result.Parts.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Parts.Clear();
                foreach (SparePartDto p in result.Parts)
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
        // OPT: Reset cả 2 picker index về 0 để UI đồng bộ với FilterCategory/FilterDiscontinued
        PickerCategoryIndex = 0;
        PickerDiscontinuedIndex = 0;
        FilterSupplierId = 0;
        // FilterCategory và FilterDiscontinued sẽ tự update qua OnPickerXxxChanged hooks
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
    private void OpenEditForm(SparePartDto part)
    {
        IsEditing = true;
        SelectedPart = part;

        FormPartName = part.PartName ?? System.String.Empty;
        FormPurchasePrice = part.PurchasePrice.ToString("0.##");
        FormSellingPrice = part.SellingPrice.ToString("0.##");
        FormInventoryQuantity = part.InventoryQuantity.ToString();
        FormSupplierId = part.SupplierId;

        // FIX: Gán FormIsDiscontinued từ entity đang edit
        FormIsDiscontinued = part.IsDiscontinued;

        // Form category picker: Khác=0, Engine=1, Brake=2, ...
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
            _ => 0  // Other
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
            SparePartDto data = BuildPacketFromForm();
            SparePartWriteResult result = IsEditing
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
                        System.Int32 idx = IndexOfPart(result.UpdatedEntity.SparePartId);
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

    // FIX: Đổi tên thành RequestDiscontinueCommand (XAML trước đó bind RequestDeleteCommand - SAI)
    [RelayCommand]
    private void RequestDiscontinue(SparePartDto part)
    {
        SelectedPart = part;
        IsDiscontinueConfirmVisible = true;
    }

    // FIX: Đổi tên thành CancelDiscontinueCommand (XAML trước đó bind CancelDeleteCommand - SAI)
    [RelayCommand]
    private void CancelDiscontinue() => IsDiscontinueConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDiscontinueAsync()
    {
        if (SelectedPart is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsDiscontinueConfirmVisible = false;
        IsLoading = true;

        SparePartDto toUpdate = SelectedPart;

        try
        {
            SparePartWriteResult result = await _service.DiscontinueAsync(toUpdate, ct);

            if (result.IsSuccess)
            {
                System.Int32 idx = IndexOfPart(toUpdate.SparePartId);
                if (idx >= 0)
                {
                    toUpdate.IsDiscontinued = true;
                    Parts[idx] = toUpdate;
                }

                SelectedPart = null;
            }
            else
            {
                HandleError("Ngừng bán thất bại", result.ErrorMessage!, result.Advice);
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

    // ─── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

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
        FormPartName = System.String.Empty;
        FormPurchasePrice = System.String.Empty;
        FormSellingPrice = System.String.Empty;
        FormInventoryQuantity = "0";
        FormSupplierId = 0;
        FormPickerCategoryIndex = 0;
        FormIsDiscontinued = false;
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
        if (System.String.IsNullOrWhiteSpace(FormPartName))
        { SetFormError("Tên phụ tùng không được để trống."); return false; }

        if (FormPartName.Length > 100)
        { SetFormError("Tên không được vượt quá 100 ký tự."); return false; }

        if (!System.Decimal.TryParse(FormPurchasePrice, out System.Decimal purchasePrice) || purchasePrice <= 0)
        { SetFormError("Giá nhập không hợp lệ (phải là số dương)."); return false; }

        if (!System.Decimal.TryParse(FormSellingPrice, out System.Decimal sellingPrice) || sellingPrice <= 0)
        { SetFormError("Giá bán không hợp lệ (phải là số dương)."); return false; }

        if (sellingPrice < purchasePrice)
        { SetFormError("Giá bán không được thấp hơn giá nhập."); return false; }

        if (!System.Int32.TryParse(FormInventoryQuantity, out System.Int32 qty) || qty < 0)
        { SetFormError("Số lượng tồn kho phải là số nguyên không âm."); return false; }

        if (FormSupplierId <= 0)
        { SetFormError("Vui lòng chọn nhà cung cấp."); return false; }

        return true;
    }

    private SparePartDto BuildPacketFromForm()
    {
        _ = System.Decimal.TryParse(FormPurchasePrice, out System.Decimal purchase);
        _ = System.Decimal.TryParse(FormSellingPrice, out System.Decimal selling);
        _ = System.Int32.TryParse(FormInventoryQuantity, out System.Int32 qty);

        return new SparePartDto
        {
            PartName = FormPartName,
            PurchasePrice = purchase,
            SellingPrice = selling,
            InventoryQuantity = qty,
            SupplierId = FormSupplierId,
            PartCategory = FormCategory,
            // FIX: Dùng FormIsDiscontinued thay vì đọc từ SelectedPart
            IsDiscontinued = FormIsDiscontinued,
            SparePartId = IsEditing ? SelectedPart?.SparePartId : null
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
            if (Parts[i].SparePartId is System.Object id && id.Equals(partId))
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
                // OPT: NONE / Timeout → chỉ hiện inline error, không popup
                // Tránh popup xuất hiện khi lần đầu vào tab mà server chưa sẵn sàng
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
