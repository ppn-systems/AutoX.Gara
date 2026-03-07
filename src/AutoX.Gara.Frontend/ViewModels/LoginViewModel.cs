// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Common.Diagnostics;
using Nalix.Common.Messaging.Protocols;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Messaging.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoX.Gara.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    #region Properties

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordCommand { get; }
    public ICommand ClosePopupCommand { get; }
    public ICommand RetryConnectionCommand { get; }
    public ICommand PopupButtonCommand { get; private set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsPopupVisible { get; set; }

    [ObservableProperty]
    public partial string PopupTitle { get; set; }

    [ObservableProperty]
    public partial string PopupMessage { get; set; }

    [ObservableProperty]
    public partial string PopupButtonText { get; set; }

    [ObservableProperty]
    public partial bool IsNetworkReady { get; set; }

    [ObservableProperty]
    public partial string Username { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool IsPasswordHidden { get; set; }

    public System.String PasswordIcon => IsPasswordHidden ? "eye_off.png" : "eye.png";

    #endregion Properties

    #region Constructors

    public LoginViewModel()
    {
        HasError = false;
        IsLoading = true;
        IsPopupVisible = false;
        IsPasswordHidden = true;

        PopupButtonText = "OK";
        PopupTitle = System.String.Empty;
        PopupMessage = System.String.Empty;

        Username = System.String.Empty;
        Password = System.String.Empty;

        LoginCommand = new AsyncRelayCommand(OnLoginAsync);
        TogglePasswordCommand = new RelayCommand(TogglePasswordVisibility);
        ClosePopupCommand = new RelayCommand(ClosePopup);
        RetryConnectionCommand = new RelayCommand(ExecuteRetryConnection);

        // Ban đầu, thiết lập command cho popup là đóng popup
        PopupButtonCommand = ClosePopupCommand;

        _ = InitConnectionAsync();
    }

    #endregion Constructors

    #region Private Methods

    // Method gọi lại khi nhấn Retry
    private void ExecuteRetryConnection()
    {
        IsPopupVisible = false;
        _ = InitConnectionAsync();
    }

    /// <summary>
    /// Hiển thị popup, đồng thời xác định nút bấm là OK hoặc Retry theo từng loại lỗi.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="isRetry"></param>
    private void ShowPopup(System.String title, System.String message, System.Boolean isRetry = false)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupVisible = true;

        if (isRetry)
        {
            PopupButtonText = "Try again";
            PopupButtonCommand = RetryConnectionCommand;
        }
        else
        {
            PopupButtonText = "OK";
            PopupButtonCommand = ClosePopupCommand;
        }
    }

    private void ClosePopup() => IsPopupVisible = false;

    private async Task OnLoginAsync()
    {
        HasError = false;
        ErrorMessage = null;

        if (System.String.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username cannot be empty.";
            HasError = true;
            return;
        }

        if (System.String.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password cannot be empty.";
            HasError = true;
            return;
        }

        //if (!VALIDATE_USERNAME(Username))
        //{
        //    ErrorMessage = "Username is invalid: only letters, numbers, '_' and '-' allowed and maximum 50 characters.";
        //    HasError = true;
        //    return;
        //}

        //if (!VALIDATE_PASSWORD(Password))
        //{
        //    ErrorMessage = "Password must be at least 8 characters, include upper/lowercase, numbers, and a special character.";
        //    HasError = true;
        //    return;
        //}

        try
        {
            var client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            // 1. Đóng gói dữ liệu thành AccountPacket
            var accountModel = new Shared.Models.AccountModel
            {
                Username = this.Username,
                Password = this.Password
            };
            var packet = new AccountPacket();
            packet.Initialize((UInt16)OpCommand.LOGIN, accountModel);

            // 2. Đăng ký "lắng nghe" phản hồi login một lần duy nhất (OnOnce)
            IDisposable? sub = null;
            sub = client.OnOnce<Directive>(
                p => p.OpCode == (UInt16)OpCommand.LOGIN, // lọc đúng loại trả về
                resp =>
                {
                    IsLoading = false;
                    sub?.Dispose(); // huỷ đăng ký sau khi nhận
                    if (resp.Type == ControlType.NONE)
                    {
                        // Đăng nhập thành công
                        ShellItem? loginItem = Shell.Current.Items.FirstOrDefault(i => i.Title == "Login");
                        if (loginItem != null)
                        {
                            Shell.Current.Items.Remove(loginItem);
                        }

                        Application.Current?.Windows[0].Width = 1280;
                        Application.Current?.Windows[0].Height = 720;
                        Shell.Current.GoToAsync("///MainPage");
                    }
                    else
                    {
                        HandleLoginResponse(resp);
                    }
                });

            // 3. Gửi packet tới server (thường là Send hoặc SendAsync)
            await client.SendAsync(packet); // hoặc await client.SendAsync(packet);

            // Có thể thêm timeout: nếu sau X giây không nhận được, thì báo lỗi
            await Task.Delay(8000); // 8 giây (ví dụ)
            if (IsLoading)
            {
                sub?.Dispose();
                IsLoading = false;
                ErrorMessage = "Login timeout.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            IsLoading = false;
            ErrorMessage = $"Login error: {ex.Message}";
            HasError = true;
        }
    }

    private void HandleLoginResponse(Directive respPacket)
    {
        // Giả lập model phản hồi: bạn cần map lại nếu ControlType hay ControlPacket của bạn khác
        var reason = respPacket.Reason;    // thuộc tính ProtocolReason
        var advice = respPacket.Action;    // thuộc tính ProtocolAdvice
        System.String error = reason switch
        {
            ProtocolReason.MALFORMED_PACKET => "Packet định dạng không hợp lệ, vui lòng thử lại.",
            ProtocolReason.NOT_FOUND => "Tài khoản không tồn tại.",
            ProtocolReason.ACCOUNT_LOCKED => "Tài khoản tạm bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau.",
            ProtocolReason.UNAUTHENTICATED => "Sai mật khẩu. Vui lòng kiểm tra lại.",
            ProtocolReason.FORBIDDEN => "Tài khoản bị cấm sử dụng hoặc chưa kích hoạt. Vui lòng liên hệ quản trị viên.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống nội bộ. Vui lòng thử lại sau.",
            _ => "Đăng nhập thất bại. Vui lòng thử lại."
        };

        switch (advice)
        {
            case ProtocolAdvice.FIX_AND_RETRY:
                // Gợi ý hiện lỗi cho phép nhập lại tài khoản (không khóa nút login)
                break;

            case ProtocolAdvice.DO_NOT_RETRY:
                // Hiện lỗi, disable login field, hoặc show popup cảnh báo! 
                break;

            case ProtocolAdvice.BACKOFF_RETRY:
                // Có thể hiện đồng hồ đếm ngược
                break;
                // Các advice khác...
        }

        ErrorMessage = error;
        HasError = true;
        return;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        OnPropertyChanged(nameof(PasswordIcon));
    }

    /// <summary>
    /// Kết nối mạng, hiển thị loading khi retry, hiện popup retry khi lỗi.
    /// </summary>
    private async System.Threading.Tasks.Task InitConnectionAsync()
    {
        ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

        IsLoading = true;
        IsNetworkReady = false;

        try
        {
            // Attempt connect
            await client.ConnectAsync("127.0.0.1", 57206);

            System.Boolean handshakeSuccess = await client.HandshakeAsync((System.UInt16)OpCommand.HANDSHAKE, timeoutMs: 10000);
            IsLoading = false;

            if (handshakeSuccess)
            {
                IsNetworkReady = true;
            }
            else
            {
                // Hiện popup dạng có nút Retry
                ShowPopup("Handshake failed", $"key_length={client.Options.EncryptionKey?.Length} IsConnected={client.IsConnected}", isRetry: true);
            }
        }
        catch (System.Exception ex)
        {
            IsLoading = false;
            InstanceManager.Instance.GetExistingInstance<ILogger>()?.Error($"[ERROR] Failed to connect or handshake: {ex}");
            // Hiện popup dạng có Retry
            ShowPopup("Network error", ex.Message, isRetry: true);
        }
    }

    #endregion Private Methods

    #region Private Validation

    private static System.Boolean VALIDATE_USERNAME(System.String username)
    {
        if (System.String.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username.Length > 50)
        {
            return false;
        }

        foreach (System.Char c in username)
        {
            if (!IS_ALLOWED_USERNAME_CHAR(c))
            {
                return false;
            }
        }

        return true;

        static System.Boolean IS_ALLOWED_USERNAME_CHAR(System.Char c) => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-';
    }

    private static System.Boolean VALIDATE_PASSWORD(System.String password)
        => !System.String.IsNullOrWhiteSpace(password) && password.Length >= 8 && password.Any(System.Char.IsLower)
        && password.Any(System.Char.IsUpper) && password.Any(System.Char.IsDigit) && !password.All(System.Char.IsLetterOrDigit);

    #endregion Private Validation
}