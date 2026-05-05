namespace BloodBankPro.Models
{
    using System.ComponentModel;

    public class User
    {
        public int    Id       { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role     { get; set; } = "Staff";
        public string FullName { get; set; } = "";
        public string Email    { get; set; } = "";
        public bool   IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Today;
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
        public string RoleColor => Role == "Admin" ? "#E53935" : "#00BCD4";
    }

    public class Donor : INotifyPropertyChanged
    {
        public int       Id               { get; set; }
        public string    FullName         { get; set; } = "";
        public string    BloodType        { get; set; } = "";
        public string    Phone            { get; set; } = "";
        public string    Email            { get; set; } = "";
        public string    Address          { get; set; } = "";
        public int       Age              { get; set; }
        public string    Gender           { get; set; } = "";
        public double    Weight           { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public string    Status           { get; set; } = "Active";
        public DateTime  RegisteredDate   { get; set; } = DateTime.Today;
        public int       TotalDonations   { get; set; }

        public string EligibilityStatus { get; set; } = "Eligible";
        public string EligibilityColor => EligibilityStatus == "Eligible" ? "#00E676"
                                        : EligibilityStatus.StartsWith("Wait") ? "#FFD740"
                                        : "#90A4AE";
        public string LastDonationDisplay => LastDonationDate?.ToString("yyyy-MM-dd") ?? "Never";
        public string BloodTypeBadge => BloodType;

        private bool _isPhoneRevealed = false;
        public bool IsPhoneRevealed
        {
            get => _isPhoneRevealed;
            set
            {
                _isPhoneRevealed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPhoneRevealed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhone)));
            }
        }

        public string DisplayPhone
        {
            get
            {
                if (IsPhoneRevealed || string.IsNullOrEmpty(Phone))
                    return Phone;

                var masked = Phone.ToCharArray();
                for (int i = 3; i < masked.Length; i++)
                {
                    if (char.IsDigit(masked[i]))
                        masked[i] = 'X';
                }
                return new string(masked);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class BloodStock
    {
        public int       Id             { get; set; }
        public string    BloodType      { get; set; } = "";
        public int       UnitsAvailable { get; set; }
        public DateTime? ExpiryDate     { get; set; }
        public string    Source         { get; set; } = "";
        public DateTime  ReceivedDate   { get; set; } = DateTime.Today;
        public string    Status         { get; set; } = "Available";

        public string ExpiryDisplay => ExpiryDate?.ToString("yyyy-MM-dd") ?? "N/A";
        public int    DaysToExpiry  => ExpiryDate.HasValue ? (ExpiryDate.Value - DateTime.Today).Days : 999;
        public string ExpiryWarning
        {
            get
            {
                if (!ExpiryDate.HasValue) return "";
                int d = DaysToExpiry;
                if (d < 0)   return "⛔ EXPIRED";
                if (d <= 7)  return $"⚠ {d}d left";
                if (d <= 30) return $"📅 {d}d left";
                return "";
            }
        }
        public string ExpiryWarningColor
        {
            get
            {
                int d = DaysToExpiry;
                if (d < 0)  return "#EF5350";
                if (d <= 7) return "#FFD740";
                return "#90A4AE";
            }
        }
        public string StockLevelColor => UnitsAvailable <= 3 ? "#EF5350"
                                       : UnitsAvailable <= 8 ? "#FFD740"
                                       : "#00E676";
    }

    public class Hospital
    {
        public int    Id            { get; set; }
        public string Name          { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string Phone         { get; set; } = "";
        public string Email         { get; set; } = "";
        public string Address       { get; set; } = "";
        public string City          { get; set; } = "";
    }

    public class BloodRequest
    {
        public int      Id           { get; set; }
        public int      HospitalId   { get; set; }
        public string   HospitalName { get; set; } = "";
        public string   PatientName  { get; set; } = "";
        public string   BloodType    { get; set; } = "";
        public int      UnitsNeeded  { get; set; }
        public string   UrgencyLevel { get; set; } = "Medium";
        public string   Status       { get; set; } = "Pending";
        public DateTime RequestDate  { get; set; } = DateTime.Today;
        public string   Notes        { get; set; } = "";
        public string   FulfilledBy  { get; set; } = "";

        public string UrgencyColor => UrgencyLevel switch
        {
            "Critical" => "#EF5350",
            "High"     => "#FFD740",
            "Medium"   => "#00BCD4",
            _          => "#90A4AE"
        };
        public string UrgencyBg => UrgencyLevel switch
        {
            "Critical" => "#3B1A1A",
            "High"     => "#3B2E0A",
            _          => "#111224"
        };
        public string StatusColor => Status switch
        {
            "Fulfilled" => "#00E676",
            "Approved"  => "#00BCD4",
            "Pending"   => "#FFD740",
            "Rejected"  => "#EF5350",
            _           => "#90A4AE"
        };
        public bool CanFulfill => Status == "Pending" || Status == "Approved";
    }

    public class DonationRecord
    {
        public int      Id           { get; set; }
        public int      DonorId      { get; set; }
        public string   DonorName    { get; set; } = "";
        public string   BloodType    { get; set; } = "";
        public int      UnitsDonated { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.Today;
        public string   Notes        { get; set; } = "";
        public string   RecordedBy   { get; set; } = "";
    }

    public class Appointment
    {
        public int      Id              { get; set; }
        public int      DonorId         { get; set; }
        public string   DonorName       { get; set; } = "";
        public string   BloodType       { get; set; } = "";
        public DateTime AppointmentDate { get; set; } = DateTime.Today;
        public string   AppointmentTime { get; set; } = "09:00";
        public string   Purpose         { get; set; } = "Donation";
        public string   Status          { get; set; } = "Scheduled";
        public string   Notes           { get; set; } = "";

        public string DateDisplay => AppointmentDate.ToString("yyyy-MM-dd");
        public bool   IsToday     => AppointmentDate.Date == DateTime.Today;
        public bool   IsOverdue   => AppointmentDate.Date < DateTime.Today && Status == "Scheduled";
        public string StatusColor => Status switch
        {
            "Scheduled"  => "#00BCD4",
            "Completed"  => "#00E676",
            "Cancelled"  => "#90A4AE",
            "NoShow"     => "#EF5350",
            _            => "#90A4AE"
        };
        public string RowHighlight => IsToday ? "#1A2A1A"
                                   : IsOverdue ? "#2A1A1A"
                                   : "#111224";
    }

    public class AuditLog
    {
        public int      Id        { get; set; }
        public string   Action    { get; set; } = "";
        public string   TableName { get; set; } = "";
        public string   Details   { get; set; } = "";
        public string   Username  { get; set; } = "";
        public DateTime LogDate   { get; set; }

        public string LogDateDisplay => LogDate.ToString("yyyy-MM-dd HH:mm:ss");
        public string ActionColor => Action switch
        {
            "INSERT"  => "#00E676",
            "UPDATE"  => "#00BCD4",
            "DELETE"  => "#EF5350",
            "FULFILL" => "#FFD740",
            _         => "#90A4AE"
        };
    }

    public class BloodTypeReport
    {
        public string BloodType         { get; set; } = "";
        public int    TotalDonations    { get; set; }
        public int    UnitsInStock      { get; set; }
        public int    TotalRequests     { get; set; }
        public int    FulfilledRequests { get; set; }
        public int    ActiveDonors      { get; set; }
        public double FulfillRate       => TotalRequests > 0
            ? Math.Round(FulfilledRequests / (double)TotalRequests * 100, 1) : 0;
        public string FulfillRateText   => $"{FulfillRate:F0}%";
        public string FulfillRateColor  => FulfillRate >= 75 ? "#00E676"
                                         : FulfillRate >= 40 ? "#FFD740"
                                         : "#EF5350";
        public string StockColor        => UnitsInStock <= 3 ? "#EF5350"
                                         : UnitsInStock <= 8 ? "#FFD740"
                                         : "#00E676";
    }
}
