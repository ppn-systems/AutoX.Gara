// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Frontend.Services.Billings;
using AutoX.Gara.Frontend.Services.Employees;
using AutoX.Gara.Frontend.Services.Invoices;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Employees;
using AutoX.Gara.Shared.Protocol.Invoices;
using AutoX.Gara.Shared.Protocol.Repairs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;



namespace AutoX.Gara.Frontend.Controllers.Billings;



public sealed partial class RepairTasksViewModel : ObservableObject, System.IDisposable



{

    private readonly RepairTaskService _service;



    private readonly EmployeeService _employeeService;



    private readonly ServiceItemService _serviceItemService;



    private System.Threading.CancellationTokenSource? _cts;



    private System.Threading.CancellationTokenSource? _lookupCts;



    private readonly Dictionary<int, string> _serviceDescById = [];



    private readonly Dictionary<int, string> _employeeNameById = [];



    private List<RepairTaskDto> _rawTasks = [];



    private const int DefaultPageSize = 10;



    public RepairTasksViewModel(



        RepairTaskService service,



        EmployeeService employeeService,



        ServiceItemService serviceItemService)



    {

        _service = service ?? throw new System.ArgumentNullException(nameof(service));



        _employeeService = employeeService ?? throw new System.ArgumentNullException(nameof(employeeService));



        _serviceItemService = serviceItemService ?? throw new System.ArgumentNullException(nameof(serviceItemService));



    }



    public sealed record LookupOption(int Id, string Display);



    public sealed class RepairTaskRow



    {

        public required RepairTaskDto Task { get; init; }



        public string EmployeeName { get; init; } = string.Empty;



        public string ServiceDescription { get; init; } = string.Empty;



    }



    [ObservableProperty] public partial RepairOrderDto? RepairOrder { get; set; }



    public string PageTitle => RepairOrder?.RepairOrderId is null



        ? "Task s?a ch?a"



        : $"Task l?nh #{RepairOrder.RepairOrderId}";



    public ObservableCollection<RepairTaskRow> RepairTaskRows { get; } = [];



    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;



    [ObservableProperty] public partial bool HasNextPage { get; set; }



    [ObservableProperty] public partial bool HasPreviousPage { get; set; }



    [ObservableProperty] public partial int TotalCount { get; set; }



