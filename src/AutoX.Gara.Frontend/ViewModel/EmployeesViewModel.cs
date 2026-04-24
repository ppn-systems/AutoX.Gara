// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Employees;
using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoX.Gara.Frontend.Controllers;

/// <summary>

/// ViewModel for employee management.

/// </summary>

public sealed partial class EmployeesViewModel : ObservableObject, System.IDisposable

{
    private readonly EmployeeService _service;

    private readonly EmployeeSalaryService _salaryService;

    private System.Threading.CancellationTokenSource? _loadCts;

    private System.Threading.CancellationTokenSource? _writeCts;

    private System.Threading.CancellationTokenSource? _searchCts;

    private System.Threading.CancellationTokenSource? _salaryCts;

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

    [ObservableProperty] public partial EmployeeSortField SortBy { get; set; } = EmployeeSortField.Name;

    [ObservableProperty] public partial bool SortDescending { get; set; } = false;

    // --- Filter ---------------------------------------------------------------

    [ObservableProperty] public partial Position FilterPosition { get; set; } = Position.None;

    [ObservableProperty] public partial EmploymentStatus FilterStatus { get; set; } = EmploymentStatus.None;

    [ObservableProperty] public partial Gender FilterGender { get; set; } = Gender.None;

    private static readonly Position[] FilterPositionValues =

    [

        Position.None,

        Position.Apprentice,

        Position.CarWasher,

        Position.AutoElectrician,

        Position.UnderCarMechanic,

        Position.BodyworkMechanic,

        Position.Technician,

        Position.Receptionist,

        Position.Advisor,

        Position.Support,

        Position.Accountant,

        Position.Manager,

    ];

    private static readonly EmploymentStatus[] StatusValues = System.Enum.GetValues<EmploymentStatus>();

    private static readonly Gender[] GenderValues = System.Enum.GetValues<Gender>();

    private static readonly Position[] FormPositionValues = System.Enum.GetValues<Position>();

    public String[] FilterPositionOptions { get; } = [.. FilterPositionValues.Select((v, idx) => idx == 0 ? "T?t c? ch?c v?" : EnumText.Get(v))];

    public String[] FilterStatusOptions { get; } = [.. StatusValues.Select((v, idx) => idx == 0 ? "T?t c? tr?ng th�i" : EnumText.Get(v))];

    public String[] FilterGenderOptions { get; } = [.. GenderValues.Select((v, idx) => idx == 0 ? "T?t c?" : EnumText.Get(v))];

    public String[] FormGenderOptions { get; } = [.. GenderValues.Select(EnumText.Get)];

    public String[] FormPositionOptions { get; } = [.. FormPositionValues.Select(EnumText.Get)];

    public String[] FormStatusOptions { get; } = [.. StatusValues.Select(EnumText.Get)];

    public String[] ChangeStatusOptions { get; } = [.. StatusValues.Select(EnumText.Get)];

    [ObservableProperty] public partial int PickerPositionIndex { get; set; } = 0;

    [ObservableProperty] public partial int PickerStatusIndex { get; set; } = 0;

    [ObservableProperty] public partial int PickerGenderIndex { get; set; } = 0;

    [ObservableProperty] public partial int PickerSalaryIndex { get; set; } = 0;

    public String[] SalaryFilterOptions { get; } =

    [

        "T?t c? luong",

        "C� luong",

        "Chua c� luong",

        "Theo th�ng",

        "Theo ng�y",

        "Theo gi?",

    ];

    public string SelectedPositionText

        => FilterPositionOptions[System.Math.Clamp(PickerPositionIndex, 0, FilterPositionOptions.Length - 1)];

    public string SelectedStatusText

        => FilterStatusOptions[System.Math.Clamp(PickerStatusIndex, 0, FilterStatusOptions.Length - 1)];

    public string SelectedGenderText

        => FilterGenderOptions[System.Math.Clamp(PickerGenderIndex, 0, FilterGenderOptions.Length - 1)];

    public string SelectedSalaryText

        => SalaryFilterOptions[System.Math.Clamp(PickerSalaryIndex, 0, SalaryFilterOptions.Length - 1)];

    public bool HasActiveFilters

        => FilterPosition != Position.None

        || FilterStatus != EmploymentStatus.None

