// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Frontend.Results.Employees;
using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel for employee management.
/// </summary>
public sealed partial class EmployeesViewModel : ObservableObject, System.IDisposable
{
    private readonly EmployeeService _service;
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
    [ObservableProperty] public partial EmployeeSortField SortBy { get; set; } = EmployeeSortField.Name;
    [ObservableProperty] public partial System.Boolean SortDescending { get; set; } = false;

    // ─── Filter ───────────────────────────────────────────────────────────────

    [ObservableProperty] public partial Position FilterPosition { get; set; } = Position.None;
    [ObservableProperty] public partial EmploymentStatus FilterStatus { get; set; } = EmploymentStatus.None;
    [ObservableProperty] public partial Gender FilterGender { get; set; } = Gender.None;

    [ObservableProperty] public partial System.Int32 PickerPositionIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerStatusIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 PickerGenderIndex { get; set; } = 0;

    public System.Boolean HasActiveFilters
        => FilterPosition != Position.None || FilterStatus != EmploymentStatus.None || FilterGender != Gender.None;

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    public System.Boolean IsEmpty => !IsLoading && Employees.Count == 0;

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
    [ObservableProperty] public partial EmployeeDto? SelectedEmployee { get; set; }

    public System.String FormTitle => IsEditing ? "Sửa nhân viên" : "Thêm nhân viên";
    public System.String FormSaveText => IsEditing ? "Lưu thay đổi" : "Thêm nhân viên";

