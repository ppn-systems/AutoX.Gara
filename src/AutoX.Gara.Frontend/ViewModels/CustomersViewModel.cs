// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

public sealed partial class CustomersViewModel : ObservableObject, System.IDisposable
{
    private readonly ICustomerService _customerService;
    private System.Threading.CancellationTokenSource? _cts;
    private System.Threading.CancellationTokenSource? _searchCts; // FIX: dùng CTS thay Timer

    private const System.Int32 DefaultPageSize = 5;
    private const System.Int32 SearchDebounceMs = 400;

    // ─── Pagination ───────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }
    [ObservableProperty] public partial System.Int32 TotalCount { get; set; }

    public System.Int32 TotalPages => TotalCount > 0 ? (System.Int32)System.Math.Ceiling((System.Double)TotalCount / DefaultPageSize) : 0;

    // ─── Search / Sort ────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.String SearchTerm { get; set; } = System.String.Empty;
    [ObservableProperty] public partial CustomerSortField SortBy { get; set; } = CustomerSortField.CreatedAt;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = true;

    // ─── Filter ───────────────────────────────────────────────────────────────

    // Giữ nguyên FilterType/FilterMembership để logic không đổi
    [ObservableProperty] public partial CustomerType FilterType { get; set; } = CustomerType.None;
    [ObservableProperty] public partial MembershipLevel FilterMembership { get; set; } = MembershipLevel.None;

    // FIX PICKER: Picker.SelectedIndex (int) thay vì SelectedItem (string→enum không match)
    // 0=Tất cả, 1=Cá nhân, 2=Doanh nghiệp
    [ObservableProperty] public partial System.Int32 PickerFilterTypeIndex { get; set; } = 0;
    // 0=Tất cả, 1=Bronze, 2=Silver, 3=Gold, 4=Platinum
    [ObservableProperty] public partial System.Int32 PickerMembershipIndex { get; set; } = 0;

    public System.Boolean HasActiveFilters => FilterType != CustomerType.None || FilterMembership != MembershipLevel.None;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Customers.Count == 0;

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
    [ObservableProperty] public partial CustomerDto? SelectedCustomer { get; set; }

    // FIX: thay StringFormat bool không hoạt động — dùng computed property
    public System.String FormTitle => IsEditing ? "Sửa khách hàng" : "Thêm khách hàng";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm khách hàng";

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

    // Picker index cho form
    [ObservableProperty] public partial System.Int32 FormPickerTypeIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormPickerMembershipIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormPickerGenderIndex { get; set; } = 0;

