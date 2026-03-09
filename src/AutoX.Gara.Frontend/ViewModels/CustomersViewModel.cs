// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Packets.Customers;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.ViewModels;

/// <summary>
/// ViewModel for the Customers management screen.
/// <para>
/// Single Responsibility:
///   - Manages UI state (IsLoading, HasError, Popup, Form...)
///   - Orchestrates: validate → call service → update list → notify UI
/// </para>
/// <para>Does NOT contain: network code, navigation code, validation regex.</para>
/// </summary>
public sealed partial class CustomersViewModel : ObservableObject
{
    // ─── Dependencies ─────────────────────────────────────────────────────────

    private readonly ICustomerService _customerService;

    // ─── Cancellation ─────────────────────────────────────────────────────────

    /// <summary>Cancellation token for the active network request.</summary>
    private System.Threading.CancellationTokenSource? _cts;

    // ─── Pagination ───────────────────────────────────────────────────────────

    private const System.Int32 DefaultPageSize = 20;

    [ObservableProperty] public partial System.Int32 CurrentPage { get; set; } = 1;
    [ObservableProperty] public partial System.Boolean HasNextPage { get; set; }
    [ObservableProperty] public partial System.Boolean HasPreviousPage { get; set; }

    // ─── State ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsLoading { get; set; }
    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }

    // ─── Popup ────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;

    // ─── Form (Create / Edit) ─────────────────────────────────────────────────

    /// <summary>Indicates whether the create/edit form panel is visible.</summary>
    [ObservableProperty] public partial System.Boolean IsFormVisible { get; set; }

    /// <summary>True when editing an existing customer, false when creating a new one.</summary>
    [ObservableProperty] public partial System.Boolean IsEditing { get; set; }

    /// <summary>Currently selected customer for editing or deletion.</summary>
    [ObservableProperty] public partial CustomerDataPacket? SelectedCustomer { get; set; }

    // ─── Form Fields ──────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.String FormName { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormEmail { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormPhone { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormAddress { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String FormTaxCode { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.Boolean HasFormError { get; set; }
    [ObservableProperty] public partial System.String? FormErrorMessage { get; set; }

    // ─── Delete Confirmation Popup ────────────────────────────────────────────

    /// <summary>Indicates whether the delete confirmation popup is visible.</summary>
    [ObservableProperty] public partial System.Boolean IsDeleteConfirmVisible { get; set; }

    // ─── Customer List ────────────────────────────────────────────────────────

    /// <summary>Observable list of customers shown in the UI.</summary>
    public System.Collections.ObjectModel.ObservableCollection<CustomerDataPacket> Customers { get; } = [];

    // ─── Constructor ─────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes the ViewModel with a customer service via dependency injection.
    /// </summary>
    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        _ = LoadAsync();
    }

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));

    partial void OnCurrentPageChanged(int value)
    {
        HasPreviousPage = value > 1;
        _ = LoadAsync();
    }

    // ─── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Loads the current page of customers from the server.</summary>
    [RelayCommand]
    private async System.Threading.Tasks.Task LoadAsync()
    {
        _cts?.Cancel();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        ClearError();
        IsLoading = true;

        try
        {
            CustomerListResult result = await _customerService.GetListAsync(CurrentPage, DefaultPageSize, ct);

            Debug.WriteLine($"[VM DEBUG] LoadAsync result: IsSuccess={result.IsSuccess}, Customers={result.Customers.Count}");

            if (result.IsSuccess)
            {
                Customers.Clear();
                foreach (CustomerDataPacket c in result.Customers)
                {
                    Customers.Add(c);
                }

                // Show next page button only when a full page was returned
                HasNextPage = result.Customers.Count == DefaultPageSize;
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

    /// <summary>Opens the create-customer form with empty fields.</summary>
    [RelayCommand]
    private void OpenCreateForm()
    {
        IsEditing = false;
        SelectedCustomer = null;
        ClearForm();
        IsFormVisible = true;
    }

    /// <summary>Opens the edit form pre-filled with the selected customer's data.</summary>
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
        ClearFormError();
        IsFormVisible = true;
    }

    /// <summary>Closes the create/edit form without saving.</summary>
    [RelayCommand]
    private void CloseForm()
    {
        IsFormVisible = false;
        ClearForm();
    }

    /// <summary>Saves the current form — creates or updates based on <see cref="IsEditing"/>.</summary>
    [RelayCommand]
    private async System.Threading.Tasks.Task SaveFormAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        _cts?.Cancel();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsLoading = true;

        try
        {
            CustomerDataPacket data = BuildPacketFromForm();
            CustomerWriteResult result = IsEditing ? await _customerService.UpdateAsync(data, ct) : await _customerService.CreateAsync(data, ct);

            if (result.IsSuccess)
            {
                IsFormVisible = false;
                ClearForm();
                // Reload to reflect server-side changes (id, timestamps, etc.)
                await LoadAsync();
            }
            else
            {
                SetFormError(MapWriteErrorToMessage(result.ErrorMessage!, result.Advice));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Opens the delete confirmation popup for the given customer.</summary>
    [RelayCommand]
    private void RequestDelete(CustomerDataPacket customer)
    {
        SelectedCustomer = customer;
        IsDeleteConfirmVisible = true;
    }

    /// <summary>Cancels the pending delete operation.</summary>
    [RelayCommand]
    private void CancelDelete() => IsDeleteConfirmVisible = false;

    /// <summary>Confirms and executes the delete operation.</summary>
    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeleteAsync()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        _cts?.Cancel();
        _cts = new System.Threading.CancellationTokenSource();
        var ct = _cts.Token;

        IsDeleteConfirmVisible = false;
        IsLoading = true;

        try
        {
            CustomerWriteResult result = await _customerService.DeleteAsync(SelectedCustomer, ct);

            if (result.IsSuccess)
            {
                Customers.Remove(SelectedCustomer);
                SelectedCustomer = null;
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

    /// <summary>Navigates to the next page.</summary>
    [RelayCommand]
    private void NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
        }
    }

    /// <summary>Navigates to the previous page.</summary>
    [RelayCommand]
    private void PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
        }
    }

    /// <summary>Closes the error/info popup.</summary>
    [RelayCommand]
    private void ClosePopup() => IsPopupVisible = false;

    /// <summary>Retries the last list load and dismisses the popup.</summary>
    [RelayCommand]
    private void RetryLoad()
    {
        IsPopupVisible = false;
        _ = LoadAsync();
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    private void ClearForm()
    {
        FormName = FormEmail = FormPhone = FormAddress = FormTaxCode = System.String.Empty;
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

    /// <summary>Client-side form validation before sending to the server.</summary>
    private System.Boolean ValidateForm()
    {
        if (System.String.IsNullOrWhiteSpace(FormName))
        {
            SetFormError("Tên khách hàng không được để trống.");
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

        return true;
    }

    /// <summary>Builds a <see cref="CustomerDataPacket"/> from the current form field values.</summary>
    private CustomerDataPacket BuildPacketFromForm()
    {
        CustomerDataPacket data = new()
        {
            Name = FormName,
            Email = FormEmail,
            PhoneNumber = FormPhone,
            Address = FormAddress,
            TaxCode = FormTaxCode,
            UpdatedAt = System.DateTime.UtcNow
        };

        if (IsEditing && SelectedCustomer is not null)
        {
            // Preserve original ID, type, membership, DOB when editing
            data.Type = SelectedCustomer.Type;
            data.CreatedAt = SelectedCustomer.CreatedAt;
            data.CustomerId = SelectedCustomer.CustomerId;
            data.Membership = SelectedCustomer.Membership;
            data.DateOfBirth = SelectedCustomer.DateOfBirth;
        }
        else
        {
            data.CreatedAt = System.DateTime.UtcNow;
        }

        return data;
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
                // Inline error for fixable issues
                HasError = true;
                ErrorMessage = message;
                break;
        }
    }

    private static System.String MapWriteErrorToMessage(System.String serverMessage, ProtocolAdvice advice)
        => advice == ProtocolAdvice.FIX_AND_RETRY ? serverMessage : serverMessage;

    private void ShowPopup(System.String title, System.String message, System.Boolean isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? "Retry" : "OK";
        IsPopupVisible = true;
    }
}