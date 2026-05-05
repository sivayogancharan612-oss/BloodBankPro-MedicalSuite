using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class UserManagementView : UserControl
    {
        private List<User> _all = new();
        private User? _editing;
        public UserManagementView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();
        private void LoadData() { _all = DatabaseHelper.GetUsers(); DgUsers.ItemsSource = _all; TxtCount.Text = $"{_all.Count} user account(s)"; }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"BloodBankPro_Backup_{DateTime.Now:yyyyMMdd}.bak",
                DefaultExt = ".bak",
                Filter = "SQL Backup Files (*.bak)|*.bak"
            };

            if (dlg.ShowDialog() == true)
            {
                var (success, msg) = DatabaseHelper.BackupDatabase(dlg.FileName);
                if (success)
                {
                    DatabaseHelper.Log("BACKUP", "System", "Full database backup generated successfully.");
                    MessageBox.Show("Database backup created successfully!", "Backup Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Backup failed: {msg}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        { _editing = null; FrmTitle.Text = "Add User Account"; ClearForm(); ViewHelper.ShowOverlay(Overlay); }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not User u) return;
            _editing = u; FrmTitle.Text = "Edit User Account";
            FUsername.Text = u.Username; FPassword.Text = u.Password;
            FFullName.Text = u.FullName; FEmail.Text = u.Email;
            ViewHelper.SetCombo(FRole, u.Role);
            ViewHelper.SetCombo(FActive, u.IsActive ? "Active" : "Inactive");
            FUsername.IsEnabled = false; // Prevent username change on edit
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not User u) return;
            if (u.Username == Session.CurrentUser?.Username)
            { MessageBox.Show("You cannot delete your own account.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (MessageBox.Show($"Delete user account '{u.Username}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            DatabaseHelper.DeleteUser(u.Id, u.Username);
            LoadData();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(FUsername.Text)) { ShowErr("Username is required."); return; }
            if (string.IsNullOrWhiteSpace(FPassword.Text) || FPassword.Text.Length < 6) { ShowErr("Password must be at least 6 characters."); return; }
            if (string.IsNullOrWhiteSpace(FFullName.Text)) { ShowErr("Full name is required."); return; }

            var u = new User
            {
                Username = FUsername.Text.Trim(),
                Password = FPassword.Text.Trim(),
                FullName = FFullName.Text.Trim(),
                Email    = FEmail.Text.Trim(),
                Role     = ViewHelper.GetCombo(FRole, "Staff"),
                IsActive = ViewHelper.GetCombo(FActive, "Active") == "Active",
                CreatedDate = _editing?.CreatedDate ?? DateTime.Today
            };

            if (_editing != null)
            {
                u.Id = _editing.Id;
                u.Username = _editing.Username; // Keep original username
                DatabaseHelper.UpdateUser(u);
            }
            else
            {
                if (_all.Any(x => x.Username == u.Username))
                { ShowErr("Username already exists. Please choose a different one."); return; }
                DatabaseHelper.AddUser(u);
            }
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        { FUsername.IsEnabled = true; ViewHelper.HideOverlay(Overlay); }
        private void ClearForm() { FUsername.Text=FPassword.Text=FFullName.Text=FEmail.Text=""; FRole.SelectedIndex=0; FActive.SelectedIndex=0; FUsername.IsEnabled=true; FError.Visibility=Visibility.Collapsed; }
        private void ShowErr(string m) { FError.Text=m; FError.Visibility=Visibility.Visible; }
    }
}
