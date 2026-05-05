using BloodBankPro.Models;

namespace BloodBankPro.Database
{
    public static class Session
    {
        public static User? CurrentUser { get; set; }
        public static bool IsAdmin => CurrentUser?.Role == "Admin";
    }
}
