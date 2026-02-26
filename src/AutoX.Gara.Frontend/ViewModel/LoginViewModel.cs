using AutoX.Gara.Frontend;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

public class LoginViewModel : ReactiveObject
{
    public System.String Username { get; set; } = "";
    public System.String Password { get; set; } = "";

    private System.String _errorMessage;
    public System.String ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public LoginViewModel() => LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);

    private async Task LoginAsync()
    {
        // Validate username/password (có thể gọi backend/api)
        if (Username == "admin" && Password == "123456")
        {
            ErrorMessage = "";
            // Đóng LoginWindow, mở MainWindow
            OpenMainWindow();
        }
        else
        {
            ErrorMessage = "Sai tài khoản hoặc mật khẩu!";
        }
    }

    private void OpenMainWindow()
    {
        var main = new MainWindow();
        main.Show();
        // Đóng LoginWindow
        var loginWindow = Avalonia.Application.Current?.ApplicationLifetime switch
        {
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop =>
                desktop.Windows.FirstOrDefault(w => w is LoginWindow),
            _ => null
        };
        loginWindow?.Close();
    }
}