// Copyright (c) 2026 PPN Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AutoX.Gara.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    #region Properties

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordCommand { get; }

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
        IsPasswordHidden = true;

        Username = System.String.Empty;
        Password = System.String.Empty;
        LoginCommand = new RelayCommand(OnLogin);
        TogglePasswordCommand = new RelayCommand(TogglePasswordVisibility);
    }

    #endregion Constructors

    #region Private Methods

    private void OnLogin()
    {
        HasError = false;
        ErrorMessage = null;

        // Validate input
        if (System.String.IsNullOrWhiteSpace(Username) || System.String.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "username and password are required.";
            HasError = true;
            return;
        }

        // TODO: Replace with real authentication logic
        if (Username == "admin" && Password == "123456")
        {
            // Navigate to main menu, e.g. Shell.Current.GoToAsync("//Dashboard");
        }
        else
        {
            ErrorMessage = "Invalid login credentials.";
            HasError = true;
        }
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        OnPropertyChanged(nameof(PasswordIcon)); // Để cập nhật icon nếu đổi trạng thái
    }

    #endregion Private Methods
}