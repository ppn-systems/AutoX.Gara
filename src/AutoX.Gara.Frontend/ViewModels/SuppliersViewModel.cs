// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class SuppliersViewModel : ObservableObject, IDisposable
{
    private readonly ISupplierService _supplierService;

    // Separate CTS for load vs write to avoid cancelling each other
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _writeCts;
    private CancellationTokenSource? _searchCts;

    private Boolean _suppressAutoLoad = false;

    private const Int32 DefaultPageSize = 20;
    private const Int32 SearchDebounceMs = 400;

    // ─── Pagination ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial bool HasNextPage { get; set; }
    [ObservableProperty] public partial bool HasPreviousPage { get; set; }
    [ObservableProperty] public partial int TotalCount { get; set; }

    public Int32 TotalPages
        => TotalCount > 0
            ? (Int32)Math.Ceiling((Double)TotalCount / DefaultPageSize)
            : 0;

    // ─── Search / Sort ────────────────────────────────────────────────────────

    [ObservableProperty] public partial string SearchTerm { get; set; } = string.Empty;
    [ObservableProperty] public partial SupplierSortField SortBy { get; set; } = SupplierSortField.Name;
    [ObservableProperty] public partial bool SortDescending { get; set; } = false;

    // ─── Filter ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;
    [ObservableProperty] public partial PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;

    // SupplierStatus picker: 0=All, 1=Active, 2=Inactive, 3=Suspended, 4=Blacklisted
    [ObservableProperty] public partial int PickerFilterStatusIndex { get; set; } = 0;

    // PaymentTerms picker: 0=All, 1=Net7, 2=Net15, 3=Net30, 4=Net60, 5=Net90, 6=Immediate, 7=EndOfMonth
    [ObservableProperty] public partial int PickerPaymentTermsIndex { get; set; } = 0;

    public Boolean HasActiveFilters
        => FilterStatus != SupplierStatus.None || FilterPaymentTerms != PaymentTerms.None;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    public Boolean IsEmpty => !IsLoading && Suppliers.Count == 0;

    // ─── Popup ──────────────────────────────────────────────────────��────────

    [ObservableProperty] public partial bool IsPopupVisible { get; set; }
    [ObservableProperty] public partial bool IsPopupRetry { get; set; }
    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupButtonText { get; set; } = "OK";

    public Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form Add/Edit ────────────────────────────────────────────────────────

    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial bool IsEditing { get; set; }
    [ObservableProperty] public partial SupplierDto? SelectedSupplier { get; set; }

    public String FormTitle => IsEditing ? "Sửa nhà cung cấp" : "Thêm nhà cung cấp";
    public String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm nhà cung cấp";

    [ObservableProperty] public partial string FormName { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormEmail { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormAddress { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormTaxCode { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormBankAccount { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormPhoneNumbers { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormNotes { get; set; } = string.Empty;

    // FIX: Use non-nullable DateTime; null state tracked by HasContractStartDate
    [ObservableProperty] public partial DateTime FormContractStartDate { get; set; } = DateTime.Today;
    [ObservableProperty] public partial DateTime FormContractEndDate { get; set; } = DateTime.Today.AddYears(1);
    [ObservableProperty] public partial Boolean HasContractStartDate { get; set; }
    [ObservableProperty] public partial Boolean HasContractEndDate { get; set; }

    [ObservableProperty] public partial SupplierStatus FormStatus { get; set; } = SupplierStatus.Active;
    [ObservableProperty] public partial PaymentTerms FormPaymentTerms { get; set; } = PaymentTerms.None;

    // FIX: Form SupplierStatus picker: 0=Active, 1=Inactive, 2=Suspended, 3=Blacklisted
    [ObservableProperty] public partial int FormPickerStatusIndex { get; set; } = 0;

    // FIX: Form PaymentTerms picker: 0=None, 1=Net7, 2=Net15, 3=Net30, 4=Net60, 5=Net90, 6=Immediate, 7=EndOfMonth
    [ObservableProperty] public partial int FormPickerPaymentTermsIndex { get; set; } = 0;

    [ObservableProperty] public partial bool HasFormError { get; set; }
    [ObservableProperty] public partial string? FormErrorMessage { get; set; }

    // ─── Change Status Confirm ────────────────────────────────────────────────

    [ObservableProperty] public partial bool IsChangeStatusVisible { get; set; }
    [ObservableProperty] public partial SupplierStatus PendingStatus { get; set; }

    public String ChangeStatusConfirmName => SelectedSupplier?.Name ?? String.Empty;
    public String ChangeStatusConfirmMessage
        => $"Bạn có chắc muốn đổi trạng thái của \"{ChangeStatusConfirmName}\" thành {PendingStatus}?";

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<SupplierDto> Suppliers { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    public SuppliersViewModel(ISupplierService supplierService)
    {
        _supplierService = supplierService
            ?? throw new ArgumentNullException(nameof(supplierService));

        Suppliers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnSelectedSupplierChanged(SupplierDto? value)
    {
        OnPropertyChanged(nameof(ChangeStatusConfirmName));
        OnPropertyChanged(nameof(ChangeStatusConfirmMessage));
    }

    partial void OnPendingStatusChanged(SupplierStatus value)
        => OnPropertyChanged(nameof(ChangeStatusConfirmMessage));

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
        // Cancel previous debounce timer
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        // FIX: No need for Task.Run — Task.Delay already runs asynchronously
        _ = DebounceSearchAsync(token);
    }

    private async Task DebounceSearchAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(SearchDebounceMs, token).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
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
        catch (OperationCanceledException) { /* debounce cancelled — expected */ }
    }

    partial void OnSortByChanged(SupplierSortField value)
    {
        if (!_suppressAutoLoad) _ = LoadAsync();
    }

    partial void OnSortDescendingChanged(bool value)
    {
        if (!_suppressAutoLoad) _ = LoadAsync();
    }

    partial void OnFilterStatusChanged(SupplierStatus value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnFilterPaymentTermsChanged(PaymentTerms value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    // FIX: PaymentTerms filter picker — correct full mapping
    // ─── Picker index → enum (Filter bar) ────────────────────────────────────────

    /// <summary>
    /// Maps filter status picker index to <see cref="SupplierStatus"/>.
    /// Index: 0=All(None), 1=Active, 2=Inactive, 3=Potential,
    ///        4=Suspended, 5=UnderReview, 6=ContractSigned, 7=Blacklisted
    /// </summary>
    partial void OnPickerFilterStatusIndexChanged(int value)
    {
        FilterStatus = value switch
        {
            1 => SupplierStatus.Active,
            2 => SupplierStatus.Inactive,
            3 => SupplierStatus.Potential,
            4 => SupplierStatus.Suspended,
            5 => SupplierStatus.UnderReview,
            6 => SupplierStatus.ContractSigned,
            7 => SupplierStatus.Blacklisted,
            _ => SupplierStatus.None    // 0 = Tất cả
        };
    }

    /// <summary>
    /// Maps filter payment terms picker index to <see cref="PaymentTerms"/>.
    /// Index: 0=All(None), 1=DueOnReceipt, 2=Net7, 3=Net15, 4=Net30, 5=Custom
    /// </summary>
    partial void OnPickerPaymentTermsIndexChanged(int value)
    {
        FilterPaymentTerms = value switch
        {
            1 => PaymentTerms.DueOnReceipt,
            2 => PaymentTerms.Net7,
            3 => PaymentTerms.Net15,
            4 => PaymentTerms.Net30,
            5 => PaymentTerms.Custom,
            _ => PaymentTerms.None      // 0 = Tất cả
        };
    }

    // ─── Picker index → enum (Create/Edit form) ──────────────────────────────────

    /// <summary>
    /// Maps form status picker index to <see cref="SupplierStatus"/>.
    /// Index: 0=None, 1=Active, 2=Inactive, 3=Potential,
    ///        4=Suspended, 5=UnderReview, 6=ContractSigned, 7=Blacklisted
    /// </summary>
    partial void OnFormPickerStatusIndexChanged(int value)
    {
        FormStatus = value switch
        {
            1 => SupplierStatus.Active,
            2 => SupplierStatus.Inactive,
            3 => SupplierStatus.Potential,
            4 => SupplierStatus.Suspended,
            5 => SupplierStatus.UnderReview,
            6 => SupplierStatus.ContractSigned,
            7 => SupplierStatus.Blacklisted,
            _ => SupplierStatus.None    // 0 = Không xác định
        };
    }

    /// <summary>
    /// Maps form payment terms picker index to <see cref="PaymentTerms"/>.
    /// Index: 0=None, 1=DueOnReceipt, 2=Net7, 3=Net15, 4=Net30, 5=Custom
    /// </summary>
    partial void OnFormPickerPaymentTermsIndexChanged(int value)
    {
        FormPaymentTerms = value switch
        {
            1 => PaymentTerms.DueOnReceipt,
            2 => PaymentTerms.Net7,
            3 => PaymentTerms.Net15,
            4 => PaymentTerms.Net30,
            5 => PaymentTerms.Custom,
            _ => PaymentTerms.None      // 0 = Không xác định
        };
    }

    // ─── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadAsync()
    {
        // Cancel any in-flight load request before starting a new one
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        ClearError();
        IsLoading = true;

        try
        {
            SupplierListResult result = await _supplierService.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterStatus: FilterStatus,
                filterPaymentTerms: FilterPaymentTerms,
                ct: ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            Debug.WriteLine(
                $"[VM] Load ok={result.IsSuccess} count={result.Suppliers.Count} total={result.TotalCount}");

            if (result.IsSuccess)
            {
                Suppliers.Clear();
                foreach (SupplierDto s in result.Suppliers)
                {
                    Suppliers.Add(s);
                }

                TotalCount = result.TotalCount >= 0 ? result.TotalCount : TotalCount;
                HasNextPage = result.TotalCount >= 0
                    ? CurrentPage < TotalPages
                    : result.HasMore;
            }
            else
            {
                HandleWriteError("Tải danh sách thất bại", result.ErrorMessage!, result.Advice);
            }
        }
        catch (OperationCanceledException)
        {
            // Load was cancelled by a newer request — silently ignore
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SortByColumn(String? fieldName)
    {
        if (!Enum.TryParse<SupplierSortField>(fieldName, out SupplierSortField field))
        {
            return;
        }

        _suppressAutoLoad = true;
        try
        {
            Boolean isSameColumn = SortBy == field;
            SortBy = field;
            SortDescending = isSameColumn && !SortDescending;
        }
        finally
        {
            _suppressAutoLoad = false;
        }

        _ = LoadAsync();
    }

    [RelayCommand]
    private void ClearSearch() => SearchTerm = String.Empty;

    [RelayCommand]
    private void ClearFilters()
    {
        PickerFilterStatusIndex = 0;
        PickerPaymentTermsIndex = 0;
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedSupplier = null;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(SupplierDto supplier)
    {
        IsEditing = true;
        SelectedSupplier = supplier;

        FormName = supplier.Name ?? String.Empty;
        FormEmail = supplier.Email ?? String.Empty;
        FormAddress = supplier.Address ?? String.Empty;
        FormTaxCode = supplier.TaxCode ?? String.Empty;
        FormBankAccount = supplier.BankAccount ?? String.Empty;
        FormPhoneNumbers = supplier.PhoneNumbers ?? String.Empty;
        FormNotes = supplier.Notes ?? String.Empty;

        // FIX: Handle nullable DateTime for DatePicker
        HasContractStartDate = supplier.ContractStartDate.HasValue;
        FormContractStartDate = supplier.ContractStartDate ?? DateTime.Today;

        HasContractEndDate = supplier.ContractEndDate.HasValue;
        FormContractEndDate = supplier.ContractEndDate ?? DateTime.Today.AddYears(1);

        // FIX: Correct index mapping — 0=Active, 1=Inactive, 2=Suspended, 3=Blacklisted
        FormPickerStatusIndex = (supplier.Status ?? SupplierStatus.Active) switch
        {
            SupplierStatus.Active => 1,
            SupplierStatus.Inactive => 2,
            SupplierStatus.Potential => 3,
            SupplierStatus.Suspended => 4,
            SupplierStatus.UnderReview => 5,
            SupplierStatus.ContractSigned => 6,
            SupplierStatus.Blacklisted => 7,
            _ => 0
        };

        // FIX: Full PaymentTerms mapping
        FormPickerPaymentTermsIndex = (supplier.PaymentTerms ?? PaymentTerms.None) switch
        {
            PaymentTerms.None => 0,
            PaymentTerms.DueOnReceipt => 1,
            PaymentTerms.Net7 => 2,
            PaymentTerms.Net15 => 3,
            PaymentTerms.Net30 => 4,
            PaymentTerms.Custom => 5,
            _ => 0  // None / unknown
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
    private async Task SaveFormAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new CancellationTokenSource();
        var ct = _writeCts.Token;

        IsLoading = true;

        try
        {
            SupplierDto data = BuildPacketFromForm();
            SupplierWriteResult result = IsEditing
                ? await _supplierService.UpdateAsync(data, ct)
                : await _supplierService.CreateAsync(data, ct);

            if (result.IsSuccess)
            {
                IsFormVisible = false;
                ClearForm();

                if (result.UpdatedEntity is not null)
                {
                    if (IsEditing)
                    {
                        Int32 idx = IndexOfSupplier(result.UpdatedEntity.SupplierId);
                        if (idx >= 0)
                        {
                            Suppliers[idx] = result.UpdatedEntity;
                        }
                        else
                        {
                            await LoadAsync();
                        }
                    }
                    else
                    {
                        Suppliers.Insert(0, result.UpdatedEntity);
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
    private void RequestChangeStatus(SupplierDto supplier)
    {
        SelectedSupplier = supplier;
        // Suggest next logical status: Active → Suspended, anything else → Active
        PendingStatus = (supplier.Status ?? SupplierStatus.Active) == SupplierStatus.Active
            ? SupplierStatus.Suspended
            : SupplierStatus.Active;
        IsChangeStatusVisible = true;
    }

    [RelayCommand]
    private void CancelChangeStatus() => IsChangeStatusVisible = false;

    [RelayCommand]
    private async Task ConfirmChangeStatusAsync()
    {
        if (SelectedSupplier?.SupplierId is null)
        {
            return;
        }

        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new CancellationTokenSource();
        var ct = _writeCts.Token;

        IsChangeStatusVisible = false;
        IsLoading = true;

        Int32 id = SelectedSupplier.SupplierId.Value;
        SupplierStatus newStatus = PendingStatus;

        try
        {
            SupplierWriteResult result = await _supplierService.ChangeStatusAsync(id, newStatus, ct);

            if (result.IsSuccess)
            {
                Int32 idx = IndexOfSupplier(id);
                if (idx >= 0)
                {
                    SupplierDto old = Suppliers[idx];

                    // Manually clone with updated Status since SupplierDto is a class, not a record.
                    // This creates a new reference so CollectionView detects the item change.
                    SupplierDto updated = new()
                    {
                        SupplierId = old.SupplierId,
                        Name = old.Name,
                        Email = old.Email,
                        Address = old.Address,
                        TaxCode = old.TaxCode,
                        BankAccount = old.BankAccount,
                        PhoneNumbers = old.PhoneNumbers,
                        Notes = old.Notes,
                        Status = newStatus,          // ← only this changes
                        PaymentTerms = old.PaymentTerms,
                        ContractStartDate = old.ContractStartDate,
                        ContractEndDate = old.ContractEndDate,
                        OpCode = old.OpCode,
                        SequenceId = old.SequenceId
                    };

                    Suppliers[idx] = updated;
                }
            }
            else
            {
                HandleWriteError("Đổi trạng thái thất bại", result.ErrorMessage!, result.Advice);
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
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _writeCts?.Cancel();
        _writeCts?.Dispose();
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
        FormName = FormEmail = FormAddress = FormTaxCode =
            FormBankAccount = FormPhoneNumbers = FormNotes = String.Empty;
        FormContractStartDate = DateTime.Today;
        FormContractEndDate = DateTime.Today.AddYears(1);
        HasContractStartDate = false;
        HasContractEndDate = false;
        FormPickerStatusIndex = 0;
        FormPickerPaymentTermsIndex = 0;
        ClearFormError();
    }

    private void ClearFormError()
    {
        HasFormError = false;
        FormErrorMessage = null;
    }

    private void SetFormError(String message)
    {
        FormErrorMessage = message;
        HasFormError = true;
    }

    private Boolean ValidateForm()
    {
        if (String.IsNullOrWhiteSpace(FormName))
        { SetFormError("Tên nhà cung cấp không được để trống."); return false; }

        if (FormName.Length > 100)
        { SetFormError("Tên không được vượt quá 100 ký tự."); return false; }

        if (!AccountValidation.IsValidEmail(FormEmail))
        { SetFormError("Email không hợp lệ."); return false; }

        if (String.IsNullOrWhiteSpace(FormTaxCode))
        { SetFormError("Mã số thuế không được để trống."); return false; }

        if (!String.IsNullOrWhiteSpace(FormPhoneNumbers))
        {
            foreach (String phone in FormPhoneNumbers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!AccountValidation.IsValidVietnamPhoneNumber(phone))
                {
                    SetFormError($"Số điện thoại \"{phone}\" không hợp lệ (VD: 0901234567).");
                    return false;
                }
            }
        }

        if (HasContractEndDate && HasContractStartDate
            && FormContractEndDate <= FormContractStartDate)
        {
            SetFormError("Ngày kết thúc hợp đồng phải sau ngày bắt đầu.");
            return false;
        }

        if (FormNotes.Length > 500)
        { SetFormError("Ghi chú không được vượt quá 500 ký tự."); return false; }

        return true;
    }

    private SupplierDto BuildPacketFromForm() => new()
    {
        SupplierId = IsEditing ? SelectedSupplier?.SupplierId : null,
        Name = FormName,
        Email = FormEmail,
        Address = FormAddress,
        TaxCode = FormTaxCode,
        BankAccount = FormBankAccount,
        PhoneNumbers = FormPhoneNumbers,
        Notes = FormNotes,
        Status = FormStatus,
        PaymentTerms = FormPaymentTerms,
        ContractStartDate = HasContractStartDate ? FormContractStartDate : null,
        ContractEndDate = HasContractEndDate ? FormContractEndDate : null
    };

    private Int32 IndexOfSupplier(Int32? supplierId)
    {
        if (supplierId is null)
        {
            return -1;
        }

        for (Int32 i = 0; i < Suppliers.Count; i++)
        {
            if (Suppliers[i].SupplierId == supplierId)
            {
                return i;
            }
        }

        return -1;
    }

    private void HandleWriteError(String title, String message, ProtocolAdvice advice)
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

    private void ShowPopup(String title, String message, Boolean isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? "Thử lại" : "OK";
        IsPopupVisible = true;
    }
}