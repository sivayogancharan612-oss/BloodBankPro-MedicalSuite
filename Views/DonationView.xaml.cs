using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class DonationView : UserControl
    {
        private List<DonationRecord> _all = new();
        public DonationView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();
        private void LoadData() { _all = DatabaseHelper.GetDonationRecords(); ApplyFilter(); }

        private void ApplyFilter()
        {
            if (!this.IsLoaded || _all == null) return;
            string q  = TxtSearch.Text.Trim().ToLower();
            string bt = ViewHelper.GetCombo(CmbBlood, "All Types");
            var r = _all.AsEnumerable();
            if (!string.IsNullOrEmpty(q))  r = r.Where(x => x.DonorName.ToLower().Contains(q));
            if (bt != "All Types")          r = r.Where(x => x.BloodType == bt);
            var list = r.ToList();
            DgDonations.ItemsSource = list;
            TxtCount.Text = $"Showing {list.Count} of {_all.Count} records";
        }

        private void OnFilter(object s, object e) => ApplyFilter();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            FDonor.ItemsSource = DatabaseHelper.GetDonors(status: "Active");
            FDonor.SelectedIndex = -1;
            FBlood.SelectedIndex = 0;
            FUnits.Text = "1";
            FDate.SelectedDate = DateTime.Today;
            FNotes.Text = "";
            EligBorder.Visibility = Visibility.Collapsed;
            FError.Visibility = Visibility.Collapsed;
            ViewHelper.ShowOverlay(Overlay);
        }

        private void FDonor_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            if (FDonor.SelectedItem is not Donor d) return;
            ViewHelper.SetCombo(FBlood, d.BloodType);
            if (d.EligibilityStatus != "Eligible")
            {
                EligText.Text = $"⚠  {d.FullName} — {d.EligibilityStatus}. Last donated: {d.LastDonationDisplay}. A 56-day gap is required between donations. Proceed only if medically approved.";
                EligBorder.Visibility = Visibility.Visible;
            }
            else
            {
                EligBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not DonationRecord dr) return;
            if (MessageBox.Show($"Delete donation record for '{dr.DonorName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            DatabaseHelper.DeleteDonationRecord(dr.Id);
            LoadData();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (FDonor.SelectedItem is not Donor donor) { ShowErr("Please select a donor."); return; }
            if (!int.TryParse(FUnits.Text, out int u) || u <= 0) { ShowErr("Units must be greater than 0."); return; }
            if (FDate.SelectedDate == null) { ShowErr("Please select a donation date."); return; }

            var record = new DonationRecord
            {
                DonorId      = donor.Id,
                DonorName    = donor.FullName,
                BloodType    = ViewHelper.GetCombo(FBlood),
                UnitsDonated = u,
                DonationDate = FDate.SelectedDate.Value,
                Notes        = FNotes.Text.Trim()
            };

            DatabaseHelper.AddDonationRecord(record);

            donor.LastDonationDate = FDate.SelectedDate.Value;
            DatabaseHelper.UpdateDonor(donor);

            ViewHelper.HideOverlay(Overlay);
            LoadData();
            MessageBox.Show($"✅ Donation recorded for {donor.FullName} ({donor.BloodType})\nLast donation date updated to {FDate.SelectedDate.Value:yyyy-MM-dd}.",
                "Donation Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e) => ViewHelper.ExportToCsv(DgDonations, "DonationRecords");
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ShowErr(string m) { FError.Text = m; FError.Visibility = Visibility.Visible; }
    }
}