        || FilterGender != Gender.None

        || PickerSalaryIndex != 0;

    // --- State ----------------------------------------------------------------

    [ObservableProperty] public partial bool IsLoading { get; set; }

    [ObservableProperty] public partial bool HasError { get; set; }

    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    public IEnumerable<EmployeeRow> FilteredEmployees

        => ApplySalaryFilter(Employees);

    public bool IsEmpty => !IsLoading && !FilteredEmployees.Any();

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

    [ObservableProperty] public partial EmployeeRow? SelectedEmployee { get; set; }

    public string FormTitle => IsEditing ? "S?a nh�n vi�n" : "Th�m nh�n vi�n";

    public string FormSaveText => IsEditing ? "Luu thay d?i" : "Th�m nh�n vi�n";

    [ObservableProperty] public partial string FormName { get; set; } = string.Empty;

    [ObservableProperty] public partial string FormEmail { get; set; } = string.Empty;

    [ObservableProperty] public partial string FormAddress { get; set; } = string.Empty;

    [ObservableProperty] public partial string FormPhoneNumber { get; set; } = string.Empty;

    [ObservableProperty] public partial int FormGenderIndex { get; set; } = 0;

    [ObservableProperty] public partial int FormPositionIndex { get; set; } = 0;

    [ObservableProperty] public partial int FormStatusIndex { get; set; } = 1;

    [ObservableProperty] public partial DateTime FormDateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

    [ObservableProperty] public partial DateTime FormStartDate { get; set; } = DateTime.Today;

    public string SelectedFormGenderText =>

        FormGenderOptions[System.Math.Clamp(FormGenderIndex, 0, FormGenderOptions.Length - 1)];

    public string SelectedFormPositionText =>

        FormPositionOptions[System.Math.Clamp(FormPositionIndex, 0, FormPositionOptions.Length - 1)];

    public string SelectedFormStatusText =>

        FormStatusOptions[System.Math.Clamp(FormStatusIndex, 0, FormStatusOptions.Length - 1)];

    [ObservableProperty] public partial DateTime? FormEndDate { get; set; } = null;

    [ObservableProperty] public partial bool HasFormError { get; set; }

    [ObservableProperty] public partial string? FormErrorMessage { get; set; }

    // --- Status Confirm -------------------------------------------------------

    [ObservableProperty] public partial bool IsStatusConfirmVisible { get; set; }

    [ObservableProperty] public partial int NewStatusIndex { get; set; } = 1;

    public string StatusConfirmName => SelectedEmployee?.Dto?.Name ?? string.Empty;

    public string SelectedNewStatusText =>

        ChangeStatusOptions[System.Math.Clamp(NewStatusIndex, 0, ChangeStatusOptions.Length - 1)];

    // --- Collection -----------------------------------------------------------

    public System.Collections.ObjectModel.ObservableCollection<EmployeeRow> Employees { get; } = [];

    public sealed class EmployeeRow : ObservableObject

    {
        public EmployeeDto Dto { get; }

        private string _salaryText = "Chua c�";

        public string SalaryText

        {
            get => _salaryText;

            set => SetProperty(ref _salaryText, value);

        }

        private bool _hasSalary;

        public bool HasSalary

        {
            get => _hasSalary;

            set => SetProperty(ref _hasSalary, value);

        }

        private SalaryType _latestSalaryType = SalaryType.None;

        public SalaryType LatestSalaryType

        {
            get => _latestSalaryType;

            set => SetProperty(ref _latestSalaryType, value);

        }

        public EmployeeRow(EmployeeDto dto) => Dto = dto;

    }

    // --- Salary Form (per-employee) -------------------------------------------

    [ObservableProperty] public partial bool IsSalaryFormVisible { get; set; }

    [ObservableProperty] public partial bool IsSalaryEditing { get; set; }

    [ObservableProperty] public partial EmployeeRow? SalaryEmployee { get; set; }

    [ObservableProperty] public partial EmployeeSalaryDto? SelectedSalary { get; set; }

    public int SalaryEmployeeId => SalaryEmployee?.Dto?.EmployeeId ?? 0;

    public string SalaryEmployeeName => SalaryEmployee?.Dto?.Name ?? string.Empty;