    // ─── Delete Confirm ───────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }
    public System.String DeleteConfirmName => SelectedCustomer?.Name ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<CustomerDto> Customers { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        Customers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
        _ = LoadAsync();
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedCustomerChanged(CustomerDto? value)
        => OnPropertyChanged(nameof(DeleteConfirmName));

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

    // FIX: CancellationTokenSource debounce — không leak, cancel ngay lập tức
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

    partial void OnSortByChanged(CustomerSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();

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

    // Picker index → enum (filter)
    // CustomerType: 0=None,1=Individual,2=Business,3=Government,4=Fleet,5=Insurance,6=VIP,7=Potential,8=Supplier,9=NonProfit,10=Dealer,11=Other
    partial void OnPickerFilterTypeIndexChanged(int value)
    {
        FilterType = value switch
        {
            1 => CustomerType.Individual,
            2 => CustomerType.Business,
            3 => CustomerType.Government,
            4 => CustomerType.Fleet,
            5 => CustomerType.InsuranceCompany,
            6 => CustomerType.VIP,
            7 => CustomerType.Potential,
            8 => CustomerType.Supplier,
            9 => CustomerType.NonProfit,
            10 => CustomerType.Dealer,
            11 => CustomerType.Other,
            _ => CustomerType.None
        };
    }

    // MembershipLevel: 0=None,1=Trial,2=Standard,3=Silver,4=Gold,5=Platinum,6=Diamond
    partial void OnPickerMembershipIndexChanged(int value)
    {
        FilterMembership = value switch
        {
            1 => MembershipLevel.Trial,
            2 => MembershipLevel.Standard,
            3 => MembershipLevel.Silver,
            4 => MembershipLevel.Gold,
            5 => MembershipLevel.Platinum,
            6 => MembershipLevel.Diamond,
            _ => MembershipLevel.None
        };
    }

    // Picker index → enum (form)
    partial void OnFormPickerTypeIndexChanged(int value)
    {
        FormType = value switch
        {
            1 => CustomerType.Individual,
            2 => CustomerType.Business,
            3 => CustomerType.Government,
            4 => CustomerType.Fleet,
            5 => CustomerType.InsuranceCompany,
            6 => CustomerType.VIP,
            7 => CustomerType.Potential,
            8 => CustomerType.Supplier,
            9 => CustomerType.NonProfit,
            10 => CustomerType.Dealer,
            11 => CustomerType.Other,
            _ => CustomerType.None
        };
    }

    partial void OnFormPickerMembershipIndexChanged(int value)
    {
        FormMembership = value switch
        {
            1 => MembershipLevel.Trial,
            2 => MembershipLevel.Standard,
            3 => MembershipLevel.Silver,
            4 => MembershipLevel.Gold,
            5 => MembershipLevel.Platinum,
            6 => MembershipLevel.Diamond,
            _ => MembershipLevel.None
        };
    }

    partial void OnFormPickerGenderIndexChanged(int value)
    {
        FormGender = value switch
        {
            1 => Gender.Male,
            2 => Gender.Female,
            _ => Gender.None
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
            CustomerListResult result = await _customerService.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterType: FilterType,
                filterMembership: FilterMembership,
                ct: ct);

            Debug.WriteLine(
                $"[VM] Load ok={result.IsSuccess} count={result.Customers.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Customers.Clear();
                foreach (CustomerDto c in result.Customers)
                {
                    Customers.Add(c);
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
        finally
        {
            IsLoading = false;
        }
    }

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
        else { SortBy = field; SortDescending = false; }
    }

    [RelayCommand]
    private void ClearSearch() => SearchTerm = System.String.Empty;

    [RelayCommand]
    private void ClearFilters()
    {
        PickerFilterTypeIndex = 0;
        PickerMembershipIndex = 0;
        // OnPickerXxxChanged tự cập nhật FilterType / FilterMembership
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
    private void OpenEditForm(CustomerDto customer)
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

        FormPickerTypeIndex = (customer.Type ?? CustomerType.None) switch
        {
            CustomerType.Individual => 1,
            CustomerType.Business => 2,
            CustomerType.Government => 3,
            CustomerType.Fleet => 4,
            CustomerType.InsuranceCompany => 5,
            CustomerType.VIP => 6,
            CustomerType.Potential => 7,
            CustomerType.Supplier => 8,
            CustomerType.NonProfit => 9,
            CustomerType.Dealer => 10,
            CustomerType.Other => 11,
            _ => 0
        };

        FormPickerMembershipIndex = (customer.Membership ?? MembershipLevel.None) switch
        {
            MembershipLevel.Trial => 1,
            MembershipLevel.Standard => 2,
            MembershipLevel.Silver => 3,
            MembershipLevel.Gold => 4,
            MembershipLevel.Platinum => 5,
            MembershipLevel.Diamond => 6,
            _ => 0
        };

        FormPickerGenderIndex = (customer.Gender ?? Gender.None) switch
        {
            Gender.Male => 1,
            Gender.Female => 2,
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
            CustomerDto data = BuildPacketFromForm();
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
    private static async System.Threading.Tasks.Task OpenVehiclesAsync(CustomerDto customer)
    {
        // Tạo page mới, truyền customer context vào trước khi navigate
        var page = new Views.VehiclesPage();
        page.Initialize(customer);

        // Push page lên navigation stack của Shell
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private static async System.Threading.Tasks.Task OpenInvoicesAsync(CustomerDto customer)
    {
        var page = new Views.InvoicesPage();
        page.Initialize(customer);
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private void RequestDelete(CustomerDto customer)
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

        CustomerDto toDelete = SelectedCustomer;

        try
        {
            CustomerWriteResult result = await _customerService.DeleteAsync(toDelete, ct);

            if (result.IsSuccess)
            {
                Customers.Remove(toDelete);
                TotalCount = System.Math.Max(0, TotalCount - 1);
                SelectedCustomer = null;
                OnPropertyChanged(nameof(TotalPages));
                HasNextPage = CurrentPage < TotalPages;

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
        FormName = FormEmail = FormPhone = FormAddress = FormTaxCode = FormNotes = System.String.Empty;
        FormDateOfBirth = null;
        FormPickerTypeIndex = 0;
        FormPickerMembershipIndex = 0;
        FormPickerGenderIndex = 0;
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
        { SetFormError("Tên khách hàng không được để trống."); return false; }

        if (FormName.Length > 100)
        { SetFormError("Tên không được vượt quá 100 ký tự."); return false; }

        if (!AccountValidation.IsValidEmail(FormEmail))
        { SetFormError("Email không hợp lệ."); return false; }

        if (!AccountValidation.IsValidVietnamPhoneNumber(FormPhone))
        { SetFormError("Số điện thoại không hợp lệ (VD: 0901234567)."); return false; }

        if (FormDateOfBirth.HasValue)
        {
            if (FormDateOfBirth.Value > System.DateTime.Today)
            { SetFormError("Ngày sinh không được là ngày trong tương lai."); return false; }

            if (FormDateOfBirth.Value < System.DateTime.Today.AddYears(-120))
            { SetFormError("Ngày sinh không hợp lệ."); return false; }
        }

        if (FormType == CustomerType.Business && System.String.IsNullOrWhiteSpace(FormTaxCode))
        { SetFormError("Mã số thuế bắt buộc đối với khách hàng doanh nghiệp."); return false; }

        if (FormNotes.Length > 500)
        { SetFormError("Ghi chú không được vượt quá 500 ký tự."); return false; }

        return true;
    }

    private CustomerDto BuildPacketFromForm() => new()
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
        UpdatedAt = System.DateTime.UtcNow,
        CustomerId = IsEditing ? SelectedCustomer?.CustomerId : null,
        CreatedAt = IsEditing ? SelectedCustomer?.CreatedAt ?? System.DateTime.UtcNow
                                : System.DateTime.UtcNow
    };

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
