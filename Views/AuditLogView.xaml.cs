using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class AuditLogView : UserControl
    {
        private List<AuditLog> _all = new();
        public AuditLogView() => InitializeComponent();
        private void OnLoaded(object sender, RoutedEventArgs e) => LoadData();

        private void LoadData()
        {
            _all = DatabaseHelper.GetAuditLogs();
            DgLogs.ItemsSource = _all;
            TxtCount.Text = $"{_all.Count} log entries";
        }

        private void OnSearch(object sender, TextChangedEventArgs e)
        {
            var q = TxtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(q)) { DgLogs.ItemsSource = _all; return; }
            var filtered = _all.Where(l =>
                l.Details.ToLower().Contains(q) ||
                l.Username.ToLower().Contains(q) ||
                l.TableName.ToLower().Contains(q) ||
                l.Action.ToLower().Contains(q)).ToList();
            DgLogs.ItemsSource = filtered;
            TxtCount.Text = $"Showing {filtered.Count} of {_all.Count} entries";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadData();
        private void BtnExport_Click(object sender, RoutedEventArgs e) => ViewHelper.ExportToCsv(DgLogs, "AuditLog");
    }
}
