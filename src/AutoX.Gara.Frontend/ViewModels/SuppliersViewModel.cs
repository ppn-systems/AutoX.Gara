// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Results.Suppliers;
using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;
using System.Linq;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel for supplier management.
/// </summary>
public sealed partial class SuppliersViewModel : ObservableObject, System.IDisposable
{
    private readonly SupplierService _service;
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
    [ObservableProperty] public partial SupplierSortField SortBy { get; set; } = SupplierSortField.Name;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = false;

    // ─── Filter ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;
    [ObservableProperty] public partial PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;

    [ObservableProperty] public partial System.Int32 PickerStatusIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerPaymentTermsIndex { get; set; } = 0;

    private static readonly SupplierStatus[] StatusValues =
    [
        SupplierStatus.None,
        SupplierStatus.Active,
        SupplierStatus.Inactive,
        SupplierStatus.Potential,
        SupplierStatus.Suspended,
        SupplierStatus.UnderReview,
        SupplierStatus.ContractSigned,
        SupplierStatus.Blacklisted
    ];

    private static readonly PaymentTerms[] PaymentTermsValues =
    [
        PaymentTerms.None,
        PaymentTerms.DueOnReceipt,
        PaymentTerms.Net7,
        PaymentTerms.Net15,
        PaymentTerms.Net30,
        PaymentTerms.Custom
    ];

    private readonly SupplierStatus?[] _statusFilterValues =
    [
        null,
        SupplierStatus.Active,
        SupplierStatus.Inactive,
        SupplierStatus.Potential,
        SupplierStatus.Suspended,
        SupplierStatus.UnderReview,
        SupplierStatus.ContractSigned,
        SupplierStatus.Blacklisted
    ];

    private readonly PaymentTerms?[] _paymentTermsFilterValues =
    [
        null,
        PaymentTerms.DueOnReceipt,
        PaymentTerms.Net7,
        PaymentTerms.Net15,
        PaymentTerms.Net30,
        PaymentTerms.Custom
    ];

    public string[] FilterStatusOptions { get; } =
        new[] { "Tất cả trạng thái" }.Concat(StatusValues.Where(v => v != SupplierStatus.None).Select(EnumText.Get)).ToArray();
    public string[] FilterPaymentTermsOptions { get; } =
        new[] { "Tất cả thanh toán" }.Concat(PaymentTermsValues.Where(v => v != PaymentTerms.None).Select(EnumText.Get)).ToArray();

    public System.Boolean HasActiveFilters
        => FilterStatus != SupplierStatus.None || FilterPaymentTerms != PaymentTerms.None;

    public System.String SelectedFilterStatusText =>
        FilterStatusOptions[System.Math.Clamp(PickerStatusIndex, 0, FilterStatusOptions.Length - 1)];
    public System.String SelectedFilterPaymentTermsText =>
        FilterPaymentTermsOptions[System.Math.Clamp(PickerPaymentTermsIndex, 0, FilterPaymentTermsOptions.Length - 1)];

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Suppliers.Count == 0;

