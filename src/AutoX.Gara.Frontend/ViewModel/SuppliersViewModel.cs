// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Helpers;
using AutoX.Gara.Frontend.Models.Results.Suppliers;
using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System;
using System.Diagnostics;
using System.Linq;



namespace AutoX.Gara.Frontend.Controllers;



/// <summary>



/// ViewModel for supplier management.



/// </summary>



public sealed partial class SuppliersViewModel : ObservableObject, System.IDisposable



{

    private readonly SupplierService _service;



    private System.Threading.CancellationTokenSource? _loadCts;



    private System.Threading.CancellationTokenSource? _writeCts;



    private System.Threading.CancellationTokenSource? _searchCts;



    private const int DefaultPageSize = 20;



    private const int SearchDebounceMs = 400;



    // --- Pagination ---------------------------------------------------------──



    [ObservableProperty] public partial int CurrentPage { get; set; } = 1;



    [ObservableProperty] public partial bool HasNextPage { get; set; }



    [ObservableProperty] public partial bool HasPreviousPage { get; set; }



    [ObservableProperty] public partial int TotalCount { get; set; }



    public int TotalPages => TotalCount > 0



        ? (int)System.Math.Ceiling((double)TotalCount / DefaultPageSize)



        : 0;



    // --- Search / Sort ------------------------------------------------------──



    [ObservableProperty] public partial string SearchTerm { get; set; } = string.Empty;



    [ObservableProperty] public partial SupplierSortField SortBy { get; set; } = SupplierSortField.Name;



    [ObservableProperty] public partial bool SortDescending { get; set; } = false;



    // --- Filter ---------------------------------------------------------------



    [ObservableProperty] public partial SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;



    [ObservableProperty] public partial PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;



    [ObservableProperty] public partial int PickerStatusIndex { get; set; } = 0;



    [ObservableProperty] public partial int PickerPaymentTermsIndex { get; set; } = 0;



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



        new[] { "T?t c? tr?ng th�i" }.Concat(StatusValues.Where(v => v != SupplierStatus.None).Select(EnumText.Get)).ToArray();



    public string[] FilterPaymentTermsOptions { get; } =



        new[] { "T?t c? thanh to�n" }.Concat(PaymentTermsValues.Where(v => v != PaymentTerms.None).Select(EnumText.Get)).ToArray();



    public bool HasActiveFilters



        => FilterStatus != SupplierStatus.None || FilterPaymentTerms != PaymentTerms.None;



    public string SelectedFilterStatusText =>



        FilterStatusOptions[System.Math.Clamp(PickerStatusIndex, 0, FilterStatusOptions.Length - 1)];



    public string SelectedFilterPaymentTermsText =>



        FilterPaymentTermsOptions[System.Math.Clamp(PickerPaymentTermsIndex, 0, FilterPaymentTermsOptions.Length - 1)];



    // --- State ---------------------------------------------------------------─



    [ObservableProperty] public partial bool IsLoading { get; set; }



    [ObservableProperty] public partial bool HasError { get; set; }



    [ObservableProperty] public partial string? ErrorMessage { get; set; }



    public bool IsEmpty => !IsLoading && Suppliers.Count == 0;



    // --- Popup ---------------------------------------------------------------─



    [ObservableProperty] public partial bool IsPopupVisible { get; set; }



    [ObservableProperty] public partial bool IsPopupRetry { get; set; }



    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;



    [ObservableProperty] public partial string PopupButtonText { get; set; } = "OK";



    public bool IsPopupNotRetry => !IsPopupRetry;



    // --- Form ------------------------------------------------------------------



    [ObservableProperty] public partial bool IsFormVisible { get; set; }



    [ObservableProperty] public partial bool IsEditing { get; set; }



    [ObservableProperty] public partial SupplierDto? SelectedSupplier { get; set; }



    public string FormTitle => IsEditing ? "S?a nh� cung c?p" : "Th�m nh� cung c?p";



    public string FormSaveText => IsEditing ? "Luu thay d?i" : "Th�m nh� cung c?p";



