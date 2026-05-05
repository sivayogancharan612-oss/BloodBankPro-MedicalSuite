using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class BloodRequestView : UserControl
    {
        private List<BloodRequest> _all = new();
        private BloodRequest? _editing;
        public BloodRequestView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAdmin)
                ColRequestDelete.Visibility = Visibility.Collapsed;
            LoadData();
        }
        private void LoadData() { _all = DatabaseHelper.GetBloodRequests(); ApplyFilter(); }

        private void ApplyFilter()
        {
            if (!this.IsLoaded || _all == null) return;
            string q   = TxtSearch.Text.Trim().ToLower();
            string bt  = ViewHelper.GetCombo(CmbBlood,   "All Types");
            string urg = ViewHelper.GetCombo(CmbUrgency, "All Urgency");
            string st  = ViewHelper.GetCombo(CmbStatus,  "All Status");
            var r = _all.AsEnumerable();
            if (!string.IsNullOrEmpty(q))  r = r.Where(x => x.PatientName.ToLower().Contains(q) || x.HospitalName.ToLower().Contains(q));
            if (bt  != "All Types")         r = r.Where(x => x.BloodType    == bt);
            if (urg != "All Urgency")       r = r.Where(x => x.UrgencyLevel == urg);
            if (st  != "All Status")        r = r.Where(x => x.Status       == st);
            var list = r.ToList();
            DgRequests.ItemsSource = list;
            TxtCount.Text = $"Showing {list.Count} of {_all.Count} requests";
        }

        private void OnFilter(object s, object e) => ApplyFilter();

        private void BtnFulfill_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not BloodRequest req) return;

            var confirm = MessageBox.Show(
                $"Fulfill request for {req.PatientName}?\n\nBlood Type: {req.BloodType}  |  Units: {req.UnitsNeeded}\nHospital: {req.HospitalName}\n\nStock will be deducted (FIFO). You will have 10 seconds to undo.",
                "Confirm Fulfillment", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var (ok, error, con, tx) = DatabaseHelper.BeginFulfillTransaction(req);
            if (!ok)
            {
                MessageBox.Show(error, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var undoWin = new UndoFulfillWindow(con!, tx!, req) { Owner = Window.GetWindow(this) };
            undoWin.ShowDialog();

            LoadData();

            if (!undoWin.WasUndone)
                MessageBox.Show($"✅ Fulfilled — {req.UnitsNeeded} unit(s) of {req.BloodType} committed to {req.HospitalName}.",
                    "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _editing = null;
            FrmTitle.Text = "New Blood Request";
            FHospital.ItemsSource = DatabaseHelper.GetHospitals();
            ClearForm();
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not BloodRequest req) return;
            _editing = req; FrmTitle.Text = "Edit Request";
            FHospital.ItemsSource = DatabaseHelper.GetHospitals();
            FHospital.SelectedValue = req.HospitalId;
            FPatient.Text = req.PatientName;
            FUnits.Text   = req.UnitsNeeded.ToString();
            FNotes.Text   = req.Notes;
            ViewHelper.SetCombo(FBlood,   req.BloodType);
            ViewHelper.SetCombo(FUrgency, req.UrgencyLevel);
            ViewHelper.SetCombo(FStatus,  req.Status);
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not BloodRequest req) return;
            if (MessageBox.Show($"Delete request for '{req.PatientName}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            DatabaseHelper.DeleteBloodRequest(req.Id, req.PatientName);
            LoadData();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (FHospital.SelectedItem == null)           { ShowErr("Please select a hospital."); return; }
            if (string.IsNullOrWhiteSpace(FPatient.Text)) { ShowErr("Patient name is required."); return; }
            if (FBlood.SelectedItem == null)              { ShowErr("Please select a blood type."); return; }
            if (!int.TryParse(FUnits.Text, out int u) || u <= 0) { ShowErr("Units must be greater than 0."); return; }

            var req = new BloodRequest
            {
                HospitalId   = (int)FHospital.SelectedValue!,
                PatientName  = FPatient.Text.Trim(),
                BloodType    = ViewHelper.GetCombo(FBlood),
                UnitsNeeded  = u,
                UrgencyLevel = ViewHelper.GetCombo(FUrgency, "Medium"),
                Status       = ViewHelper.GetCombo(FStatus,  "Pending"),
                Notes        = FNotes.Text.Trim(),
                RequestDate  = _editing?.RequestDate ?? DateTime.Today
            };
            if (_editing != null) { req.Id = _editing.Id; DatabaseHelper.UpdateBloodRequest(req); }
            else DatabaseHelper.AddBloodRequest(req);
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e) => ViewHelper.ExportToCsv(DgRequests, "BloodRequests");
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ClearForm() { FHospital.SelectedIndex=-1; FPatient.Text=FUnits.Text=FNotes.Text=""; FBlood.SelectedIndex=0; FUrgency.SelectedIndex=2; FStatus.SelectedIndex=0; FError.Visibility=Visibility.Collapsed; }
        private void ShowErr(string m) { FError.Text=m; FError.Visibility=Visibility.Visible; }
    }
}
