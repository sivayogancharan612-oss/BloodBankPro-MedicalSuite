using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class MainWindow : Window
    {
        private readonly Button[] _navBtns = null!;

        public MainWindow(User user)
        {
            InitializeComponent();
            _navBtns = new[] { Btn0, Btn1, Btn2, Btn3, Btn4, Btn5, Btn6, Btn7, Btn8, Btn9, Btn10 };
            TxtUserName.Text = user.FullName;
            TxtUserRole.Text = user.Role;

            if (!Session.IsAdmin)
            {
                Btn9.Visibility  = Visibility.Collapsed;
                Btn10.Visibility = Visibility.Collapsed;
            }

            Navigate(0);
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idx))
                Navigate(idx);
        }

        private void Navigate(int idx)
        {
            foreach (var b in _navBtns) b.Style = (Style)FindResource("NavBtn");
            _navBtns[idx].Style = (Style)FindResource("NavBtnOn");

            ContentArea.Content = idx switch
            {
                0  => new DashboardView(),
                1  => new DonorView(),
                2  => new BloodStockView(),
                3  => new HospitalView(),
                4  => new BloodRequestView(),
                5  => new DonationView(),
                6  => new AppointmentView(),
                7  => new CompatibilityView(),
                8  => new ReportView(),
                9  => new UserManagementView(),
                10 => new AuditLogView(),
                _  => new DashboardView()
            };
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbLanguage.SelectedItem is ComboBoxItem item)
                LocalizationManager.SetLanguage(item.Tag.ToString()!);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sign out of Blood Bank Pro?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DatabaseHelper.Log("UPDATE", "System", $"User {Session.CurrentUser?.Username} signed out");
                Session.CurrentUser = null;
                new LoginWindow().Show();
                Close();
            }
        }
    }
}
