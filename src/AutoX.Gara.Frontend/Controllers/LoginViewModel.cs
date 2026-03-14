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
/// <para>ViewModel cho màn hình dang nh?p.</para>
/// <para>
/// Trách nhi?m duy nh?t (SRP):
///   - Qu?n lý tr?ng thái UI (IsLoading, HasError, Popup...)
///   - Ði?u ph?i lu?ng: validate ? connect ? login ? navigate
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
    /// Token d? h?y login dang ch?y khi user b?m nút khác / thoát màn hình.
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
    /// Constructor nh?n dependencies qua DI — d? unit test hon <c>InstanceManager</c>.
    /// N?u chua dùng DI container, b?n có th? dùng constructor m?c d?nh bên du?i.
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
        // H?y login tru?c dó n?u dang ch?y (ví d? user b?m nhanh 2 l?n)
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
            SetError("Tên dang nh?p không du?c d? tr?ng.");
            return false;
        }

        if (System.String.IsNullOrWhiteSpace(Password))
        {
            SetError("M?t kh?u không du?c d? tr?ng.");
            return false;
        }

        if (!AccountValidation.IsValidUsername(Username))
        {
            SetError("Tên dang nh?p không h?p l?: ch? cho phép ch? cái, s?, '_' và '-', t?i da 50 ký t?.");
            return false;
        }

        if (!AccountValidation.IsValidPassword(Password))
        {
            SetError("M?t kh?u ph?i có ít nh?t 8 ký t?, g?m ch? hoa, thu?ng, s? và ký t? d?c bi?t.");
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
    /// X? lý ph?n h?i l?i t? server theo <see cref="ProtocolAdvice"/>:
    /// - DO_NOT_RETRY  ? khóa nút login
    /// - BACKOFF_RETRY ? hi?n popup có nút retry
    /// - FIX_AND_RETRY ? ch? hi?n l?i inline, cho phép nh?p l?i
    /// </summary>
    private void HandleFailedLogin(LoginResult result)
    {
        switch (result.Advice)
        {
            case ProtocolAdvice.DO_NOT_RETRY:
                // Tài kho?n b? c?m / chua active — show popup, không cho retry
                ShowPopup("Không th? dang nh?p", result.ErrorMessage!, isRetry: false);
                break;

            case ProtocolAdvice.BACKOFF_RETRY:
                // Tài kho?n b? lock t?m th?i — show popup có nút retry
                ShowPopup("Tài kho?n b? khóa t?m th?i", result.ErrorMessage!, isRetry: true);
                break;

            case ProtocolAdvice.FIX_AND_RETRY:
            default:
                // Sai pass / tài kho?n không t?n t?i — inline error, cho nh?p l?i
                SetError(result.ErrorMessage!);
                break;
        }
    }

    private void ShowPopup(System.String title, System.String message, System.Boolean isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? "Th? l?i" : "OK";
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
            ShowPopup("L?i k?t n?i", result.ErrorMessage!, isRetry: true);
        }
    }
}