    public int TotalPages => TotalCount > 0



        ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize)



        : 0;



    [ObservableProperty] public partial bool IsLoading { get; set; }



    [ObservableProperty] public partial bool HasError { get; set; }



    [ObservableProperty] public partial string? ErrorMessage { get; set; }



    [ObservableProperty] public partial bool IsPopupVisible { get; set; }



    [ObservableProperty] public partial bool IsPopupRetry { get; set; }



    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupButtonText { get; set; } = "OK";



    public bool IsPopupNotRetry => !IsPopupRetry;



    // Lookups (for pickers)



    public ObservableCollection<LookupOption> EmployeeOptions { get; } = [];



    public ObservableCollection<LookupOption> ServiceItemOptions { get; } = [];



    [ObservableProperty] public partial LookupOption? SelectedEmployeeOption { get; set; }



    [ObservableProperty] public partial LookupOption? SelectedServiceItemOption { get; set; }



    // Selector overlays (filterable pickers)



    [ObservableProperty] public partial bool IsEmployeeSelectorVisible { get; set; }



    [ObservableProperty] public partial bool IsServiceItemSelectorVisible { get; set; }



    public ObservableCollection<LookupOption> FilteredEmployeeOptions { get; } = [];



    public ObservableCollection<LookupOption> FilteredServiceItemOptions { get; } = [];



    [ObservableProperty] public partial string EmployeeSearchTerm { get; set; } = string.Empty;



    [ObservableProperty] public partial string ServiceItemSearchTerm { get; set; } = string.Empty;



    // Form



    [ObservableProperty] public partial bool IsFormVisible { get; set; }



    [ObservableProperty] public partial bool IsEditing { get; set; }



    [ObservableProperty] public partial RepairTaskDto? SelectedRepairTask { get; set; }



    public string FormTitle => IsEditing ? "S?a task" : "Th�m task";



    [ObservableProperty] public partial int FormEmployeeId { get; set; }



    [ObservableProperty] public partial int FormServiceItemId { get; set; }



    [ObservableProperty] public partial double FormEstimatedDuration { get; set; } = 1.0;



    [ObservableProperty] public partial int PickerStatusIndex { get; set; } = (int)RepairOrderStatus.Pending;



    public string[] StatusOptions { get; } = EnumText.GetNames<RepairOrderStatus>();



    public string SelectedStatusText =>



        StatusOptions[System.Math.Clamp(PickerStatusIndex, 0, StatusOptions.Length - 1)];



    partial void OnPickerStatusIndexChanged(int value) => OnPropertyChanged(nameof(SelectedStatusText));



    public RepairOrderStatus FormStatus



    {

        get



        {

            RepairOrderStatus[] values = System.Enum.GetValues<RepairOrderStatus>();



            return PickerStatusIndex < 0 || PickerStatusIndex >= values.Length ? RepairOrderStatus.Pending : values[PickerStatusIndex];



        }



    }



    [ObservableProperty] public partial bool HasStartDate { get; set; }



    [ObservableProperty] public partial DateTime FormStartDateValue { get; set; } = DateTime.Today;



    [ObservableProperty] public partial bool HasCompletionDate { get; set; }



    [ObservableProperty] public partial DateTime FormCompletionDateValue { get; set; } = DateTime.Today;



    // Delete confirm



    [ObservableProperty] public partial bool IsDeleteConfirmVisible { get; set; }



    public void Initialize(RepairOrderDto repairOrder)



    {

        RepairOrder = repairOrder;



        OnPropertyChanged(nameof(PageTitle));



        _ = LoadLookupsAsync();



        _ = LoadAsync();



    }



    partial void OnSelectedEmployeeOptionChanged(LookupOption? value)



        => FormEmployeeId = value?.Id ?? 0;



    partial void OnSelectedServiceItemOptionChanged(LookupOption? value)



        => FormServiceItemId = value?.Id ?? 0;



    private void SyncSelectedOptionsFromIds()



    {

        if (FormEmployeeId > 0 && (SelectedEmployeeOption?.Id ?? 0) != FormEmployeeId)



        {

            SelectedEmployeeOption = EmployeeOptions.FirstOrDefault(x => x.Id == FormEmployeeId);



        }



        if (FormServiceItemId > 0 && (SelectedServiceItemOption?.Id ?? 0) != FormServiceItemId)



        {

            SelectedServiceItemOption = ServiceItemOptions.FirstOrDefault(x => x.Id == FormServiceItemId);



        }



    }



    private void RefreshRowDisplays()



    {

        RepairTaskRows.Clear();



        for (int i = 0; i < _rawTasks.Count; i++)



        {

            RepairTaskDto t = _rawTasks[i];



            _employeeNameById.TryGetValue(t.EmployeeId, out string? empName);



            _serviceDescById.TryGetValue(t.ServiceItemId, out string? svcDesc);



            RepairTaskRows.Add(new RepairTaskRow



            {

                Task = t,



                EmployeeName = empName ?? $"#{t.EmployeeId}",



                ServiceDescription = svcDesc ?? $"#{t.ServiceItemId}"



            });



        }



    }



    private static bool ContainsIgnoreCase(string haystack, string needle)



        => haystack?.IndexOf(needle ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;



    private void RefreshEmployeeFilter()



    {

        FilteredEmployeeOptions.Clear();



        string term = EmployeeSearchTerm?.Trim() ?? string.Empty;



        for (int i = 0; i < EmployeeOptions.Count; i++)



        {

            var opt = EmployeeOptions[i];



            if (term.Length == 0 || ContainsIgnoreCase(opt.Display, term))



            {

                FilteredEmployeeOptions.Add(opt);



            }



        }



    }



    private void RefreshServiceItemFilter()



    {

        FilteredServiceItemOptions.Clear();



        string term = ServiceItemSearchTerm?.Trim() ?? string.Empty;



        for (int i = 0; i < ServiceItemOptions.Count; i++)



        {

            var opt = ServiceItemOptions[i];



            if (term.Length == 0 || ContainsIgnoreCase(opt.Display, term))



            {

                FilteredServiceItemOptions.Add(opt);



            }



        }



    }



    partial void OnEmployeeSearchTermChanged(string value) => RefreshEmployeeFilter();



    partial void OnServiceItemSearchTermChanged(string value) => RefreshServiceItemFilter();



    [RelayCommand]



    private void OpenEmployeeSelector()



    {

        EmployeeSearchTerm = string.Empty;



        RefreshEmployeeFilter();



        IsEmployeeSelectorVisible = true;



    }



    [RelayCommand]



    private void OpenServiceItemSelector()



    {

        ServiceItemSearchTerm = string.Empty;



        RefreshServiceItemFilter();



        IsServiceItemSelectorVisible = true;



    }



    [RelayCommand]



    private void CloseSelectors()



    {

        IsEmployeeSelectorVisible = false;



        IsServiceItemSelectorVisible = false;



    }



    [RelayCommand]



    private void SelectEmployee(LookupOption opt)



    {

        SelectedEmployeeOption = opt;



        IsEmployeeSelectorVisible = false;



    }



    [RelayCommand]



    private void SelectServiceItem(LookupOption opt)



    {

        SelectedServiceItemOption = opt;



        IsServiceItemSelectorVisible = false;



    }



    [RelayCommand]



    public async System.Threading.Tasks.Task LoadLookupsAsync()



    {

        _lookupCts?.Cancel();



        _lookupCts?.Dispose();



        _lookupCts = new System.Threading.CancellationTokenSource();



        var ct = _lookupCts.Token;



        try



        {

            var empResult = await _employeeService.GetListAsync(



                page: 1,



                pageSize: 200,



                ct: ct);



            if (empResult.IsSuccess)



            {

                EmployeeOptions.Clear();



                _employeeNameById.Clear();



                for (int i = 0; i < empResult.Employees.Count; i++)



                {

                    EmployeeDto e = empResult.Employees[i];



                    if (e.EmployeeId > 0)



                    {

                        string display = $"#{e.EmployeeId.Value} - {e.Name}";



                        EmployeeOptions.Add(new LookupOption(e.EmployeeId.Value, display));



                        _employeeNameById[e.EmployeeId.Value] = display;



                    }



                }



            }



            var siResult = await _serviceItemService.GetListAsync(



                page: 1,



                pageSize: 200,



                ct: ct);



            if (siResult.IsSuccess)



            {

                ServiceItemOptions.Clear();



                _serviceDescById.Clear();



                for (int i = 0; i < siResult.ServiceItems.Count; i++)



                {

                    ServiceItemDto s = siResult.ServiceItems[i];



                    if (s.ServiceItemId > 0)



                    {

                        string display = $"#{s.ServiceItemId.Value} - {EnumText.Get<ServiceType>(s.Type)} ({s.UnitPrice:n0})";



                        ServiceItemOptions.Add(new LookupOption(s.ServiceItemId.Value, display));



                        _serviceDescById[s.ServiceItemId.Value] = display;



                    }



                }



            }



            SyncSelectedOptionsFromIds();



            RefreshRowDisplays();



        }



        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)



        {

        }



    }



    [RelayCommand]



    public async System.Threading.Tasks.Task LoadAsync()



    {

        if (RepairOrder?.RepairOrderId is null)



        {

            return;



        }



        _cts?.Cancel();



        _cts?.Dispose();



        _cts = new System.Threading.CancellationTokenSource();



        var ct = _cts.Token;



        IsLoading = true;



        ClearError();



        try



        {

            RepairTaskListResult result = await _service.GetListAsync(



                page: CurrentPage,



                pageSize: DefaultPageSize,



                filterRepairOrderId: RepairOrder.RepairOrderId.Value,



                ct: ct);



            if (!result.IsSuccess)



            {

                HandleError("Kh�ng t?i được task", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);



                return;



            }



            _rawTasks = result.RepairTasks ?? [];



            RefreshRowDisplays();



            TotalCount = result.TotalCount;



            HasPreviousPage = CurrentPage > 1;



            HasNextPage = CurrentPage < TotalPages;



        }



        catch (System.OperationCanceledException) when (ct.IsCancellationRequested)



        {

        }



        finally



        {

            IsLoading = false;



        }



    }



    partial void OnCurrentPageChanged(int value) => _ = LoadAsync();



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



    private void OpenAddForm()



    {

        IsEditing = false;



        SelectedRepairTask = null;



        FormEmployeeId = 0;



        FormServiceItemId = 0;



        SelectedEmployeeOption = null;



        SelectedServiceItemOption = null;



        FormEstimatedDuration = 1.0;



        PickerStatusIndex = (int)RepairOrderStatus.Pending;



        HasStartDate = false;



        HasCompletionDate = false;



        FormStartDateValue = DateTime.Today;



        FormCompletionDateValue = DateTime.Today;



        IsFormVisible = true;



    }



    [RelayCommand]



    private void OpenEditForm(RepairTaskDto t)



    {

        IsEditing = true;



        SelectedRepairTask = t;



        FormEmployeeId = t.EmployeeId;



        FormServiceItemId = t.ServiceItemId;



        SyncSelectedOptionsFromIds();



        FormEstimatedDuration = t.EstimatedDuration;



        PickerStatusIndex = (int)t.Status;



        HasStartDate = t.StartDate.HasValue;



        FormStartDateValue = t.StartDate?.ToLocalTime().Date ?? DateTime.Today;



        HasCompletionDate = t.CompletionDate.HasValue;



        FormCompletionDateValue = t.CompletionDate?.ToLocalTime().Date ?? DateTime.Today;



        IsFormVisible = true;



    }



    [RelayCommand] private void CancelForm() => IsFormVisible = false;



    [RelayCommand]



    private async System.Threading.Tasks.Task SaveAsync()



    {

        if (RepairOrder?.RepairOrderId is null)



        {

            return;



        }



        if (SelectedEmployeeOption is null || SelectedServiceItemOption is null || FormEmployeeId <= 0 || FormServiceItemId <= 0)



        {

            HasError = true;



            ErrorMessage = "Vui l�ng ch?n Nh�n vi�n v� D?ch v?.";



            return;



        }



        IsLoading = true;



        ClearError();



        try



        {

            RepairTaskDto packet = new()



            {

                RepairTaskId = IsEditing ? SelectedRepairTask?.RepairTaskId : null,



                RepairOrderId = RepairOrder.RepairOrderId.Value,



                EmployeeId = FormEmployeeId,



                ServiceItemId = FormServiceItemId,



                Status = FormStatus,



                StartDate = HasStartDate ? FormStartDateValue.ToUniversalTime() : null,



                EstimatedDuration = FormEstimatedDuration,



                CompletionDate = HasCompletionDate ? FormCompletionDateValue.ToUniversalTime() : null,



            };



            RepairTaskWriteResult result = IsEditing



                ? await _service.UpdateAsync(packet)



                : await _service.CreateAsync(packet);



            if (!result.IsSuccess)



            {

                HandleError("Luu th?t b?i", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);



                return;



            }



            IsFormVisible = false;



            await LoadAsync();



        }



        finally



        {

            IsLoading = false;



        }



    }



    [RelayCommand]



    private void RequestDelete(RepairTaskDto t)



    {

        SelectedRepairTask = t;



        IsDeleteConfirmVisible = true;



    }



    [RelayCommand] private void CancelDelete() => IsDeleteConfirmVisible = false;



    [RelayCommand]



    private async System.Threading.Tasks.Task ConfirmDeleteAsync()



    {

        if (SelectedRepairTask?.RepairTaskId is null)



        {

            return;



        }



        IsDeleteConfirmVisible = false;



        IsLoading = true;



        ClearError();



        try



        {

            RepairTaskWriteResult result = await _service.DeleteAsync(SelectedRepairTask);



            if (!result.IsSuccess)



            {

                HandleError("X�a th?t b?i", result.ErrorMessage ?? "Thao t�c th?t b?i.", result.Advice);



                return;



            }



            await LoadAsync();



        }



        finally



        {

            IsLoading = false;



        }



    }



    [RelayCommand] private void ClosePopup() => IsPopupVisible = false;



    [RelayCommand]



    private void RetryLoad()



    {

        IsPopupVisible = false;



        _ = LoadAsync();



    }



    public void Dispose()



    {

        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);



        var lookupCts = System.Threading.Interlocked.Exchange(ref _lookupCts, null);



        if (lookupCts is not null)



        {

            try



            {

                lookupCts.Cancel();



            }



            catch (System.ObjectDisposedException)



            {

            }



            lookupCts.Dispose();



        }



        if (cts is null)



        {

            return;



        }



        try



        {

            cts.Cancel();



        }



        catch (System.ObjectDisposedException)



        {

        }



        cts.Dispose();



    }



    private void ClearError() { HasError = false; ErrorMessage = null; }



    private void HandleError(string title, string message, ProtocolAdvice advice)



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



    private void ShowPopup(string title, string message, bool isRetry)



    {

        PopupTitle = title;



        PopupMessage = message;



        IsPopupRetry = isRetry;



        PopupButtonText = isRetry ? "Th? l?i" : "OK";



        IsPopupVisible = true;



    }

}

