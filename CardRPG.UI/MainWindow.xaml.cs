using System.Windows;
using CardRPG.Core.Services;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class MainWindow : Window
{
    private AuthService _authService;

    public MainWindow()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string user = UsernameBox.Text;
        string pass = PasswordBox.Password;

        var loggedUser = _authService.Login(user, pass);

        if (loggedUser != null)
        {
            
            GameWindow game = new GameWindow(loggedUser);
            game.Show();
            
            this.Close(); 
        }
        else
        {
            StatusText.Text = "Invalid username or password!";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        string user = UsernameBox.Text;
        string pass = PasswordBox.Password;

        var newUser = _authService.Register(user, pass);

        if (newUser != null)
        {
            StatusText.Text = "Account created! You can now login.";
            StatusText.Foreground = System.Windows.Media.Brushes.Green;
        }
        else
        {
            StatusText.Text = "Username already taken.";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}