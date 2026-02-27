// Copyright (c) 2026 PPN Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System.Linq;
using System.Windows.Input;

namespace AutoX.Gara.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    #region Properties

    public ICommand LoginCommand { get; }
    public ICommand ClosePopupCommand { get; }
    public ICommand TogglePasswordCommand { get; }


    [ObservableProperty]
    public partial System.Boolean IsLoading { get; set; }

    [ObservableProperty]
    public partial System.Boolean IsPopupVisible { get; set; }

    [ObservableProperty]
    public partial System.String PopupTitle { get; set; }

    [ObservableProperty]
    public partial System.String PopupMessage { get; set; }

    [ObservableProperty]
    public partial System.String PopupButtonText { get; set; }

    [ObservableProperty]
    public partial System.Boolean IsNetworkReady { get; set; }


    [ObservableProperty]
    public partial System.String Username { get; set; }

    [ObservableProperty]
    public partial System.String Password { get; set; }

    [ObservableProperty]
    public partial System.String? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial System.Boolean HasError { get; set; }

    [ObservableProperty]
    public partial System.Boolean IsPasswordHidden { get; set; }


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
        LoginCommand = new RelayCommand(OnLogin);
        ClosePopupCommand = new RelayCommand(ClosePopup);
        TogglePasswordCommand = new RelayCommand(TogglePasswordVisibility);

        _ = InitConnectionAsync();
    }

    #endregion Constructors

    #region Private Methods

    private void ShowPopup(System.String title, System.String message, System.String buttonText)
    {
        PopupTitle = title;
        PopupMessage = message;
        PopupButtonText = buttonText;
        IsPopupVisible = true;
    }

    private void ClosePopup()
    {
        IsPopupVisible = false;

        if (!IsNetworkReady)
        {
            //_ = InitConnectionAsync();
        }
    }

    private void OnLogin()
    {
        HasError = false;
        ErrorMessage = null;

        // 1. Kiểm tra username rỗng
        if (System.String.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username cannot be empty.";
            HasError = true;
            return;
        }

        // 2. Kiểm tra password rỗng
        if (System.String.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password cannot be empty.";
            HasError = true;
            return;
        }

        // 3. Validate cú pháp username
        if (!VALIDATE_USERNAME(Username))
        {
            ErrorMessage = "Username is invalid: only letters, numbers, '_' and '-' allowed and maximum 50 characters.";
            HasError = true;
            return;
        }

        // 4. Validate cú pháp password
        if (!VALIDATE_PASSWORD(Password))
        {
            ErrorMessage = "Password must be at least 8 characters, include upper/lowercase, numbers, and a special character.";
            HasError = true;
            return;
        }

        // 5. Login logic (demo)
        if (Username == "admin" && Password == "123456")
        {
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
            ErrorMessage = "Invalid login credentials. Please check your username and password.";
            HasError = true;
        }
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        OnPropertyChanged(nameof(PasswordIcon));
    }

    private async System.Threading.Tasks.Task InitConnectionAsync()
    {
        ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

        IsLoading = true;
        IsNetworkReady = false;

        try
        {
            // Attempt connect
            await client.ConnectAsync();

            // Khi connect thành công, thực hiện handshake
            System.Boolean handshakeSuccess = await client.HandshakeAsync(timeoutMs: 5000);

            IsLoading = false;

            if (handshakeSuccess)
            {
                IsNetworkReady = true;
            }
            else
            {
                ShowPopup("Handshake failed", "Cannot establish secure connection. Please try again.", "Try again");
            }
        }
        catch (System.Exception)
        {
            IsLoading = false;
            ShowPopup("Network error", "Cannot connect to server. Please check your network.", "Try again");
        }
    }

    #endregion Private Methods

    #region Private Validation

    // Validate username: không rỗng, đúng format, đúng độ dài
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

    // Validate password: dài ≥8, có hoa, thường, số, đặc biệt - bạn điều chỉnh theo nhu cầu
    private static System.Boolean VALIDATE_PASSWORD(System.String password)
        => !System.String.IsNullOrWhiteSpace(password) && password.Length >= 8 && password.Any(System.Char.IsLower)
        && password.Any(System.Char.IsUpper) && password.Any(System.Char.IsDigit) && !password.All(System.Char.IsLetterOrDigit);

    #endregion Private Validation
}