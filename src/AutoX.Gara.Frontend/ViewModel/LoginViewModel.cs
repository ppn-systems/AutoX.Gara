// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Configuration;
using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Shared.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.Framework.Tasks;
using Nalix.SDK.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.UI.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAccountService _accountService;
    private readonly INavigationService _navigationService;
    private readonly UiTextOptions _loginText;
    private CancellationTokenSource? _loginCts;

    [ObservableProperty] public partial bool HasError { get; set; }
    [ObservableProperty] public partial bool IsNetworkReady { get; set; }
    [ObservableProperty] public partial bool IsLoading { get; set; } = true;
    [ObservableProperty] public partial bool IsPasswordHidden { get; set; } = true;
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial string Username { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;

    [ObservableProperty] public partial bool IsPopupRetry { get; set; }
    [ObservableProperty] public partial bool IsPopupVisible { get; set; }
    [ObservableProperty] public partial string PopupButtonText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string PopupMessage { get; set; } = string.Empty;

    public bool IsPopupNotRetry => !IsPopupRetry;
    public bool IsNetworkNotReady => !IsNetworkReady;
    public string PasswordIcon => IsPasswordHidden ? "eye_off.png" : "eye.png";
    public string AppTitleText => _loginText.LoginAppTitle;
    public string AppSubtitleText => _loginText.LoginAppSubtitle;
    public string UsernameLabelText => _loginText.LoginUsernameLabel;
    public string UsernamePlaceholderText => _loginText.LoginUsernamePlaceholder;
    public string PasswordLabelText => _loginText.LoginPasswordLabel;
    public string PasswordPlaceholderText => _loginText.LoginPasswordPlaceholder;
    public string LoginButtonText => _loginText.LoginButtonText;
    public string RegisterButtonText => _loginText.RegisterButtonText;
    public string NetworkWarningText => _loginText.LoginNetworkWarningText;
    public string LoadingText => _loginText.LoginLoadingText;
    public string PopupOkButtonText => _loginText.PopupOkButtonText;

    public LoginViewModel(IAccountService accountService, INavigationService navigationService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _loginText = UiTextConfiguration.Current;
        PopupButtonText = _loginText.PopupOkButtonText;
        _ = InitConnectionAsync();
    }

    partial void OnIsPopupRetryChanged(bool value) => OnPropertyChanged(nameof(IsPopupNotRetry));
    partial void OnIsNetworkReadyChanged(bool value) => OnPropertyChanged(nameof(IsNetworkNotReady));

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (!PrepareAuthRequest())
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _accountService.AuthenticateAsync(Username, Password, _loginCts!.Token);
            if (result.IsSuccess)
            {
                SetupKeepAlive();
                await _navigationService.GoToMainPageAsync();
                return;
            }

            HandleFailedAuth(result, _loginText.LoginFailedTitleText);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (!PrepareAuthRequest())
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _accountService.RegisterAsync(Username, Password, _loginCts!.Token);
            if (result.IsSuccess)
            {
                ShowPopup(_loginText.RegisterSuccessTitleText, _loginText.RegisterSuccessMessageText, isRetry: false);
                return;
            }

            HandleFailedAuth(result, _loginText.RegisterFailedTitleText);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool PrepareAuthRequest()
    {
        _loginCts?.Cancel();
        _loginCts?.Dispose();
        _loginCts = new CancellationTokenSource();
        ClearError();
        return ValidateInputs();
    }

    private void SetupKeepAlive()
    {
        var client = InstanceManager.Instance.GetExistingInstance<TcpSession>();
        if (client is null)
        {
            return;
        }

        var taskManager = InstanceManager.Instance.GetOrCreateInstance<TaskManager>();
        taskManager.ScheduleRecurring(
            "KeepAlive",
            TimeSpan.FromSeconds(30),
            async token => await client.SendAsync(new Directive { Type = ControlType.HEARTBEAT }, token));
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
            SetError(_loginText.UsernameRequiredErrorText);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError(_loginText.PasswordRequiredErrorText);
            return false;
        }

        if (!AccountValidation.IsValidUsername(Username))
        {
            SetError(_loginText.UsernameInvalidErrorText);
            return false;
        }

        if (!AccountValidation.IsValidPassword(Password))
        {
            SetError(_loginText.PasswordInvalidErrorText);
            return false;
        }

        return true;
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void HandleFailedAuth(LoginResult result, string fallbackTitle)
    {
        switch (result.Advice)
        {
            case ProtocolAdvice.DO_NOT_RETRY:
                ShowPopup(fallbackTitle, result.ErrorMessage ?? _loginText.AuthRejectedMessageText, isRetry: false);
                break;

            case ProtocolAdvice.BACKOFF_RETRY:
                ShowPopup(fallbackTitle, result.ErrorMessage ?? _loginText.RetryLaterMessageText, isRetry: false);
                break;

            default:
                SetError(result.ErrorMessage ?? fallbackTitle);
                break;
        }
    }

    private void ShowPopup(string title, string message, bool isRetry)
    {
        PopupTitle = title;
        PopupMessage = message;
        IsPopupRetry = isRetry;
        PopupButtonText = isRetry ? _loginText.PopupRetryButtonText : _loginText.PopupOkButtonText;
        IsPopupVisible = true;
    }

    private async Task InitConnectionAsync()
    {
        IsLoading = true;
        IsNetworkReady = false;

        var result = await _accountService.ConnectAsync();

        IsLoading = false;
        if (result.IsSuccess)
        {
            IsNetworkReady = true;
        }
        else
        {
            ShowPopup(_loginText.ServerErrorTitleText, result.ErrorMessage ?? _loginText.ConnectFailedMessageText, isRetry: true);
        }
    }
}