    [ObservableProperty] public partial System.String FormName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormEmail { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormAddress { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPhoneNumber { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.Int32 FormGenderIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormPositionIndex { get; set; } = 0;
    [ObservableProperty] public partial System.Int32 FormStatusIndex { get; set; } = 1;
    [ObservableProperty] public partial System.DateTime FormDateOfBirth { get; set; } = System.DateTime.Today.AddYears(-20);
    [ObservableProperty] public partial System.DateTime FormStartDate { get; set; } = System.DateTime.Today;
    [ObservableProperty] public partial System.DateTime? FormEndDate { get; set; } = null;
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Status Confirm ───────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsStatusConfirmVisible { get; set; }
    [ObservableProperty] public partial System.Int32 NewStatusIndex { get; set; } = 1;
    public System.String StatusConfirmName => SelectedEmployee?.Name ?? System.String.Empty;

    // ─── Collection ───────────────────────────────────────────────────────────

    public System.Collections.ObjectModel.ObservableCollection<EmployeeDto> Employees { get; } = [];

    // ─── Constructor ───────────────────────────────────────────────────────────

    public EmployeesViewModel(EmployeeService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Employees.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));
    }

    // ─── Property Hooks ───────────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSelectedEmployeeChanged(EmployeeDto? value) => OnPropertyChanged(nameof(StatusConfirmName));
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
    partial void OnSortByChanged(EmployeeSortField value) => _ = LoadAsync();
    partial void OnSortDescendingChanged(bool value) => _ = LoadAsync();
    partial void OnFilterPositionChanged(Position value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterStatusChanged(EmploymentStatus value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnFilterGenderChanged(Gender value)
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        ResetPageAndLoad();
    }
    partial void OnPickerPositionIndexChanged(int value)
    {
        FilterPosition = (Position)value;
    }
    partial void OnPickerStatusIndexChanged(int value)
    {
        FilterStatus = (EmploymentStatus)value;
    }
    partial void OnPickerGenderIndexChanged(int value)
    {
        FilterGender = (Gender)value;
    }

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
            EmployeeListResult result = await _service.GetListAsync(
                page: CurrentPage,
                pageSize: DefaultPageSize,
                searchTerm: SearchTerm,
                sortBy: SortBy,
                sortDescending: SortDescending,
                filterPosition: FilterPosition,
                filterStatus: FilterStatus,
                filterGender: FilterGender,
                ct: ct);

            Debug.WriteLine($"[EmployeesVM] Load ok={result.IsSuccess} count={result.Employees.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Employees.Clear();
                foreach (EmployeeDto e in result.Employees)
                {
                    Employees.Add(e);
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
        PickerPositionIndex = 0;
        PickerStatusIndex = 0;
        PickerGenderIndex = 0;
    }

    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedEmployee = null;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void OpenEditForm(EmployeeDto employee)
    {
        IsEditing = true;
        SelectedEmployee = employee;

        FormName = employee.Name ?? System.String.Empty;
        FormEmail = employee.Email ?? System.String.Empty;
        FormAddress = employee.Address ?? System.String.Empty;
        FormPhoneNumber = employee.PhoneNumber ?? System.String.Empty;
        FormGenderIndex = (System.Int32)(employee.Gender ?? Gender.None);
        FormPositionIndex = (System.Int32)(employee.Position ?? Position.None);
        FormStatusIndex = (System.Int32)(employee.Status ?? EmploymentStatus.None);
        FormDateOfBirth = employee.DateOfBirth ?? System.DateTime.Today.AddYears(-20);
        FormStartDate = employee.StartDate ?? System.DateTime.Today;
        FormEndDate = employee.EndDate;

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
            EmployeeDto data = BuildPacketFromForm();
            EmployeeWriteResult result = IsEditing
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
                        System.Int32 idx = IndexOfEmployee(result.UpdatedEntity.EmployeeId);
                        if (idx >= 0)
                        {
                            Employees[idx] = result.UpdatedEntity;
                        }
                        else
                        {
                            await LoadAsync();
                        }
                    }
                    else
                    {
                        Employees.Insert(0, result.UpdatedEntity);
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
    private void RequestChangeStatus(EmployeeDto employee)
    {
        SelectedEmployee = employee;
        NewStatusIndex = (System.Int32)(employee.Status ?? EmploymentStatus.None);
        IsStatusConfirmVisible = true;
    }

    [RelayCommand]
    private void CancelChangeStatus() => IsStatusConfirmVisible = false;

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmChangeStatusAsync()
    {
        if (SelectedEmployee is null)
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
            EmployeeDto data = new()
            {
                EmployeeId = SelectedEmployee.EmployeeId,
                Status = (EmploymentStatus)NewStatusIndex,
                SequenceId = Nalix.Framework.Random.Csprng.NextUInt32()
            };

            EmployeeWriteResult result = await _service.ChangeStatusAsync(data, ct);

            if (result.IsSuccess)
            {
                System.Int32 idx = IndexOfEmployee(SelectedEmployee.EmployeeId);
                if (idx >= 0)
                {
                    EmployeeDto updated = Employees[idx];
                    updated.Status = (EmploymentStatus)NewStatusIndex;
                    Employees[idx] = updated;
                }
                SelectedEmployee = null;
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
        FormPhoneNumber = System.String.Empty;
        FormGenderIndex = 0;
        FormPositionIndex = 0;
        FormStatusIndex = 1;
        FormDateOfBirth = System.DateTime.Today.AddYears(-20);
        FormStartDate = System.DateTime.Today;
        FormEndDate = null;
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
        { SetFormError("Tên nhân viên không được để trống."); return false; }

        if (FormName.Length > 50)
        { SetFormError("Tên không được vượt quá 50 ký tự."); return false; }

        if (!IsValidEmail(FormEmail))
        { SetFormError("Email không hợp lệ."); return false; }

        if (!System.String.IsNullOrWhiteSpace(FormPhoneNumber))
        {
            if (!IsValidPhone(FormPhoneNumber))
            { SetFormError("Số điện thoại không hợp lệ (10-14 chữ số)."); return false; }
        }

        if (FormDateOfBirth >= System.DateTime.Today)
        { SetFormError("Ngày sinh phải trong quá khứ."); return false; }

        if (FormEndDate.HasValue && FormEndDate <= FormStartDate)
        { SetFormError("Ngày kết thúc phải sau ngày bắt đầu."); return false; }

        return true;
    }

    private EmployeeDto BuildPacketFromForm()
    {
        return new EmployeeDto
        {
            EmployeeId = IsEditing ? SelectedEmployee?.EmployeeId : null,
            Name = FormName,
            Email = FormEmail,
            Address = FormAddress,
            PhoneNumber = FormPhoneNumber,
            Gender = (Gender)FormGenderIndex,
            Position = (Position)FormPositionIndex,
            Status = (EmploymentStatus)FormStatusIndex,
            DateOfBirth = FormDateOfBirth,
            StartDate = FormStartDate,
            EndDate = FormEndDate,
            SequenceId = Nalix.Framework.Random.Csprng.NextUInt32()
        };
    }

    private System.Int32 IndexOfEmployee(System.Object? id)
    {
        if (id is null)
        {
            return -1;
        }

        for (System.Int32 i = 0; i < Employees.Count; i++)
        {
            if (Employees[i].EmployeeId is System.Object employeeId && employeeId.Equals(id))
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

    private static System.Boolean IsValidPhone(System.String phone) => System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{10,14}$");
}