    // ─── Popup ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form ──────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }
    [ObservableProperty] public partial SupplierDto? SelectedSupplier { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa nhà cung cấp" : "Thêm nhà cung cấp";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm nhà cung cấp";

    [ObservableProperty] public partial System.String FormName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormEmail { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormAddress { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormTaxCode { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormBankAccount { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPhoneNumbers { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormNotes { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.Int32 FormStatusIndex { get; set; } = 1;
    [ObservableProperty] public partial System.Int32 FormPaymentTermsIndex { get; set; } = 0;
    [ObservableProperty] public partial System.DateTime FormContractStartDate { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.DateTime? FormContractEndDate { get; set; } = null;
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    public string[] FormStatusOptions { get; } =
        StatusValues.Select(EnumText.Get).ToArray();
    public string[] FormPaymentTermsOptions { get; } =
        PaymentTermsValues.Select(EnumText.Get).ToArray();

    public System.String SelectedFormStatusText =>
        FormStatusOptions[System.Math.Clamp(FormStatusIndex, 0, FormStatusOptions.Length - 1)];
    public System.String SelectedFormPaymentTermsText =>
        FormPaymentTermsOptions[System.Math.Clamp(FormPaymentTermsIndex, 0, FormPaymentTermsOptions.Length - 1)];

    // ─── Status Confirm ───────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsStatusConfirmVisible { get; set; }
    [ObservableProperty] public partial System.Int32 NewStatusIndex { get; set; } = 1;
    public System.String StatusConfirmName => SelectedSupplier?.Name ?? System.String.Empty;

    public System.String SelectedNewStatusText =>
        FormStatusOptions[System.Math.Clamp(NewStatusIndex, 0, FormStatusOptions.Length - 1)];

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<SupplierDto> Suppliers { get; } = [];

    // ─── Constructor ───────────────────────────────────────────────────────────

    public SuppliersViewModel(SupplierService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Suppliers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));

        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(SelectedFilterStatusText));
            OnPropertyChanged(nameof(SelectedFilterPaymentTermsText));
            OnPropertyChanged(nameof(SelectedFormStatusText));
            OnPropertyChanged(nameof(SelectedFormPaymentTermsText));
            OnPropertyChanged(nameof(SelectedNewStatusText));
        });
    }

    // ─── Property Hooks ───────────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedSupplierChanged(SupplierDto? value) => OnPropertyChanged(nameof(StatusConfirmName));
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
    partial void OnSortByChanged(SupplierSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();
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

    partial void OnPickerStatusIndexChanged(int value)
    {
        if (value < 0 || value >= _statusFilterValues.Length)
        {
            FilterStatus = SupplierStatus.None;
            return;
        }

        FilterStatus = _statusFilterValues[value] ?? SupplierStatus.None;
        OnPropertyChanged(nameof(SelectedFilterStatusText));
    }
    partial void OnPickerPaymentTermsIndexChanged(int value)
    {
        if (value < 0 || value >= _paymentTermsFilterValues.Length)
        {
            FilterPaymentTerms = PaymentTerms.None;
            return;
        }

        FilterPaymentTerms = _paymentTermsFilterValues[value] ?? PaymentTerms.None;
        OnPropertyChanged(nameof(SelectedFilterPaymentTermsText));
    }

    partial void OnFormStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedFormStatusText));
    partial void OnFormPaymentTermsIndexChanged(int value) => OnPropertyChanged(nameof(SelectedFormPaymentTermsText));
    partial void OnNewStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedNewStatusText));

