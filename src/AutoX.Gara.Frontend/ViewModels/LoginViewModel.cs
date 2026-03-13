// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
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
/// <para>ViewModel cho màn hình đăng nhập.</para>
/// <para>
/// Trách nhiệm duy nhất (SRP):
///   - Quản lý trạng thái UI (IsLoading, HasError, Popup...)
///   - Điều phối luồng: validate → connect → login → navigate
/// </para>
/// <para>Không chứa: network code, navigation code, validation rules.</para>
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    // ─── Dependencies (DI) ───────────────────────────────────────────────────

    private readonly IAccountService _loginService;
    private readonly INavigationService _navigation;

    // ─── Cancellation ────────────────────────────────────────────────────────

    /// <summary>
    /// Token để hủy login đang chạy khi user bấm nút khác / thoát màn hình.
    /// </summary>
    private System.Threading.CancellationTokenSource? _loginCts;

    // ─── Observable Properties ───────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean HasError { get; set; }
    [ObservableProperty] public partial System.Boolean IsNetworkReady { get; set; }
    [ObservableProperty] public partial System.Boolean IsLoading { get; set; } = true;
    [ObservableProperty] public partial System.Boolean IsPasswordHidden { get; set; } = true;

    [ObservableProperty] public partial System.String? ErrorMessage { get; set; }
    [ObservableProperty] public partial System.String Username { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String Password { get; set; } = System.String.Empty;

    // ── Popup ─────────────────────────────────────────────────────────────────

    [ObservableProperty] public partial System.Boolean IsPopupRetry { get; set; }
    [ObservableProperty] public partial System.Boolean IsPopupVisible { get; set; }
    [ObservableProperty] public partial System.String PopupButtonText { get; set; } = "OK";
    [ObservableProperty] public partial System.String PopupTitle { get; set; } = System.String.Empty;
    [ObservableProperty] public partial System.String PopupMessage { get; set; } = System.String.Empty;


    // ── Computed ──────────────────────────────────────────────────────────────

    public System.Boolean IsPopupNotRetry => !IsPopupRetry;
    public System.Boolean IsNetworkNotReady => !IsNetworkReady;
    public System.String PasswordIcon => IsPasswordHidden ? "eye_off.png" : "eye.png";


    // ─── Constructor ─────────────────────────────────────────────────────────

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

    // ─── Property Change Hooks ────────────────────────────────────────────────

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));

    partial void OnIsNetworkReadyChanged(bool value) => OnPropertyChanged(nameof(IsNetworkNotReady));

    [RelayCommand]
    private async System.Threading.Tasks.Task LoginAsync()
    {
        // Hủy login trước đó nếu đang chạy (ví dụ user bấm nhanh 2 lần)
        _loginCts?.Cancel();
        _loginCts = new System.Threading.CancellationTokenSource();
        var ct = _loginCts.Token;

        ClearError();

        // ── Validate input trước khi gửi network ──────────────────────────────
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
            // Đảm bảo IsLoading luôn được reset kể cả khi exception
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

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Validate username + password client-side, hiện lỗi ngay không cần gửi network.
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
            SetError("Tên đăng nhập không hợp lệ: chỉ cho phép chữ cái, số, '_' và '-', tối đa 50 ký tự.");
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
    /// Xử lý phản hồi lỗi từ server theo <see cref="ProtocolAdvice"/>:
    /// - DO_NOT_RETRY  → khóa nút login
    /// - BACKOFF_RETRY → hiện popup có nút retry
    /// - FIX_AND_RETRY → chỉ hiện lỗi inline, cho phép nhập lại
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
                // Tài khoản bị lock tạm thời — show popup có nút retry
                ShowPopup("Tài khoản bị khóa tạm thời", result.ErrorMessage!, isRetry: true);
                break;

            case ProtocolAdvice.FIX_AND_RETRY:
            default:
                // Sai pass / tài khoản không tồn tại — inline error, cho nhập lại
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
    /// Kết nối mạng + handshake khi màn hình load.
    /// Gọi lại được khi user nhấn "Retry".
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