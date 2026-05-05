using BloodBankPro.Database;

namespace BloodBankPro.ViewModels;

public class DashboardViewModel
{
    public DashboardStats LoadStats() => DatabaseHelper.GetDashboardStats();
}
