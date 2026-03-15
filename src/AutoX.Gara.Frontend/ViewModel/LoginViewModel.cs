// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Frontend.Results.Accounts;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Tasks;
using Nalix.SDK.Transport;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.UI.ViewModels;

/// <summary>
/// <para>ViewModel cho màn hình dang nh?p.</para>
/// <para>
/// Trách nhi?m duy nh?t (SRP):
///   - Qu?n lý tr?ng thái UI (IsLoading, HasError, Popup...)
///   - Ði?u phụi lu?ng: validate ? connect ? login ? navigate
/// </para>
/// <para>Không ch?a: network code, navigation code, validation rules.</para>
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    // --- Dependencies (DI) ---------------------------------------------------

    private readonly IAccountService _loginService;
    private readonly INavigationService _navigation;

    // --- Cancellation --------------------------------------------------------

    /// <summary>
    /// Token d? Hủy login dang cHủy khi user b?m nút khác / thoát màn hình.
    /// </summary>
    private System.Threading.CancellationTokenSource? _loginCts;

    // --- Observable Properties -----------------------------------------------

    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.Boolean IsNetworkReady { get; set; }
    [ObservableProperty] public partial System.Boolean IsLoading { get; set; } = true;
    [ObservableProperty] public partial System.Boolean IsPasswordHidden { get; set; } = true;

    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }
    [ObservableProperty] public partial System.String Username { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String Password { get; set; } = System.String.Empty;

    // -- Popup -----------------------------------------------------------------

    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;


    // -- Computed --------------------------------------------------------------

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;
    public System.Boolean IsNetworkNotReady => !IsNetworkReady;
    public System.String PasswordIcon => IsPasswordHidden ? "eye_off.png" : "eye.png";


    // --- Constructor ---------------------------------------------------------

    /// <summary>
    /// Constructor nhận dependencies qua DI — dễ unit test hơn <c>InstanceManager</c>.
    /// Nếu chưa dùng DI container, bạn có thể dùng constructor mặc định bên dưới.
    /// </summary>
    public LoginViewModel(IAccountService loginService, INavigationService navigation)
    {
        _loginService = loginService;
        _navigation = navigation;

        _ = InitConnectionAsync();
    }

    // --- Property Change Hooks ------------------------------------------------

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));

    partial void OnIsNetworkReadyChanged(bool value) => OnPropertyChanged(nameof(IsNetworkNotReady));

    [RelayCommand]
    private async System.Threading.Tasks.Task LoginAsync()
    {
        // Hủy login tru?c dó n?u dang cHủy (ví d? user b?m nhanh 2 l?n)
        _loginCts?.Cancel();
        _loginCts = new System.Threading.CancellationTokenSource();
        var ct = _loginCts.Token;

        ClearError();

        // -- Validate input tru?c khi g?i network ------------------------------
        if (!ValidateInputs())
        {
            return;
        }

        IsLoading = true;
        try
        {
            LoginResult result = await _loginService.AuthenticateAsync(Username, Password, ct);

            if (result.IsSuccess)
            {
                ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
                InstanceManager.Instance.GetOrCreateInstance<TaskManager>().ScheduleRecurring(
                    "KeepAlive", System.TimeSpan.FromSeconds(30),
                    async (ct) => await client.SendAsync(new Directive(), ct)
                );

                await _navigation.GoToMainPageAsync();
                return;
            }

            HandleFailedLogin(result);
        }
        finally
        {
            // Ð?m b?o IsLoading luôn du?c reset k? c? khi exception
            IsLoading = false;
        }
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

    // --- Private Helpers -----------------------------------------------------

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Validate username + password client-side, hi?n l?i ngay không c?n g?i network.
    /// </summary>
    private System.Boolean ValidateInputs()
    {
        if (System.String.IsNullOrWhiteSpace(Username))
        {
            SetError("Tên đăng nhập không được để trống.");
            return false;
        }

        if (System.String.IsNullOrWhiteSpace(Password))
        {
            SetError("Mật khẩu không được để trống.");
            return false;
        }

        if (!AccountValidation.IsValidUsername(Username))
        {
            SetError("Tên đăng nhập không hợp lệ: chỉ cho phép chữ cái, số, '_', '-' và tối đa 50 ký tự.");
            return false;
        }

        if (!AccountValidation.IsValidPassword(Password))
        {
            SetError("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, thường, số và ký tự đặc biệt.");
            return false;
        }

        return true;
    }

    private void SetError(System.String message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    /// <summary>
    /// Xử lý phản hồi từ server theo <see cref="ProtocolAdvice"/>:
    /// - DO_NOT_RETRY  -> khóa nút đăng nhập
    /// - BACKOFF_RETRY -> hiện popup có nút retry
    /// - FIX_AND_RETRY -> chỉ hiện lỗi inline, cho phép nhập lại
    /// </summary>
    private void HandleFailedLogin(LoginResult result)
    {
        switch (result.Advice)
        {
            case ProtocolAdvice.DO_NOT_RETRY:
                // Tài khoản bị cấm / chưa active — show popup, không cho retry
                ShowPopup("Không thể đăng nhập", result.ErrorMessage!, isRetry: false);
                break;

            case ProtocolAdvice.BACKOFF_RETRY:
                // Tài khoản bị khóa tạm thời — show popup có nút retry
                ShowPopup("Tài khoản của bạn đã bị khóa tạm thời. Vui lòng thử lại sau.", result.ErrorMessage!, isRetry: true);
                break;

            case ProtocolAdvice.FIX_AND_RETRY:
            default:
                // Sai mật khẩu / tài khoản không tồn tại — inline error, cho nhập lại
                SetError(result.ErrorMessage!);
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

    /// <summary>
    /// K?t n?i m?ng + handshake khi màn hình load.
    /// G?i l?i du?c khi user nh?n "Retry".
    /// </summary>
    private async System.Threading.Tasks.Task InitConnectionAsync()
    {
        IsLoading = true;
        IsNetworkReady = false;

        ConnectionResult result = await _loginService.ConnectAsync();

        IsLoading = false;

        if (result.IsSuccess)
        {
            IsNetworkReady = true;
        }
        else
        {
            ShowPopup("Lỗi kết nối", result.ErrorMessage!, isRetry: true);
        }
    }
}
