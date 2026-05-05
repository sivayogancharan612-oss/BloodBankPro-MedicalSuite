using System.Windows;
using System.Windows.Controls;
using BloodBankPro.Database;

namespace BloodBankPro.Views
{
    public partial class CompatibilityView : UserControl
    {
        public CompatibilityView() { InitializeComponent(); LoadChart(); }

        private void LoadChart()
        {
            DonateList.ItemsSource = new[]
            {
                new { Type = "A+",  Info = "→  A+,  AB+" },
                new { Type = "A-",  Info = "→  A+,  A-,  AB+, AB-" },
                new { Type = "B+",  Info = "→  B+,  AB+" },
                new { Type = "B-",  Info = "→  B+,  B-,  AB+, AB-" },
                new { Type = "AB+", Info = "→  AB+  only" },
                new { Type = "AB-", Info = "→  AB+, AB-" },
                new { Type = "O+",  Info = "→  A+,  B+,  O+,  AB+" },
                new { Type = "O-",  Info = "→  All types  ★ Universal Donor" },
            };
            ReceiveList.ItemsSource = new[]
            {
                new { Type = "A+",  Info = "←  A+,  A-,  O+,  O-" },
                new { Type = "A-",  Info = "←  A-,  O-" },
                new { Type = "B+",  Info = "←  B+,  B-,  O+,  O-" },
                new { Type = "B-",  Info = "←  B-,  O-" },
                new { Type = "AB+", Info = "←  All types  ★ Universal Recipient" },
                new { Type = "AB-", Info = "←  A-,  B-,  AB-, O-" },
                new { Type = "O+",  Info = "←  O+,  O-" },
                new { Type = "O-",  Info = "←  O-  only" },
            };
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string recipient = ViewHelper.GetCombo(CmbRecipient, "A+");
            var donors = DatabaseHelper.GetCompatibleDonors(recipient);

            DgCompatible.Visibility = Visibility.Visible;
            DgCompatible.ItemsSource = donors;

            int eligible = donors.Count(d => d.EligibilityStatus == "Eligible");
            if (donors.Count == 0)
                TxtResult.Text = $"⚠  No compatible donors found in the database for blood type {recipient}.";
            else
                TxtResult.Text = $"✅  Found {donors.Count} compatible donor(s) for {recipient} — {eligible} currently eligible to donate.";
        }
    }
}
