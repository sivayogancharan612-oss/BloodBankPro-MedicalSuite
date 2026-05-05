using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class HospitalView : UserControl
    {
        private List<Hospital> _all = new();
        private Hospital? _editing;
        public HospitalView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();
        private void LoadData() { _all = DatabaseHelper.GetHospitals(); Refresh(_all); }

        private void Refresh(List<Hospital> list)
        { DgHospitals.ItemsSource = list; TxtCount.Text = $"Showing {list.Count} hospitals"; }

        private void OnSearch(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            Refresh(string.IsNullOrEmpty(q) ? _all
                : _all.Where(h => h.Name.ToLower().Contains(q) || h.City.ToLower().Contains(q) || h.ContactPerson.ToLower().Contains(q)).ToList());
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        { _editing = null; FrmTitle.Text = "Add Hospital"; ClearForm(); ViewHelper.ShowOverlay(Overlay); }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Hospital h) return;
            _editing = h; FrmTitle.Text = "Edit Hospital";
            FName.Text = h.Name; FContact.Text = h.ContactPerson; FPhone.Text = h.Phone;
            FCity.Text = h.City; FEmail.Text = h.Email; FAddress.Text = h.Address;
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Hospital h) return;
            if (MessageBox.Show($"Delete hospital '{h.Name}'?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try { DatabaseHelper.DeleteHospital(h.Id, h.Name); LoadData(); }
            catch { MessageBox.Show("Cannot delete — hospital has blood requests linked to it.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(FName.Text)) { FError.Text = "Hospital name is required."; FError.Visibility = Visibility.Visible; return; }
            var h = new Hospital { Name = FName.Text.Trim(), ContactPerson = FContact.Text.Trim(), Phone = FPhone.Text.Trim(), City = FCity.Text.Trim(), Email = FEmail.Text.Trim(), Address = FAddress.Text.Trim() };
            if (_editing != null) { h.Id = _editing.Id; DatabaseHelper.UpdateHospital(h); }
            else DatabaseHelper.AddHospital(h);
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e) => ViewHelper.ExportToCsv(DgHospitals, "Hospitals");
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ClearForm() { FName.Text = FContact.Text = FPhone.Text = FCity.Text = FEmail.Text = FAddress.Text = ""; FError.Visibility = Visibility.Collapsed; }
    }
}
