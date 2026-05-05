using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;
using BloodBankPro.ViewModels;

namespace BloodBankPro.Views
{
    public class StockSummary
    {
        public string BloodType { get; set; } = string.Empty;
        public int Units { get; set; }
        public double BarWidth { get; set; }
        public string BarColor { get; set; } = string.Empty;
    }

    public partial class DashboardView : UserControl
    {
        private readonly DashboardViewModel _viewModel = new();

        public DashboardView() => InitializeComponent();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtDate.Text = $"Today is {DateTime.Now:dddd, MMMM dd yyyy}  •  {DateTime.Now:HH:mm}";
            AppEvents.DonorsChanged += RefreshDashboard;
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            var stats = _viewModel.LoadStats();
            TxtDonors.Text    = stats.ActiveDonors.ToString();
            TxtUnits.Text     = stats.TotalUnits.ToString();
            TxtPending.Text   = stats.PendingRequests.ToString();
            TxtCritical.Text  = stats.CriticalRequests.ToString();
            TxtHospitals.Text = stats.TotalHospitals.ToString();
            TxtDonations.Text = stats.TotalDonations.ToString();
            TxtAppts.Text     = stats.TodayAppointments.ToString();
            TxtExpiring.Text  = stats.ExpiringItems.ToString();

            const double MaxBar = 220.0;
            int maxUnits = stats.StockLevels.Count > 0 ? Math.Max(stats.StockLevels.Max(x => x.Units), 1) : 1;

            StockBars.ItemsSource = stats.StockLevels.Select(x => new StockSummary
            {
                BloodType = x.BloodType,
                Units = x.Units,
                BarWidth  = (x.Units / (double)maxUnits) * MaxBar,
                BarColor  = x.Units <= 3 ? "#EF5350" : x.Units <= 8 ? "#FFD740" : "#00E676"
            }).ToList();

            var low = DatabaseHelper.GetLowStock(5);
            if (low.Count == 0) TxtNoAlerts.Visibility = Visibility.Visible;
            else LowStockList.ItemsSource = low;

            var critical = DatabaseHelper.GetBloodRequests(urgency: "Critical", status: "Pending");
            if (critical.Count == 0) TxtNoCritical.Visibility = Visibility.Visible;
            else DgCritical.ItemsSource = critical;

            ActivityList.ItemsSource = stats.RecentActivity;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            AppEvents.DonorsChanged -= RefreshDashboard;
        }
    }
}
