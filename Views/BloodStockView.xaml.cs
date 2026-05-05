using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class BloodStockView : UserControl
    {
        private List<BloodStock> _all = new();
        private BloodStock? _editing;
        public BloodStockView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();
        private void LoadData() { _all = DatabaseHelper.GetBloodStock(); ApplyFilter(); }

        private void ApplyFilter()
        {
            if (!this.IsLoaded || _all == null) return;
            string bt = ViewHelper.GetCombo(CmbBlood, "All Types");
            string st = ViewHelper.GetCombo(CmbStatus, "All Status");
            var r = _all.AsEnumerable();
            if (bt != "All Types") r = r.Where(x => x.BloodType == bt);
            if (st != "All Status") r = r.Where(x => x.Status == st);
            var list = r.ToList();
            DgStock.ItemsSource = list;
            TxtCount.Text = $"Showing {list.Count} of {_all.Count} entries";
        }

        private void OnFilter(object s, SelectionChangedEventArgs e) => ApplyFilter();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        { _editing = null; FrmTitle.Text = "Add Blood Stock Entry"; ClearForm(); ViewHelper.ShowOverlay(Overlay); }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not BloodStock b) return;
            _editing = b; FrmTitle.Text = "Edit Blood Stock";
            FUnits.Text = b.UnitsAvailable.ToString();
            FSource.Text = b.Source;
            FExpiry.SelectedDate = b.ExpiryDate;
            ViewHelper.SetCombo(FBlood, b.BloodType);
            ViewHelper.SetCombo(FStatus, b.Status);
            ViewHelper.ShowOverlay(Overlay);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not BloodStock b) return;
            if (MessageBox.Show($"Delete {b.BloodType} stock entry ({b.UnitsAvailable} units)?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            DatabaseHelper.DeleteBloodStock(b.Id); LoadData();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FError.Visibility = Visibility.Collapsed;
            if (FBlood.SelectedItem == null) { ShowErr("Select a blood type."); return; }
            if (!int.TryParse(FUnits.Text, out int u) || u < 0) { ShowErr("Enter a valid unit count (0 or more)."); return; }
            var b = new BloodStock
            {
                BloodType = ViewHelper.GetCombo(FBlood), UnitsAvailable = u,
                Source = FSource.Text.Trim(), ExpiryDate = FExpiry.SelectedDate,
                Status = ViewHelper.GetCombo(FStatus, "Available"),
                ReceivedDate = _editing?.ReceivedDate ?? DateTime.Today
            };
            if (_editing != null) { b.Id = _editing.Id; DatabaseHelper.UpdateBloodStock(b); }
            else DatabaseHelper.AddBloodStock(b);
            ViewHelper.HideOverlay(Overlay); LoadData();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e) => ViewHelper.ExportToCsv(DgStock, "BloodStock");
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ViewHelper.HideOverlay(Overlay);
        private void ClearForm() { FUnits.Text=FSource.Text=""; FBlood.SelectedIndex=0; FStatus.SelectedIndex=0; FExpiry.SelectedDate=null; FError.Visibility=Visibility.Collapsed; }
        private void ShowErr(string m) { FError.Text=m; FError.Visibility=Visibility.Visible; }
    }
}
