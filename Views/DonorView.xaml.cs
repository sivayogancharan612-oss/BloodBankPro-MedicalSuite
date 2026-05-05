using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;
using BloodBankPro.Repositories;
using BloodBankPro.ViewModels;

namespace BloodBankPro.Views
{
    public partial class DonorView : UserControl
    {
        private readonly DonorViewModel _viewModel = new(new DonorRepository());
        private List<Donor> _all = new();
        private Donor? _editing;
        public DonorView() 
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAdmin)
                ColDonorDelete.Visibility = Visibility.Collapsed;
            LoadData();
        }
        private void LoadData() { _all = _viewModel.Load(); ApplyFilter(); }

        private void ApplyFilter()
        {
            if (!this.IsLoaded || _all == null) return;
            string q  = TxtSearch.Text;
            string bt = ViewHelper.GetCombo(CmbBlood, "All Types");
            string st = ViewHelper.GetCombo(CmbStatus, "All Status");
            var list = _viewModel.Filter(q, bt, st);
            DgDonors.ItemsSource = list;
            TxtCount.Text = $"Showing {list.Count} of {_viewModel.TotalCount} donors";
        }

        private void OnFilter(object s, object e) => ApplyFilter();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        { _editing = null; FrmTitle.Text = "Add New Donor"; ClearForm(); ViewHelper.ShowOverlay(Overlay); }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Donor d) return;
            _editing = d; FrmTitle.Text = "Edit Donor";
            FName.Text = d.FullName; FAge.Text = d.Age.ToString(); FPhone.Text = d.Phone;
            FEmail.Text = d.Email; FAddress.Text = d.Address; FWeight.Text = d.Weight.ToString();
            FLastDonation.SelectedDate = d.LastDonationDate;
            ViewHelper.SetCombo(FBlood, d.BloodType);
            ViewHelper.SetCombo(FGender, d.Gender);
            ViewHelper.SetCombo(FStatus, d.Status);
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Donor d) return;
            if (MessageBox.Show($"Delete donor '{d.FullName}'?\n\nThis cannot be undone.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try { _viewModel.Delete(d); LoadData(); }
            catch { MessageBox.Show("Cannot delete — donor has donation/appointment records linked.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(FName.Text))               { ShowErr("Full name is required."); return; }
            if (FBlood.SelectedItem == null)                          { ShowErr("Please select a blood type."); return; }
            if (!int.TryParse(FAge.Text, out int age) || age < 18 || age > 70) { ShowErr("Age must be between 18 and 70."); return; }
            double.TryParse(FWeight.Text, out double weight);

            var d = new Donor
            {
                FullName = FName.Text.Trim(), BloodType = ViewHelper.GetCombo(FBlood),
                Age = age, Gender = ViewHelper.GetCombo(FGender, "Male"),
                Phone = FPhone.Text.Trim(), Email = FEmail.Text.Trim(),
                Address = FAddress.Text.Trim(), Weight = weight,
                LastDonationDate = FLastDonation.SelectedDate,
                Status = ViewHelper.GetCombo(FStatus, "Active"),
                RegisteredDate = _editing?.RegisteredDate ?? DateTime.Today
            };
            if (_editing != null) d.Id = _editing.Id;
            _viewModel.Save(d);
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e) =>
            ViewHelper.ExportToCsv(DgDonors, "Donors");

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ClearForm() { FName.Text=FAge.Text=FPhone.Text=FEmail.Text=FAddress.Text=FWeight.Text=""; FBlood.SelectedIndex=0; FGender.SelectedIndex=0; FStatus.SelectedIndex=0; FLastDonation.SelectedDate=null; FError.Visibility=Visibility.Collapsed; }
        private void ShowErr(string m) { FError.Text=m; FError.Visibility=Visibility.Visible; }
    }
}
