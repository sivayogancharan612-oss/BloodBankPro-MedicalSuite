using System.Windows;
using BloodBankPro.Database;
using BloodBankPro.Views;

namespace BloodBankPro
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            try
            {
                DatabaseHelper.Initialize();
                new LoginWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"⚠ Database Connection Failed\n\n{ex.Message}\n\n" +
                    "FIX: Open Command Prompt and run:\n" +
                    "   sqllocaldb start MSSQLLocalDB\n\nThen restart.",
                    "SQL Server LocalDB Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
