// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets.Customers;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel for the Customers management screen.
/// Responsibilities: UI state, search/sort/filter/pagination, validate → service → optimistic update.
/// Does NOT contain: network code, navigation code, validation regex.
/// </summary>
public sealed partial class CustomersViewModel : ObservableObject, System.IDisposable
{
    private readonly ICustomerService _customerService;
    private System.Threading.CancellationTokenSource? _cts;
    private System.Threading.Timer? _searchDebounceTimer;

    private const System.Int32 DefaultPageSize = 20;
    private const System.Int32 SearchDebounceMs = 400;

    // ─── Pagination ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }
    [ObservableProperty] public partial System.Int32 TotalCount { get; set; }

    public System.Int32 TotalPages =>
        TotalCount > 0 ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize) : 0;

    // ─── Search / Sort ────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.String SearchTerm { get; set; } = System.String.Empty;
    [ObservableProperty] public partial CustomerSortField SortBy { get; set; } = CustomerSortField.CreatedAt;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = true;

    // ─── Filter ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Lọc theo loại khách hàng. <c>CustomerType.None</c> = hiển thị tất cả.
    /// </summary>
    [ObservableProperty] public partial CustomerType FilterType { get; set; } = CustomerType.None;

    /// <summary>
    /// Lọc theo hạng thành viên. <c>MembershipLevel.None</c> = hiển thị tất cả.
    /// </summary>
    [ObservableProperty] public partial MembershipLevel FilterMembership { get; set; } = MembershipLevel.None;

    /// <summary>True khi có ít nhất một filter đang được áp dụng.</summary>
    public System.Boolean HasActiveFilters =>
        FilterType != CustomerType.None || FilterMembership != MembershipLevel.None;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    /// <summary>True khi không loading và danh sách rỗng — dùng để hiện empty state.</summary>
    public System.Boolean IsEmpty => !IsLoading && Customers.Count == 0;

    // ─── Popup ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form (Create / Edit) ─────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }
    [ObservableProperty] public partial CustomerDataPacket? SelectedCustomer { get; set; }

    // ─── Form Fields ──────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.String FormName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormEmail { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPhone { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormAddress { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormTaxCode { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormNotes { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.DateTime? FormDateOfBirth { get; set; }
    [ObservableProperty] public partial CustomerType FormType { get; set; }
    [ObservableProperty] public partial MembershipLevel FormMembership { get; set; }
    [ObservableProperty] public partial Gender FormGender { get; set; } = Gender.None;
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Delete Confirmation ──────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }

    // ─── Customer List ────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<CustomerDataPacket> Customers { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;

        // Notify IsEmpty mỗi khi collection thay đổi
        Customers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));

        _ = LoadAsync();
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));

    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnCurrentPageChanged(int value)
    {
        HasPreviousPage = value > 1;
        _ = LoadAsync();
    }

    /// <summary>Debounce search: chờ 400ms sau lần gõ cuối mới gửi request.</summary>
    partial void OnSearchTermChanged(string value)
    {
        _searchDebounceTimer?.Dispose();
        _searchDebounceTimer = new System.Threading.Timer(_ =>
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                if (CurrentPage != 1)
                    CurrentPage = 1;
                else
                    _ = LoadAsync();
            });
        }, null, SearchDebounceMs, System.Threading.Timeout.Infinite);
    }

    partial void OnSortByChanged(CustomerSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();

    // Reset về trang 1 khi filter thay đổi
    partial void OnFilterTypeChanged(CustomerType value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }

    partial void OnFilterMembershipChanged(MembershipLevel value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
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
            CustomerListResult result = await _customerService.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterType: FilterType,
                filterMembership: FilterMembership,
                ct: ct);

            Debug.WriteLine($"[VM] Load: IsSuccess={result.IsSuccess}, Count={result.Customers.Count}, Total={result.TotalCount}");

            if (result.IsSuccess)
            {
                Customers.Clear();
                foreach (CustomerDataPacket c in result.Customers)
                {
                    Customers.Add(c);
                }

                if (result.TotalCount >= 0)
                {
                    TotalCount = result.TotalCount;
                    HasNextPage = CurrentPage < TotalPages;
                }
                else
                {
                    HasNextPage = result.HasMore;
                }
            }
            else
            {
                HandleWriteError("Tải danh sách thất bại", result.ErrorMessage!, result.Advice);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Sort theo cột — toggle direction nếu đang chọn cùng cột.</summary>
    [RelayCommand]
    private void SortByColumn(System.String? fieldName)
    {
        if (!System.Enum.TryParse<CustomerSortField>(fieldName, out CustomerSortField field))
        {
            return;
        }

        if (SortBy == field)
        {
            SortDescending = !SortDescending;
        }
        else
        {
            SortBy = field;
            SortDescending = false;
        }
    }

    [RelayCommand]
    private void ClearSearch() => SearchTerm = System.String.Empty;

    /// <summary>Xóa tất cả filter (Type + Membership) và reload.</summary>
    [RelayCommand]
    private void ClearFilters()
    {
        FilterType = CustomerType.None;
        FilterMembership = MembershipLevel.None;
        // OnFilterTypeChanged / OnFilterMembershipChanged sẽ tự trigger load
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedCustomer = null;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(CustomerDataPacket customer)
    {
        IsEditing = true;
        SelectedCustomer = customer;
        FormName = customer.Name ?? System.String.Empty;
        FormEmail = customer.Email ?? System.String.Empty;
        FormPhone = customer.PhoneNumber ?? System.String.Empty;
        FormAddress = customer.Address ?? System.String.Empty;
        FormTaxCode = customer.TaxCode ?? System.String.Empty;
        FormNotes = customer.Notes ?? System.String.Empty;
        FormDateOfBirth = customer.DateOfBirth == default ? null : customer.DateOfBirth;
        FormType = customer.Type ?? (System.Int32)CustomerType.None;
        FormMembership = customer.Membership ?? (System.Int32)MembershipLevel.None;
        FormGender = customer.Gender ?? Gender.None;
        ClearFormError();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormVisible = false;
        ClearForm();
    }

    /// <summary>
    /// Save form và thực hiện optimistic UI update từ entity được server echo lại.
    /// Không reload toàn bộ list — cập nhật trực tiếp phần tử trong ObservableCollection.
    /// </summary>
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
            CustomerDataPacket data = BuildPacketFromForm();
            CustomerWriteResult result = IsEditing
                ? await _customerService.UpdateAsync(data, ct)
                : await _customerService.CreateAsync(data, ct);

            if (result.IsSuccess)
            {
                IsFormVisible = false;
                ClearForm();

                if (result.UpdatedEntity is not null)
                {
                    if (IsEditing)
                    {
                        System.Int32 idx = IndexOfCustomer(result.UpdatedEntity.CustomerId);
                        if (idx >= 0)
                        {
                            Customers[idx] = result.UpdatedEntity;
                        }
                        else
                        {
                            await LoadAsync();
                        }
                    }
                    else
                    {
                        Customers.Insert(0, result.UpdatedEntity);
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
    private void RequestDelete(CustomerDataPacket customer)
    {
        SelectedCustomer = customer;
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelDelete() => IsDeleteConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsDeleteConfirmVisible = false;
        IsLoading = true;

        CustomerDataPacket toDelete = SelectedCustomer;

        try
        {
            CustomerWriteResult result = await _customerService.DeleteAsync(toDelete, ct);

            if (result.IsSuccess)
            {
                Customers.Remove(toDelete);
                TotalCount = System.Math.Max(0, TotalCount - 1);
                OnPropertyChanged(nameof(TotalPages));
                HasNextPage = CurrentPage < TotalPages;
                SelectedCustomer = null;

                if (Customers.Count == 0 && CurrentPage > 1)
                {
                    CurrentPage--;
                }
            }
            else
            {
                HandleWriteError("Xóa thất bại", result.ErrorMessage!, result.Advice);
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
        _searchDebounceTimer?.Dispose();
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>Reset về trang 1 nếu đang ở trang > 1, ngược lại reload trực tiếp.</summary>
    private void ResetPageAndLoad()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1; // OnCurrentPageChanged sẽ tự trigger LoadAsync
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
        FormName = FormEmail = FormPhone = FormAddress = FormTaxCode = FormNotes = System.String.Empty;
        FormDateOfBirth = null;
        FormType = default;
        FormMembership = default;
        FormGender = Gender.None;
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

    /// <summary>
    /// Client-side validation đầy đủ:
    /// tên, email, SĐT, ngày sinh, taxcode theo loại khách hàng.
    /// </summary>
    private System.Boolean ValidateForm()
    {
        if (System.String.IsNullOrWhiteSpace(FormName))
        {
            SetFormError("Tên khách hàng không được để trống.");
            return false;
        }

        if (FormName.Length > 100)
        {
            SetFormError("Tên không được vượt quá 100 ký tự.");
            return false;
        }

        if (!AccountValidation.IsValidEmail(FormEmail))
        {
            SetFormError("Email không hợp lệ.");
            return false;
        }

        if (!AccountValidation.IsValidVietnamPhoneNumber(FormPhone))
        {
            SetFormError("Số điện thoại không hợp lệ (VD: 0901234567).");
            return false;
        }

        if (FormDateOfBirth.HasValue)
        {
            if (FormDateOfBirth.Value > System.DateTime.Today)
            {
                SetFormError("Ngày sinh không được là ngày trong tương lai.");
                return false;
            }

            if (FormDateOfBirth.Value < System.DateTime.Today.AddYears(-120))
            {
                SetFormError("Ngày sinh không hợp lệ.");
                return false;
            }
        }

        if (FormType == CustomerType.Business && System.String.IsNullOrWhiteSpace(FormTaxCode))
        {
            SetFormError("Mã số thuế bắt buộc đối với khách hàng doanh nghiệp.");
            return false;
        }

        if (FormNotes.Length > 500)
        {
            SetFormError("Ghi chú không được vượt quá 500 ký tự.");
            return false;
        }

        return true;
    }

    private CustomerDataPacket BuildPacketFromForm()
    {
        CustomerDataPacket data = new()
        {
            Name = FormName,
            Email = FormEmail,
            PhoneNumber = FormPhone,
            Address = FormAddress,
            TaxCode = FormTaxCode,
            Notes = FormNotes,
            Type = FormType,
            Membership = FormMembership,
            Gender = FormGender,
            DateOfBirth = FormDateOfBirth ?? default,
            UpdatedAt = System.DateTime.UtcNow
        };

        if (IsEditing && SelectedCustomer is not null)
        {
            data.CustomerId = SelectedCustomer.CustomerId;
            data.CreatedAt = SelectedCustomer.CreatedAt;
        }
        else
        {
            data.CreatedAt = System.DateTime.UtcNow;
        }

        return data;
    }

    private System.Int32 IndexOfCustomer(System.Object? customerId)
    {
        if (customerId is null)
        {
            return -1;
        }

        for (System.Int32 i = 0; i < Customers.Count; i++)
        {
            if (Customers[i].CustomerId is System.Object id && id.Equals(customerId))
            {
                return i;
            }
        }

        return -1;
    }

    private void HandleWriteError(System.String title, System.String message, ProtocolAdvice advice)
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