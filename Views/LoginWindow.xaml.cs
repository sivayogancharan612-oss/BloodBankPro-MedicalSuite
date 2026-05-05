using System.Windows;
using System.Windows.Input;
using BloodBankPro.Database;

namespace BloodBankPro.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow() { InitializeComponent(); TxtUsername.Focus(); }
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => TryLogin();
        private void OnKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) TryLogin(); }

        private void TryLogin()
        {
            TxtError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            { TxtError.Text = "Please enter your username."; TxtError.Visibility = Visibility.Visible; return; }

            var user = DatabaseHelper.Login(TxtUsername.Text.Trim(), PwdPassword.Password);
            if (user == null)
            { TxtError.Text = "❌ Invalid username or password."; TxtError.Visibility = Visibility.Visible; PwdPassword.Clear(); return; }

            Session.CurrentUser = user;
            new MainWindow(user).Show();
            Close();
        }
    }
}