    [ObservableProperty] public partial string FormName { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormEmail { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormAddress { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormTaxCode { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormBankAccount { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormPhoneNumbers { get; set; } = string.Empty;



    [ObservableProperty] public partial string FormNotes { get; set; } = string.Empty;



    [ObservableProperty] public partial int FormStatusIndex { get; set; } = 1;



    [ObservableProperty] public partial int FormPaymentTermsIndex { get; set; } = 0;



    [ObservableProperty] public partial DateTime FormContractStartDate { get; set; } = DateTime.Today;



    [ObservableProperty] public partial DateTime? FormContractEndDate { get; set; } = null;



    [ObservableProperty] public partial bool HasFormError { get; set; }



    [ObservableProperty] public partial string? FormErrorMessage { get; set; }



    public string[] FormStatusOptions { get; } =



        StatusValues.Select(EnumText.Get).ToArray();



    public string[] FormPaymentTermsOptions { get; } =



        PaymentTermsValues.Select(EnumText.Get).ToArray();



    public string SelectedFormStatusText =>



        FormStatusOptions[System.Math.Clamp(FormStatusIndex, 0, FormStatusOptions.Length - 1)];



    public string SelectedFormPaymentTermsText =>



        FormPaymentTermsOptions[System.Math.Clamp(FormPaymentTermsIndex, 0, FormPaymentTermsOptions.Length - 1)];



    // --- Status Confirm ------------------------------------------------------─



    [ObservableProperty] public partial bool IsStatusConfirmVisible { get; set; }



    [ObservableProperty] public partial int NewStatusIndex { get; set; } = 1;



    public string StatusConfirmName => SelectedSupplier?.Name ?? string.Empty;



    public string SelectedNewStatusText =>



        FormStatusOptions[System.Math.Clamp(NewStatusIndex, 0, FormStatusOptions.Length - 1)];



    // --- Collection ---------------------------------------------------------──



    public System.Collections.ObjectModel.ObservableCollection<SupplierDto> Suppliers { get; } = [];



    // --- Constructor ---------------------------------------------------------──



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



    // --- Property Hooks ------------------------------------------------------─



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



    // --- Commands ------------------------------------------------------------──



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



        FormName = supplier.Name ?? string.Empty;



        FormEmail = supplier.Email ?? string.Empty;



        FormAddress = supplier.Address ?? string.Empty;



        FormTaxCode = supplier.TaxCode ?? string.Empty;



        FormBankAccount = supplier.BankAccount ?? string.Empty;



        FormPhoneNumbers = supplier.PhoneNumbers ?? string.Empty;



        FormNotes = supplier.Notes ?? string.Empty;



        FormStatusIndex = System.Array.IndexOf(StatusValues, supplier.Status ?? SupplierStatus.None);



        if (FormStatusIndex < 0)
        {
            FormStatusIndex = 0;
        }

        FormPaymentTermsIndex = System.Array.IndexOf(PaymentTermsValues, supplier.PaymentTerms ?? PaymentTerms.None);



        if (FormPaymentTermsIndex < 0)
        {
            FormPaymentTermsIndex = 0;
        }

        FormContractStartDate = supplier.ContractStartDate ?? DateTime.Today;



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

                        int idx = IndexOfSupplier(result.UpdatedEntity.SupplierId);



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

                SetFormError(result.ErrorMessage ?? "Thao t�c th?t b?i.");



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



                SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()



            };



            SupplierWriteResult result = await _service.ChangeStatusAsync(data, ct);



            if (result.IsSuccess)



            {

                int idx = IndexOfSupplier(SelectedSupplier.SupplierId);



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



    }



    // --- Private Helpers ------------------------------------------------------─



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

        FormName = string.Empty;



        FormEmail = string.Empty;



        FormAddress = string.Empty;



        FormTaxCode = string.Empty;



        FormBankAccount = string.Empty;



        FormPhoneNumbers = string.Empty;



        FormNotes = string.Empty;



        FormStatusIndex = 1;



        FormPaymentTermsIndex = 0;



        FormContractStartDate = DateTime.Today;



        FormContractEndDate = null;



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

        if (string.IsNullOrWhiteSpace(FormName))



        { SetFormError("T�n nh� cung c?p kh�ng được d? tr?ng."); return false; }



        if (FormName.Length > 150)



        { SetFormError("T�n kh�ng được vượt qu� 150 k� t?."); return false; }



        if (!IsValidEmail(FormEmail))



        { SetFormError("Email kh�ng h?p l?."); return false; }



        if (string.IsNullOrWhiteSpace(FormTaxCode))



        { SetFormError("M� s? thu? kh�ng được d? tr?ng."); return false; }



        if (FormPhoneNumbers.Length > 0)



        {

            foreach (var phone in FormPhoneNumbers.Split(','))



            {

                if (!IsValidPhoneNumber(phone.Trim()))



                { SetFormError("S? di?n tho?i kh�ng h?p l?."); return false; }



            }



        }



        if (FormContractEndDate.HasValue && FormContractEndDate <= FormContractStartDate)



        { SetFormError("Ng�y k?t th�c ph?i sau ng�y b?t d?u."); return false; }



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



            SequenceId = (ushort)Nalix.Framework.Random.Csprng.NextUInt32()



        };



    }



    private int IndexOfSupplier(System.Object? id)



    {

        if (id is null)



        {

            return -1;



        }



        for (int i = 0; i < Suppliers.Count; i++)



        {

            if (Suppliers[i].SupplierId is System.Object supplierId && supplierId.Equals(id))



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



    private static bool IsValidEmail(string email)



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



    private static bool IsValidPhoneNumber(string phone) => System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+?\d{1,3}[-.\s]?)?\d{10,15}$");

}


