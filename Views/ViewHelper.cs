using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BloodBankPro.Views
{
    public static class ViewHelper
    {
        public static void SetCombo(ComboBox cb, string value)
        {
            foreach (ComboBoxItem item in cb.Items)
                if (item.Content?.ToString() == value)
                { cb.SelectedItem = item; return; }
        }

        public static string GetCombo(ComboBox cb, string fallback = "")
            => cb?.SelectedItem is ComboBoxItem item ? item.Content?.ToString() ?? fallback : fallback;

        public static void ShowOverlay(Grid overlay) => overlay.Visibility = Visibility.Visible;
        public static void HideOverlay(Grid overlay) => overlay.Visibility = Visibility.Collapsed;

        public static void ExportToCsv(DataGrid dg, string title)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    FileName = $"{title.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.csv",
                    Filter   = "CSV File|*.csv",
                    Title    = $"Export {title} to CSV"
                };
                if (dlg.ShowDialog() != true) return;

                var sb = new StringBuilder();

                var textCols = dg.Columns
                    .OfType<DataGridTextColumn>()
                    .Where(c => c.Header != null)
                    .ToList();

                sb.AppendLine(string.Join(",", textCols.Select(c => $"\"{c.Header}\"")));

                foreach (var item in dg.Items)
                {
                    if (item == null) continue;
                    var row = new List<string>();
                    foreach (var col in textCols)
                    {
                        var binding = (col.Binding as System.Windows.Data.Binding);
                        if (binding == null) { row.Add(""); continue; }
                        var prop = item.GetType().GetProperty(binding.Path.Path);
                        var val  = prop?.GetValue(item)?.ToString() ?? "";
                        row.Add($"\"{val.Replace("\"", "\"\"")}\"");
                    }
                    sb.AppendLine(string.Join(",", row));
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"✅ Exported {dg.Items.Count} records to:\n{dlg.FileName}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
