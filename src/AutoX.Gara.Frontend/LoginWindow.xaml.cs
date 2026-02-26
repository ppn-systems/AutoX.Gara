using Avalonia.Controls;

namespace AutoX.Gara.Frontend;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}