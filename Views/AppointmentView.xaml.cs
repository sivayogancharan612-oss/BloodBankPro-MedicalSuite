using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class AppointmentView : UserControl
    {
        private List<Appointment> _all = new();
        private Appointment? _editing;
        public AppointmentView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();
        private void LoadData() { _all = DatabaseHelper.GetAppointments(); ApplyFilter(); }

        private void ApplyFilter()
        {
            if (!this.IsLoaded || _all == null) return;
            string st = ViewHelper.GetCombo(CmbStatus, "All Status");
            var r = _all.AsEnumerable();
            if (st != "All Status") r = r.Where(x => x.Status == st);
            if (DpFilter.SelectedDate.HasValue) r = r.Where(x => x.AppointmentDate.Date == DpFilter.SelectedDate.Value.Date);
            var list = r.ToList();
            DgAppts.ItemsSource = list;
            TxtCount.Text = $"Showing {list.Count} of {_all.Count} appointments";
        }

        private void OnFilter(object s, object e) => ApplyFilter();

        private void BtnClearDate_Click(object sender, RoutedEventArgs e)
        { DpFilter.SelectedDate = null; ApplyFilter(); }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Appointment a) return;
            a.Status = "Completed";
            DatabaseHelper.UpdateAppointment(a);
            LoadData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            _editing = null;
            FrmTitle.Text = "Schedule Appointment";
            FDonor.ItemsSource = DatabaseHelper.GetDonors(status: "Active");
            FDate.SelectedDate = DateTime.Today;
            ClearForm();
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Appointment a) return;
            _editing = a;
            FrmTitle.Text = "Edit Appointment";
            FDonor.ItemsSource = DatabaseHelper.GetDonors(status: "Active");
            FDonor.SelectedValue = a.DonorId;
            FDate.SelectedDate = a.AppointmentDate;
            FNotes.Text = a.Notes;
            ViewHelper.SetCombo(FTime,    a.AppointmentTime);
            ViewHelper.SetCombo(FPurpose, a.Purpose);
            ViewHelper.SetCombo(FStatus,  a.Status);
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Appointment a) return;
            if (MessageBox.Show($"Delete appointment for '{a.DonorName}' on {a.DateDisplay}?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            DatabaseHelper.DeleteAppointment(a.Id);
            LoadData();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (FDonor.SelectedItem == null)    { ShowErr("Please select a donor."); return; }
            if (FDate.SelectedDate == null)     { ShowErr("Please select a date."); return; }
            if (FDate.SelectedDate.Value < DateTime.Today && _editing == null)
            { ShowErr("Appointment date cannot be in the past."); return; }

            var donor = (Donor)FDonor.SelectedItem;
            var a = new Appointment
            {
                DonorId         = donor.Id,
                DonorName       = donor.FullName,
                BloodType       = donor.BloodType,
                AppointmentDate = FDate.SelectedDate.Value,
                AppointmentTime = ViewHelper.GetCombo(FTime, "09:00"),
                Purpose         = ViewHelper.GetCombo(FPurpose, "Donation"),
                Status          = ViewHelper.GetCombo(FStatus, "Scheduled"),
                Notes           = FNotes.Text.Trim()
            };
            if (_editing != null) { a.Id = _editing.Id; DatabaseHelper.UpdateAppointment(a); }
            else DatabaseHelper.AddAppointment(a);
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ClearForm() { FDonor.SelectedIndex=-1; FNotes.Text=""; FTime.SelectedIndex=2; FPurpose.SelectedIndex=0; FStatus.SelectedIndex=0; FError.Visibility=Visibility.Collapsed; }
        private void ShowErr(string m) { FError.Text=m; FError.Visibility=Visibility.Visible; }
    }
}