    // ─── Commands ──────────────────────────────────────────────────────────────

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
            SupplierListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterStatus: FilterStatus,
                filterPaymentTerms: FilterPaymentTerms,
                ct: ct);

            Debug.WriteLine($"[SuppliersVM] Load ok={result.IsSuccess} count={result.Suppliers.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

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
        PickerStatusIndex = 0;
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

        FormName = supplier.Name ?? System.String.Empty;
        FormEmail = supplier.Email ?? System.String.Empty;
        FormAddress = supplier.Address ?? System.String.Empty;
        FormTaxCode = supplier.TaxCode ?? System.String.Empty;
        FormBankAccount = supplier.BankAccount ?? System.String.Empty;
        FormPhoneNumbers = supplier.PhoneNumbers ?? System.String.Empty;
        FormNotes = supplier.Notes ?? System.String.Empty;
        FormStatusIndex = System.Array.IndexOf(StatusValues, supplier.Status ?? SupplierStatus.None);
        if (FormStatusIndex < 0) FormStatusIndex = 0;

        FormPaymentTermsIndex = System.Array.IndexOf(PaymentTermsValues, supplier.PaymentTerms ?? PaymentTerms.None);
        if (FormPaymentTermsIndex < 0) FormPaymentTermsIndex = 0;
        FormContractStartDate = supplier.ContractStartDate ?? System.DateTime.Today;
        FormContractEndDate = supplier.ContractEndDate;

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
            SupplierDto data = BuildPacketFromForm();
            SupplierWriteResult result = IsEditing
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
                        System.Int32 idx = IndexOfSupplier(result.UpdatedEntity.SupplierId);
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
        NewStatusIndex = System.Array.IndexOf(StatusValues, supplier.Status ?? SupplierStatus.None);
        if (NewStatusIndex < 0) NewStatusIndex = 0;
        IsStatusConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelChangeStatus() => IsStatusConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmChangeStatusAsync()
    {
        if (SelectedSupplier is null)
        {
            return;
        }

        _writeCts?.Cancel();
        _writeCts?.Dispose();
        _writeCts = new System.Threading.CancellationTokenSource();
        var ct = _writeCts.Token;

        IsStatusConfirmVisible = false;
        IsLoading = true;

        try
        {
            SupplierDto data = new()
            {
                SupplierId = SelectedSupplier.SupplierId,
                Status = NewStatusIndex >= 0 && NewStatusIndex < StatusValues.Length
                    ? StatusValues[NewStatusIndex]
                    : SupplierStatus.None,
                SequenceId = Nalix.Framework.Random.Csprng.NextUInt32()
            };

            SupplierWriteResult result = await _service.ChangeStatusAsync(data, ct);

            if (result.IsSuccess)
            {
                System.Int32 idx = IndexOfSupplier(SelectedSupplier.SupplierId);
                if (idx >= 0)
                {
                    SupplierDto updated = Suppliers[idx];
                    updated.Status = NewStatusIndex >= 0 && NewStatusIndex < StatusValues.Length
                        ? StatusValues[NewStatusIndex]
                        : SupplierStatus.None;
                    Suppliers[idx] = updated;
                }
                SelectedSupplier = null;
            }
            else
            {
                HandleError("Đổi trạng thái thất bại", result.ErrorMessage!, result.Advice);
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

    // ─── Private Helpers ───────────────────────────────────────────────────────

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
        FormName = System.String.Empty;
        FormEmail = System.String.Empty;
        FormAddress = System.String.Empty;
        FormTaxCode = System.String.Empty;
        FormBankAccount = System.String.Empty;
        FormPhoneNumbers = System.String.Empty;
        FormNotes = System.String.Empty;
        FormStatusIndex = 1;
        FormPaymentTermsIndex = 0;
        FormContractStartDate = System.DateTime.Today;
        FormContractEndDate = null;
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
        if (System.String.IsNullOrWhiteSpace(FormName))
        { SetFormError("Tên nhà cung cấp không được để trống."); return false; }

        if (FormName.Length > 150)
        { SetFormError("Tên không được vượt quá 150 ký tự."); return false; }

        if (!IsValidEmail(FormEmail))
        { SetFormError("Email không hợp lệ."); return false; }

        if (System.String.IsNullOrWhiteSpace(FormTaxCode))
        { SetFormError("Mã số thuế không được để trống."); return false; }

        if (FormPhoneNumbers.Length > 0)
        {
            foreach (var phone in FormPhoneNumbers.Split(','))
            {
                if (!IsValidPhoneNumber(phone.Trim()))
                { SetFormError("Số điện thoại không hợp lệ."); return false; }
            }
        }

        if (FormContractEndDate.HasValue && FormContractEndDate <= FormContractStartDate)
        { SetFormError("Ngày kết thúc phải sau ngày bắt đầu."); return false; }

        return true;
    }

    private SupplierDto BuildPacketFromForm()
    {
        return new SupplierDto
        {
            SupplierId = IsEditing ? SelectedSupplier?.SupplierId : null,
            Name = FormName,
            Email = FormEmail,
            Address = FormAddress,
            TaxCode = FormTaxCode,
            BankAccount = FormBankAccount,
            PhoneNumbers = FormPhoneNumbers,
            Notes = FormNotes,
            Status = FormStatusIndex >= 0 && FormStatusIndex < StatusValues.Length
                ? StatusValues[FormStatusIndex]
                : SupplierStatus.None,
            PaymentTerms = FormPaymentTermsIndex >= 0 && FormPaymentTermsIndex < PaymentTermsValues.Length
                ? PaymentTermsValues[FormPaymentTermsIndex]
                : PaymentTerms.None,
            ContractStartDate = FormContractStartDate,
            ContractEndDate = FormContractEndDate,
            SequenceId = Nalix.Framework.Random.Csprng.NextUInt32()
        };
    }

    private System.Int32 IndexOfSupplier(System.Object? id)
    {
        if (id is null)
        {
            return -1;
        }

        for (System.Int32 i = 0; i < Suppliers.Count; i++)
        {
            if (Suppliers[i].SupplierId is System.Object supplierId && supplierId.Equals(id))
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

    private static System.Boolean IsValidEmail(System.String email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static System.Boolean IsValidPhoneNumber(System.String phone) => System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+?\d{1,3}[-.\s]?)?\d{10,15}$");
}
