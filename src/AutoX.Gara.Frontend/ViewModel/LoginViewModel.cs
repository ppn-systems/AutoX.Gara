// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Tasks;
using Nalix.SDK.Transport;
using Nalix.Framework.DataFrames.SignalFrames;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.UI.ViewModels;

/// <summary>
/// ViewModel quản lý luồng nghiệp vụ cho màn hình Đăng nhập.
/// Áp dụng Pattern MVVM chuẩn công nghiệp, SRP và Clean Code.
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAccountService _loginService;
    private readonly INavigationService _navigation;
    private CancellationTokenSource? _loginCts;

    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _isNetworkReady;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isPasswordHidden = true;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    // -- Popup State --
    [ObservableProperty] private bool _isPopupRetry;
    [ObservableProperty] private bool _isPopupVisible;
    [ObservableProperty] private string _popupButtonText = "OK";
    [ObservableProperty] private string _popupTitle = string.Empty;
    [ObservableProperty] private string _popupMessage = string.Empty;

    // -- Computed Properties --
    public bool IsPopupNotRetry => !IsPopupRetry;
    public bool IsNetworkNotReady => !IsNetworkReady;
    public string PasswordIcon => IsPasswordHidden ? "eye_off.png" : "eye.png";

    /// <summary>
    /// Khởi tạo ViewModel với các dependencies được inject từ MauiProgram.
    /// </summary>
    public LoginViewModel(IAccountService loginService, INavigationService navigation)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

        // Tự động khởi tạo kết nối khi ViewModel được tạo
        _ = InitConnectionAsync();
    }

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnIsNetworkReadyChanged(bool value) => OnPropertyChanged(nameof(IsNetworkNotReady));

    /// <summary>
    /// Xử lý lệnh đăng nhập. Bao gồm validate, auth và chuyển trang.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        // Tránh race-condition: Hủy tiến trình login cũ nếu đang chạy
        _loginCts?.Cancel();
        _loginCts = new CancellationTokenSource();
        var ct = _loginCts.Token;

        ClearError();

        if (!ValidateInputs()) return;

        IsLoading = true;
        try
        {
            var result = await _loginService.AuthenticateAsync(Username, Password, ct);
            if (result.IsSuccess)
            {
                SetupKeepAlive();
                await _navigation.GoToMainPageAsync();
                return;
            }

            HandleFailedLogin(result);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Thiết lập định kỳ gửi Heartbeat để duy trì phiên làm việc Nalix.
    /// </summary>
    private void SetupKeepAlive()
    {
        var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
        if (client == null) return;

        var taskManager = InstanceManager.Instance.GetOrCreateInstance<TaskManager>();
        taskManager.ScheduleRecurring(
            "KeepAlive", 
            TimeSpan.FromSeconds(30),
            async (token) => await client.SendAsync(new Directive { Type = ControlType.HEARTBEAT }, token)
        );
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        OnPropertyChanged(nameof(PasswordIcon));
    }

    [RelayCommand]
    private void ClosePopup() => IsPopupVisible = false;

    [RelayCommand]
    private void RetryConnection()
    {
        IsPopupVisible = false;
        _ = InitConnectionAsync();
    }

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetError("Tên đăng nhập không được để trống.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError("Mật khẩu không được để trống.");
            return false;
        }

        if (!AccountValidation.IsValidUsername(Username))
        {
            SetError("Tên đăng nhập không hợp lệ (5-50 ký tự).");
            return false;
        }

        if (!AccountValidation.IsValidPassword(Password))
        {
            SetError("Mật khẩu không đủ mạnh (tối thiểu 8 ký tự, bao gồm chữ hoa/thường/số/đặc biệt).");
            return false;
        }

        return true;
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void HandleFailedLogin(LoginResult result)
    {
        switch (result.Advice)
        {
            case ProtocolAdvice.DO_NOT_RETRY:
                ShowPopup("Lỗi nghiêm trọng", result.ErrorMessage ?? "Máy chủ từ chối đăng nhập.", false);
                break;
            case ProtocolAdvice.BACKOFF_RETRY:
                ShowPopup("Tài khoản bị khóa", result.ErrorMessage ?? "Thử lại sau vài phút.", true);
                break;
            default:
                SetError(result.ErrorMessage ?? "Thông tin đăng nhập không chính xác.");
                break;
        }
    }

    private void ShowPopup(string title, string message, bool isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? "Thử lại" : "Đã hiểu";
        IsPopupVisible = true;
    }

    private async Task InitConnectionAsync()
    {
        IsLoading = true;
        IsNetworkReady = false;

        var result = await _loginService.ConnectAsync();
        
        IsLoading = false;
        if (result.IsSuccess)
        {
            IsNetworkReady = true;
        }
        else
        {
            ShowPopup("Lỗi máy chủ", result.ErrorMessage ?? "Không thể kết nối tới hạ tầng Nalix.", true);
        }
    }
}