    private static readonly SalaryType[] SalaryTypeValues = System.Enum.GetValues<SalaryType>();

    public String[] SalaryFormTypeOptions { get; } = SalaryTypeValues.Select(EnumText.Get).ToArray();

    [ObservableProperty] public partial decimal SalaryFormSalary { get; set; }

    [ObservableProperty]

    public partial int SalaryFormTypeIndex { get; set; } =

        System.Array.IndexOf(SalaryTypeValues, SalaryType.Monthly);

    [ObservableProperty] public partial decimal SalaryFormUnit { get; set; } = 1;

    [ObservableProperty] public partial DateTime SalaryFormEffectiveFrom { get; set; } = DateTime.Today;

    [ObservableProperty] public partial string SalaryFormNote { get; set; } = string.Empty;

    [ObservableProperty] public partial bool HasSalaryFormError { get; set; }

    [ObservableProperty] public partial string? SalaryFormErrorMessage { get; set; }

    public String SalaryFormTitle => IsSalaryEditing ? "Ch?nh s?a luong" : "Thi?t l?p luong";

    public String SalaryFormSaveText => IsSalaryEditing ? "Luu" : "T?o";

    public string SelectedSalaryFormTypeText =>

        SalaryFormTypeOptions[System.Math.Clamp(SalaryFormTypeIndex, 0, SalaryFormTypeOptions.Length - 1)];

    // --- Constructor -----------------------------------------------------------

    public EmployeesViewModel(EmployeeService service, EmployeeSalaryService salaryService)

    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _salaryService = salaryService ?? throw new ArgumentNullException(nameof(salaryService));

        Employees.CollectionChanged += (_, _) =>

        {
            OnPropertyChanged(nameof(IsEmpty));

            OnPropertyChanged(nameof(FilteredEmployees));

        };

        // Some MAUI/WinUI layouts (especially when using gesture-based "picker" UI) can render

        // the initial text as blank until a PropertyChanged is raised. Force a one-time refresh.

        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>

        {
            OnPropertyChanged(nameof(SelectedPositionText));

            OnPropertyChanged(nameof(SelectedStatusText));

            OnPropertyChanged(nameof(SelectedGenderText));

            OnPropertyChanged(nameof(SelectedSalaryText));

            // Form/dropdown labels

            OnPropertyChanged(nameof(SelectedFormGenderText));

            OnPropertyChanged(nameof(SelectedFormPositionText));

            OnPropertyChanged(nameof(SelectedFormStatusText));

            OnPropertyChanged(nameof(SelectedNewStatusText));

            OnPropertyChanged(nameof(SelectedSalaryFormTypeText));

        });

    }

    // --- Property Hooks -------------------------------------------------------

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(TotalPages));

    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnSelectedEmployeeChanged(EmployeeRow? value) => OnPropertyChanged(nameof(StatusConfirmName));

    partial void OnNewStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedNewStatusText));

    partial void OnSalaryFormTypeIndexChanged(int value) => OnPropertyChanged(nameof(SelectedSalaryFormTypeText));

    partial void OnFormGenderIndexChanged(int value) => OnPropertyChanged(nameof(SelectedFormGenderText));

    partial void OnFormPositionIndexChanged(int value) => OnPropertyChanged(nameof(SelectedFormPositionText));

    partial void OnFormStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedFormStatusText));

    partial void OnPickerSalaryIndexChanged(int value)

    {
        if (value < 0)

        {
            PickerSalaryIndex = 0;

            return;

        }

        OnPropertyChanged(nameof(SelectedSalaryText));

        OnPropertyChanged(nameof(FilteredEmployees));

        OnPropertyChanged(nameof(IsEmpty));

        OnPropertyChanged(nameof(HasActiveFilters));

    }

    partial void OnIsEditingChanged(bool value)

    {
        OnPropertyChanged(nameof(FormTitle));

        OnPropertyChanged(nameof(FormSaveText));

    }

    partial void OnIsSalaryEditingChanged(bool value)

    {
        OnPropertyChanged(nameof(SalaryFormTitle));

        OnPropertyChanged(nameof(SalaryFormSaveText));

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
        if (value < 0)

        {
            PickerPositionIndex = 0;

            return;

        }

        FilterPosition = FilterPositionValues[System.Math.Clamp(value, 0, FilterPositionValues.Length - 1)];

        OnPropertyChanged(nameof(SelectedPositionText));

        OnPropertyChanged(nameof(FilteredEmployees));

        OnPropertyChanged(nameof(IsEmpty));

    }

    partial void OnPickerStatusIndexChanged(int value)

    {
        if (value < 0)

        {
            PickerStatusIndex = 0;

            return;

        }

        FilterStatus = StatusValues[System.Math.Clamp(value, 0, StatusValues.Length - 1)];

        OnPropertyChanged(nameof(SelectedStatusText));

        OnPropertyChanged(nameof(FilteredEmployees));

        OnPropertyChanged(nameof(IsEmpty));

    }

    partial void OnPickerGenderIndexChanged(int value)

    {
        if (value < 0)

        {
            PickerGenderIndex = 0;

            return;

        }

        FilterGender = GenderValues[System.Math.Clamp(value, 0, GenderValues.Length - 1)];

        OnPropertyChanged(nameof(SelectedGenderText));

        OnPropertyChanged(nameof(FilteredEmployees));

        OnPropertyChanged(nameof(IsEmpty));

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

            InstanceManager.Instance.GetExistingInstance<ILogger>()?

                                    .Info($"[EmployeesVM] Load ok={result.IsSuccess} count={result.Employees.Count} total={result.TotalCount}");

            if (ct.IsCancellationRequested)

            {
                return;

            }

            if (result.IsSuccess)

            {
                Employees.Clear();

                List<EmployeeRow> rows = [];

                foreach (EmployeeDto e in result.Employees)

                {
                    var row = new EmployeeRow(e)

                    {
                        SalaryText = "Chua c�",

                        HasSalary = false,

                        LatestSalaryType = SalaryType.None

                    };

                    Employees.Add(row);

                    rows.Add(row);

                }

                TotalCount = result.TotalCount >= 0 ? result.TotalCount : TotalCount;

                HasNextPage = result.TotalCount >= 0 ? CurrentPage < TotalPages : result.HasMore;

                // Warmup salary text in the background (doesn't block list rendering).

                _ = WarmupSalarySummariesAsync(rows);

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
        PickerPositionIndex = 0;

        PickerStatusIndex = 0;

        PickerGenderIndex = 0;

        PickerSalaryIndex = 0;

    }

    // --- Filter Pickers (ActionSheet) -----------------------------------------

    [RelayCommand]

    private async System.Threading.Tasks.Task PickPositionAsync()

    {
        var page = Microsoft.Maui.Controls.Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Ch?n ch?c v?", "H?y", null, FilterPositionOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx = System.Array.IndexOf(FilterPositionOptions, pick);

        PickerPositionIndex = idx >= 0 ? idx : 0;

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task PickStatusAsync()

    {
        var page = Microsoft.Maui.Controls.Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Ch?n tr?ng th�i", "H?y", null, FilterStatusOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx = System.Array.IndexOf(FilterStatusOptions, pick);

        PickerStatusIndex = idx >= 0 ? idx : 0;

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task PickGenderAsync()

    {
        var page = Microsoft.Maui.Controls.Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("Ch?n gi?i t�nh", "H?y", null, FilterGenderOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx = System.Array.IndexOf(FilterGenderOptions, pick);

        PickerGenderIndex = idx >= 0 ? idx : 0;

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task PickSalaryAsync()

    {
        var page = Microsoft.Maui.Controls.Application.Current?.Windows[0].Page;

        if (page is null)

        {
            return;

        }

        String pick = await page.DisplayActionSheetAsync("L?c theo luong", "H?y", null, SalaryFilterOptions);

        if (pick == "H?y" || String.IsNullOrWhiteSpace(pick))

        {
            return;

        }

        Int32 idx = System.Array.IndexOf(SalaryFilterOptions, pick);

        PickerSalaryIndex = idx >= 0 ? idx : 0;

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

    private void OpenEditForm(EmployeeRow employee)

    {
        IsEditing = true;

        SelectedEmployee = employee;

        EmployeeDto dto = employee.Dto;

        FormName = dto.Name ?? string.Empty;

        FormEmail = dto.Email ?? string.Empty;

        FormAddress = dto.Address ?? string.Empty;

        FormPhoneNumber = dto.PhoneNumber ?? string.Empty;

        FormGenderIndex = System.Array.IndexOf(GenderValues, dto.Gender ?? Gender.None);

        if (FormGenderIndex < 0)

        {
            FormGenderIndex = 0;

        }

        FormPositionIndex = System.Array.IndexOf(FormPositionValues, dto.Position ?? Position.None);

        if (FormPositionIndex < 0)

        {
            FormPositionIndex = 0;

        }

        FormStatusIndex = System.Array.IndexOf(StatusValues, dto.Status ?? EmploymentStatus.None);

        if (FormStatusIndex < 0)

        {
            FormStatusIndex = 0;

        }

        FormDateOfBirth = dto.DateOfBirth ?? DateTime.Today.AddYears(-20);

        FormStartDate = dto.StartDate ?? DateTime.Today;

        FormEndDate = dto.EndDate;

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
                        int idx = IndexOfEmployee(result.UpdatedEntity.EmployeeId);

                        if (idx >= 0)

                        {
                            Employees[idx] = new EmployeeRow(result.UpdatedEntity)

                            {
                                SalaryText = Employees[idx].SalaryText,

                                HasSalary = Employees[idx].HasSalary

                            };

                        }

                        else

                        {
                            await LoadAsync();

                        }

                    }

                    else

                    {
                        Employees.Insert(0, new EmployeeRow(result.UpdatedEntity));

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

    private void RequestChangeStatus(EmployeeRow employee)

    {
        SelectedEmployee = employee;

        NewStatusIndex = System.Array.IndexOf(StatusValues, employee.Dto.Status ?? EmploymentStatus.None);

        if (NewStatusIndex < 0)

        {
            NewStatusIndex = 0;

        }

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
                EmployeeId = SelectedEmployee.Dto.EmployeeId,

                Status = StatusValues[System.Math.Clamp(NewStatusIndex, 0, StatusValues.Length - 1)],

                SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()

            };

            EmployeeWriteResult result = await _service.ChangeStatusAsync(data, ct);

            if (result.IsSuccess)

            {
                int idx = IndexOfEmployee(SelectedEmployee.Dto.EmployeeId);

                if (idx >= 0)

                {
                    EmployeeRow updated = Employees[idx];

                    updated.Dto.Status = StatusValues[System.Math.Clamp(NewStatusIndex, 0, StatusValues.Length - 1)];

                    Employees[idx] = updated;

                }

                SelectedEmployee = null;

            }

            else

            {
                HandleError("�?i tr?ng th�i th?t b?i", result.ErrorMessage!, result.Advice);

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

        _salaryCts?.Cancel();

        _salaryCts?.Dispose();

    }

    // --- Salary Commands (per-employee) --------------------------------------

    [RelayCommand]

    private async System.Threading.Tasks.Task OpenSalaryAsync(EmployeeRow row)

    {
        if (row?.Dto?.EmployeeId is null)

        {
            return;

        }

        SalaryEmployee = row;

        ClearSalaryFormError();

        // Default form state.

        IsSalaryEditing = false;

        SelectedSalary = null;

        SalaryFormSalary = 0;

        SalaryFormTypeIndex = System.Array.IndexOf(SalaryTypeValues, SalaryType.Monthly);

        SalaryFormUnit = 1;

        SalaryFormEffectiveFrom = DateTime.Today;

        SalaryFormNote = string.Empty;

        IsSalaryFormVisible = true;

        // Prefill by latest salary (if any).

        EmployeeSalaryDto? latest = await GetLatestSalaryAsync(row.Dto.EmployeeId.Value).ConfigureAwait(false);

        if (latest is null)

        {
            return;

        }

        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>

        {
            IsSalaryEditing = true;

            SelectedSalary = latest;

            SalaryFormSalary = latest.Salary;

            SalaryFormTypeIndex = System.Array.IndexOf(SalaryTypeValues, latest.SalaryType);

            if (SalaryFormTypeIndex < 0)

            {
                SalaryFormTypeIndex = 0;

            }

            SalaryFormUnit = latest.SalaryUnit <= 0 ? 1 : latest.SalaryUnit;

            SalaryFormEffectiveFrom = latest.EffectiveFrom.ToLocalTime().Date;

            SalaryFormNote = latest.Note ?? string.Empty;

        });

    }

    [RelayCommand]

    private void CloseSalaryForm()

    {
        IsSalaryFormVisible = false;

        SalaryEmployee = null;

        SelectedSalary = null;

        ClearSalaryFormError();

    }

    [RelayCommand]

    private async System.Threading.Tasks.Task SaveSalaryFormAsync()

    {
        ClearSalaryFormError();

        if (SalaryEmployeeId <= 0)

        {
            SetSalaryFormError("Chua ch?n nh�n vi�n.");

            return;

        }

        if (SalaryFormSalary < 0)

        {
            SetSalaryFormError("M?c luong ph?i >= 0.");

            return;

        }

        if (SalaryFormUnit < 0)

        {
            SetSalaryFormError("S? don v? ph?i >= 0.");

            return;

        }

        SalaryType type = SalaryTypeValues[System.Math.Clamp(SalaryFormTypeIndex, 0, SalaryTypeValues.Length - 1)];

        EmployeeSalaryDto packet = new()

        {
            EmployeeSalaryId = IsSalaryEditing ? SelectedSalary?.EmployeeSalaryId : null,

            EmployeeId = SalaryEmployeeId,

            Salary = SalaryFormSalary,

            SalaryType = type,

            SalaryUnit = SalaryFormUnit <= 0 ? 1 : SalaryFormUnit,

            EffectiveFrom = SalaryFormEffectiveFrom.Date.ToUniversalTime(),

            EffectiveTo = null,

            Note = SalaryFormNote ?? string.Empty,

            SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()

        };

        EmployeeSalaryWriteResult result = IsSalaryEditing

            ? await _salaryService.UpdateAsync(packet).ConfigureAwait(false)

            : await _salaryService.CreateAsync(packet).ConfigureAwait(false);

        if (!result.IsSuccess)

        {
            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>

                SetSalaryFormError(result.ErrorMessage ?? "Luu th?t b?i."));

            return;

        }

        // Update row display.

        EmployeeSalaryDto? saved = result.Salary;

        if (SalaryEmployee is not null)

        {
            String text = saved is null ? "Chua c�" : FormatSalaryText(saved);

            Boolean has = saved is not null;

            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>

            {
                SalaryEmployee.SalaryText = text;

                SalaryEmployee.HasSalary = has;

                SalaryEmployee.LatestSalaryType = saved?.SalaryType ?? SalaryType.None;

            });

        }

        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>

        {
            IsSalaryFormVisible = false;

            SalaryEmployee = null;

            SelectedSalary = null;

        });

    }

    private async System.Threading.Tasks.Task WarmupSalarySummariesAsync(List<EmployeeRow> rows)

    {
        try { _salaryCts?.Cancel(); } catch { }

        _salaryCts?.Dispose();

        _salaryCts = new System.Threading.CancellationTokenSource();

        var ct = _salaryCts.Token;

        for (int i = 0; i < rows.Count; i++)

        {
            if (ct.IsCancellationRequested)

            {
                return;

            }

            EmployeeRow row = rows[i];

            if (row?.Dto?.EmployeeId is null)

            {
                continue;

            }

            EmployeeSalaryDto? latest = await GetLatestSalaryAsync(row.Dto.EmployeeId.Value, ct).ConfigureAwait(false);

            if (ct.IsCancellationRequested)

            {
                return;

            }

            String text = latest is null ? "Chua c�" : FormatSalaryText(latest);

            Boolean has = latest is not null;

            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>

            {
                row.SalaryText = text;

                row.HasSalary = has;

                row.LatestSalaryType = latest?.SalaryType ?? SalaryType.None;

                if (PickerSalaryIndex != 0)

                {
                    OnPropertyChanged(nameof(FilteredEmployees));

                    OnPropertyChanged(nameof(IsEmpty));

                }

            });

        }

    }

    private async System.Threading.Tasks.Task<EmployeeSalaryDto?> GetLatestSalaryAsync(int employeeId, System.Threading.CancellationToken ct = default)

    {
        EmployeeSalaryListResult result = await _salaryService.GetListAsync(

            page: 1,

            pageSize: 1,

            filterEmployeeId: employeeId,

            searchTerm: null,

            sortBy: EmployeeSalarySortField.EffectiveFrom,

            sortDescending: true,

            filterSalaryType: null,

            filterFromDate: null,

            filterToDate: null,

            ct: ct).ConfigureAwait(false);

        return !result.IsSuccess || result.Salaries.Count == 0 ? null : result.Salaries[0];

    }

    private static string FormatSalaryText(EmployeeSalaryDto dto)

    {
        String typeText = EnumText.Get(dto.SalaryType);

        if (dto.SalaryType == SalaryType.Monthly)

        {
            return $"{dto.Salary:N0} ({typeText})";

        }

        Decimal unit = dto.SalaryUnit <= 0 ? 1 : dto.SalaryUnit;

        Decimal total = dto.Salary * unit;

        return $"{dto.Salary:N0} x {unit:N0} = {total:N0} ({typeText})";

    }

    private IEnumerable<EmployeeRow> ApplySalaryFilter(IEnumerable<EmployeeRow> src)

    {
        return PickerSalaryIndex switch

        {
            1 => src.Where(r => r.HasSalary),

            2 => src.Where(r => !r.HasSalary),

            3 => src.Where(r => r.HasSalary && r.LatestSalaryType == SalaryType.Monthly),

            4 => src.Where(r => r.HasSalary && r.LatestSalaryType == SalaryType.Daily),

            5 => src.Where(r => r.HasSalary && r.LatestSalaryType == SalaryType.Hourly),

            _ => src

        };

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

    private void ClearSalaryFormError()

    {
        HasSalaryFormError = false;

        SalaryFormErrorMessage = null;

    }

    private void SetSalaryFormError(string message)

    {
        SalaryFormErrorMessage = message;

        HasSalaryFormError = true;

    }

    private void ClearForm()

    {
        FormName = string.Empty;

        FormEmail = string.Empty;

        FormAddress = string.Empty;

        FormPhoneNumber = string.Empty;

        FormGenderIndex = 0;

        FormPositionIndex = 0;

        FormStatusIndex = 1;

        FormDateOfBirth = DateTime.Today.AddYears(-20);

        FormStartDate = DateTime.Today;

        FormEndDate = null;

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
        ClearFormError();
        if (!EmployeeValidation.IsValidName(FormName))
        { SetFormError("Tên nhân viên không hợp lệ (2-100 ký tự)."); return false; }

        if (!AccountValidation.IsValidEmail(FormEmail))
        { SetFormError("Email không hợp lệ."); return false; }

        if (!string.IsNullOrWhiteSpace(FormPhoneNumber))
        {
            if (!AccountValidation.IsValidVietnamPhoneNumber(FormPhoneNumber))
            { SetFormError("Số điện thoại không hợp lệ."); return false; }
        }

        if (!EmployeeValidation.IsValidDateOfBirth(FormDateOfBirth))
        { SetFormError("Ngày sinh phải trong quá khứ."); return false; }

        if (!EmployeeValidation.IsValidDates(FormStartDate, FormEndDate))
        { SetFormError("Ngày kết thúc phải sau ngày bắt đầu."); return false; }

        return true;
    }

    private EmployeeDto BuildPacketFromForm()

    {
        return new EmployeeDto

        {
            EmployeeId = IsEditing ? SelectedEmployee?.Dto?.EmployeeId : null,

            Name = FormName,

            Email = FormEmail,

            Address = FormAddress,

            PhoneNumber = FormPhoneNumber,

            Gender = GenderValues[System.Math.Clamp(FormGenderIndex, 0, GenderValues.Length - 1)],

            Position = FormPositionValues[System.Math.Clamp(FormPositionIndex, 0, FormPositionValues.Length - 1)],

            Status = StatusValues[System.Math.Clamp(FormStatusIndex, 0, StatusValues.Length - 1)],

            DateOfBirth = FormDateOfBirth,

            StartDate = FormStartDate,

            EndDate = FormEndDate,

            SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()

        };

    }

    private int IndexOfEmployee(System.Object? id)

    {
        if (id is null)

        {
            return -1;

        }

        for (int i = 0; i < Employees.Count; i++)

        {
            if (Employees[i].Dto.EmployeeId is System.Object employeeId && employeeId.Equals(id))

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

