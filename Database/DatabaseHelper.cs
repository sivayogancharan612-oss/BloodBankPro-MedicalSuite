using Microsoft.Data.SqlClient;
using BloodBankPro.Models;
using BloodBankPro.ViewModels;

namespace BloodBankPro.Database
{
    public static class DatabaseHelper
    {
        private const string ConnStr =
            @"Server=(localdb)\MSSQLLocalDB;Database=BloodBankProDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection() => new SqlConnection(ConnStr);

        public static void Initialize()
        {
            var masterConnStr = ConnStr.Replace("Database=BloodBankProDB;", "");
            using (var masterCon = new SqlConnection(masterConnStr))
            {
                masterCon.Open();
                Exec(masterCon, "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BloodBankProDB') CREATE DATABASE BloodBankProDB;");
            }

            using var con = GetConnection(); con.Open();
            Exec(con, @"
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users(
                    Id          INT IDENTITY(1,1) PRIMARY KEY,
                    Username    NVARCHAR(100) NOT NULL UNIQUE,
                    Password    NVARCHAR(100) NOT NULL,
                    Role        NVARCHAR(50)  NOT NULL DEFAULT 'Staff',
                    FullName    NVARCHAR(200) NOT NULL,
                    Email       NVARCHAR(200) NULL,
                    IsActive    BIT           NOT NULL DEFAULT 1,
                    CreatedDate DATE          NOT NULL DEFAULT GETDATE()
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='Donors' AND xtype='U')
                CREATE TABLE Donors(
                    Id               INT IDENTITY(1,1) PRIMARY KEY,
                    FullName         NVARCHAR(200) NOT NULL,
                    BloodType        NVARCHAR(5)   NOT NULL,
                    Phone            NVARCHAR(50)  NULL,
                    Email            NVARCHAR(200) NULL,
                    Address          NVARCHAR(300) NULL,
                    Age              INT           NOT NULL DEFAULT 0,
                    Gender           NVARCHAR(10)  NULL,
                    Weight           DECIMAL(5,1)  NOT NULL DEFAULT 0,
                    LastDonationDate DATE          NULL,
                    Status           NVARCHAR(20)  NOT NULL DEFAULT 'Active',
                    RegisteredDate   DATE          NOT NULL DEFAULT GETDATE()
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='BloodStock' AND xtype='U')
                CREATE TABLE BloodStock(
                    Id             INT IDENTITY(1,1) PRIMARY KEY,
                    BloodType      NVARCHAR(5)   NOT NULL,
                    UnitsAvailable INT           NOT NULL DEFAULT 0,
                    ExpiryDate     DATE          NULL,
                    Source         NVARCHAR(200) NULL,
                    ReceivedDate   DATE          NOT NULL DEFAULT GETDATE(),
                    Status         NVARCHAR(20)  NOT NULL DEFAULT 'Available'
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='Hospitals' AND xtype='U')
                CREATE TABLE Hospitals(
                    Id            INT IDENTITY(1,1) PRIMARY KEY,
                    Name          NVARCHAR(200) NOT NULL,
                    ContactPerson NVARCHAR(200) NULL,
                    Phone         NVARCHAR(50)  NULL,
                    Email         NVARCHAR(200) NULL,
                    Address       NVARCHAR(300) NULL,
                    City          NVARCHAR(100) NULL
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='BloodRequests' AND xtype='U')
                CREATE TABLE BloodRequests(
                    Id           INT IDENTITY(1,1) PRIMARY KEY,
                    HospitalId   INT           NOT NULL,
                    PatientName  NVARCHAR(200) NOT NULL,
                    BloodType    NVARCHAR(5)   NOT NULL,
                    UnitsNeeded  INT           NOT NULL DEFAULT 1,
                    UrgencyLevel NVARCHAR(20)  NOT NULL DEFAULT 'Medium',
                    Status       NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
                    RequestDate  DATE          NOT NULL DEFAULT GETDATE(),
                    Notes        NVARCHAR(500) NULL,
                    FulfilledBy  NVARCHAR(100) NULL,
                    CONSTRAINT FK_Req_Hosp FOREIGN KEY(HospitalId) REFERENCES Hospitals(Id)
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='DonationRecords' AND xtype='U')
                CREATE TABLE DonationRecords(
                    Id           INT IDENTITY(1,1) PRIMARY KEY,
                    DonorId      INT           NOT NULL,
                    BloodType    NVARCHAR(5)   NOT NULL,
                    UnitsDonated INT           NOT NULL DEFAULT 1,
                    DonationDate DATE          NOT NULL DEFAULT GETDATE(),
                    Notes        NVARCHAR(500) NULL,
                    RecordedBy   NVARCHAR(100) NULL,
                    CONSTRAINT FK_Don_Donor FOREIGN KEY(DonorId) REFERENCES Donors(Id)
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='Appointments' AND xtype='U')
                CREATE TABLE Appointments(
                    Id              INT IDENTITY(1,1) PRIMARY KEY,
                    DonorId         INT           NOT NULL,
                    AppointmentDate DATE          NOT NULL,
                    AppointmentTime NVARCHAR(10)  NOT NULL DEFAULT '09:00',
                    Purpose         NVARCHAR(50)  NOT NULL DEFAULT 'Donation',
                    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Scheduled',
                    Notes           NVARCHAR(500) NULL,
                    CONSTRAINT FK_Appt_Donor FOREIGN KEY(DonorId) REFERENCES Donors(Id)
                );
                IF NOT EXISTS(SELECT 1 FROM sysobjects WHERE name='AuditLogs' AND xtype='U')
                CREATE TABLE AuditLogs(
                    Id        INT IDENTITY(1,1) PRIMARY KEY,
                    Action    NVARCHAR(20)  NOT NULL,
                    TableName NVARCHAR(50)  NOT NULL,
                    Details   NVARCHAR(500) NOT NULL,
                    Username  NVARCHAR(100) NOT NULL,
                    LogDate   DATETIME      NOT NULL DEFAULT GETDATE()
                );
            ");
            SeedData(con);
        }

        static void SeedData(SqlConnection con)
        {
            if (Scalar<int>(con, "SELECT COUNT(*) FROM Users") > 0) return;
            Exec(con, @"
                INSERT INTO Users(Username,Password,Role,FullName,Email,IsActive,CreatedDate) VALUES
                ('admin','admin123','Admin','Dr. System Admin','admin@bloodbank.lk',1,GETDATE()),
                ('staff','staff123','Staff','Lab Technician','staff@bloodbank.lk',1,GETDATE()),
                ('nurse','nurse123','Staff','Head Nurse','nurse@bloodbank.lk',1,GETDATE());

                INSERT INTO Donors(FullName,BloodType,Phone,Email,Address,Age,Gender,Weight,LastDonationDate,Status,RegisteredDate) VALUES
                ('Kasun Perera','A+','077-1111111','kasun@mail.com','No 12, Galle Rd, Colombo 3',28,'Male',72.5,'2025-10-15','Active','2023-05-10'),
                ('Nimal Silva','O+','071-2222222','nimal@mail.com','No 45, Kandy Rd, Kandy',35,'Male',80.0,'2025-08-20','Active','2022-08-01'),
                ('Priya Fernando','B+','076-3333333','priya@mail.com','No 8, High Level Rd, Nugegoda',24,'Female',58.0,'2025-11-01','Active','2024-01-15'),
                ('Ahmed Farook','AB+','070-4444444','ahmed@mail.com','No 22, Union Pl, Colombo 2',42,'Male',75.5,NULL,'Active','2024-03-20'),
                ('Sandya Kumari','O-','075-5555555','sandya@mail.com','No 3, Matara Rd, Galle',30,'Female',55.5,'2025-09-10','Active','2023-11-05'),
                ('Ruwan Jayawardena','A-','072-6666666','ruwan@mail.com','No 17, Beach Rd, Matara',38,'Male',68.0,NULL,'Inactive','2022-06-18'),
                ('Fathima Nazar','B-','074-7777777','fathima@mail.com','No 5, Maradana Rd, Colombo 10',26,'Female',52.5,'2025-12-01','Active','2025-01-10'),
                ('Chaminda Bandara','AB-','078-8888888','chaminda@mail.com','No 33, Peradeniya Rd, Kandy',31,'Male',70.0,'2025-07-15','Active','2023-09-22'),
                ('Sampath Rathnayake','O+','077-9090909','sampath@mail.com','No 7, Baseline Rd, Colombo 9',29,'Male',77.0,NULL,'Active','2025-03-01'),
                ('Dilani Jayasuriya','A+','076-0101010','dilani@mail.com','No 19, High St, Dehiwala',27,'Female',60.0,'2025-06-10','Active','2024-07-20');

                INSERT INTO Hospitals(Name,ContactPerson,Phone,Email,Address,City) VALUES
                ('National Hospital of Sri Lanka','Dr. K. Perera','011-1111111','national@hospital.lk','Regent St, Colombo 7','Colombo'),
                ('Asiri Medical Centre','Dr. R. Silva','011-2222222','asiri@hospital.lk','181 Kirula Rd, Colombo 5','Colombo'),
                ('Kandy Teaching Hospital','Dr. S. Fernando','081-3333333','kandy@hospital.lk','Hospital Rd, Kandy','Kandy'),
                ('Lanka Hospitals Corporation','Dr. M. Nimal','011-4444444','lanka@hospital.lk','578 Elvitigala Mawatha, Colombo 5','Colombo'),
                ('Galle Base Hospital','Dr. P. Ananda','091-5555555','galle@hospital.lk','Wakwella Rd, Galle','Galle');

                INSERT INTO BloodStock(BloodType,UnitsAvailable,ExpiryDate,Source,ReceivedDate,Status) VALUES
                ('A+',15,'2026-02-15','Kasun Perera','2025-12-01','Available'),
                ('O+',22,'2026-01-20','Blood Drive - Colombo University','2025-11-15','Available'),
                ('B+',8,'2026-03-10','Priya Fernando','2025-12-05','Available'),
                ('AB+',5,'2026-02-28','Ahmed Farook','2025-12-10','Available'),
                ('O-',3,'2025-12-25','Sandya Kumari','2025-10-20','Available'),
                ('A-',2,'2025-12-30','Ruwan Jayawardena','2025-11-01','Available'),
                ('B-',6,'2026-01-15','Fathima Nazar','2025-12-01','Available'),
                ('AB-',1,'2025-12-22','Chaminda Bandara','2025-10-15','Available'),
                ('O+',10,'2026-04-10','Blood Drive - Kelaniya','2025-12-15','Available'),
                ('A+',8,'2026-03-20','Dilani Jayasuriya','2025-12-18','Available');

                INSERT INTO BloodRequests(HospitalId,PatientName,BloodType,UnitsNeeded,UrgencyLevel,Status,RequestDate,Notes) VALUES
                (1,'John Doe','O+',2,'Critical','Pending','2025-12-19','Emergency surgery - accident victim'),
                (2,'Mary Silva','A+',1,'High','Approved','2025-12-18','Pre-op transfusion'),
                (3,'Sunil Perera','B+',3,'Medium','Pending','2025-12-17','Scheduled cardiac operation'),
                (1,'Aisha Farook','AB+',1,'Low','Fulfilled','2025-12-15','Routine transfusion'),
                (4,'Priya Nair','O-',2,'Critical','Pending','2025-12-19','Rare type - maternity emergency'),
                (2,'Ravi Kumar','A-',1,'Medium','Approved','2025-12-18','Thalassemia patient'),
                (5,'Dinesh Perera','B-',2,'High','Pending','2025-12-19','Road accident victim');

                INSERT INTO DonationRecords(DonorId,BloodType,UnitsDonated,DonationDate,Notes,RecordedBy) VALUES
                (1,'A+',1,'2025-10-15','Regular quarterly donation','admin'),
                (2,'O+',1,'2025-08-20','Walk-in donor','staff'),
                (3,'B+',1,'2025-11-01','Referred by Asiri Medical','staff'),
                (5,'O-',1,'2025-09-10','Priority - rare blood type','admin'),
                (7,'B-',1,'2025-12-01','Regular donor - 5th donation','staff'),
                (8,'AB-',1,'2025-07-15','First-time donor','nurse'),
                (1,'A+',1,'2025-04-10','Previous donation','admin'),
                (2,'O+',1,'2025-02-20','Previous donation','staff');

                INSERT INTO Appointments(DonorId,AppointmentDate,AppointmentTime,Purpose,Status,Notes) VALUES
                (1,'2025-12-20','09:00','Donation','Scheduled','Regular quarterly donation'),
                (4,'2025-12-20','10:30','Donation','Scheduled','First time - needs orientation'),
                (9,'2025-12-21','09:00','Donation','Scheduled',''),
                (3,'2025-12-19','14:00','CheckUp','Completed','Pre-donation health check passed'),
                (2,'2025-12-18','11:00','Donation','Completed','Successfully donated O+'),
                (6,'2025-12-15','09:00','Donation','NoShow','Donor did not arrive');

                INSERT INTO AuditLogs(Action,TableName,Details,Username,LogDate) VALUES
                ('INSERT','Donors','Added new donor: Kasun Perera (A+)','admin',GETDATE()),
                ('INSERT','BloodStock','Added 15 units of A+ blood','admin',GETDATE()),
                ('FULFILL','BloodRequests','Fulfilled request for Aisha Farook (AB+, 1 unit) - National Hospital','admin',GETDATE()),
                ('INSERT','Appointments','Scheduled appointment for Kasun Perera on 2025-12-20','staff',GETDATE());
            ");
        }

        static void Exec(SqlConnection con, string sql)
        {
            using var cmd = new SqlCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        static T Scalar<T>(SqlConnection con, string sql)
        {
            using var cmd = new SqlCommand(sql, con);
            return (T)cmd.ExecuteScalar()!;
        }

        public static void Log(string action, string table, string details)
        {
            try
            {
                using var con = GetConnection(); con.Open();
                using var cmd = new SqlCommand(
                    "INSERT INTO AuditLogs(Action,TableName,Details,Username,LogDate) VALUES(@a,@t,@d,@u,GETDATE())", con);
                cmd.Parameters.AddWithValue("@a", action);
                cmd.Parameters.AddWithValue("@t", table);
                cmd.Parameters.AddWithValue("@d", details);
                cmd.Parameters.AddWithValue("@u", Session.CurrentUser?.Username ?? "system");
                cmd.ExecuteNonQuery();
            }
            catch { /* never crash app on audit failure */ }
        }

        public static List<AuditLog> GetAuditLogs(string? search = null)
        {
            var list = new List<AuditLog>();
            using var con = GetConnection(); con.Open();
            var sql = "SELECT Id,Action,TableName,Details,Username,LogDate FROM AuditLogs WHERE 1=1";
            if (!string.IsNullOrEmpty(search)) sql += " AND (Details LIKE @q OR Username LIKE @q OR TableName LIKE @q)";
            sql += " ORDER BY LogDate DESC";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@q", $"%{search}%");
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new AuditLog { Id=r.GetInt32(0), Action=r.GetString(1), TableName=r.GetString(2),
                    Details=r.GetString(3), Username=r.GetString(4), LogDate=r.GetDateTime(5) });
            return list;
        }

        public static User? Login(string username, string password)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id,Username,Password,Role,FullName,Email,IsActive,CreatedDate FROM Users WHERE Username=@u AND Password=@p AND IsActive=1", con);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return MapUser(r);
        }

        public static List<User> GetUsers()
        {
            var list = new List<User>();
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id,Username,Password,Role,FullName,Email,IsActive,CreatedDate FROM Users ORDER BY Role,FullName", con);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        public static void AddUser(User u)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Users(Username,Password,Role,FullName,Email,IsActive,CreatedDate) VALUES(@un,@pw,@ro,@fn,@em,@ac,GETDATE())", con);
            cmd.Parameters.AddWithValue("@un", u.Username);
            cmd.Parameters.AddWithValue("@pw", u.Password);
            cmd.Parameters.AddWithValue("@ro", u.Role);
            cmd.Parameters.AddWithValue("@fn", u.FullName);
            cmd.Parameters.AddWithValue("@em", (object?)u.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ac", u.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
            Log("INSERT", "Users", $"Created user: {u.Username} ({u.Role})");
        }

        public static void UpdateUser(User u)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "UPDATE Users SET Password=@pw,Role=@ro,FullName=@fn,Email=@em,IsActive=@ac WHERE Id=@id", con);
            cmd.Parameters.AddWithValue("@pw", u.Password);
            cmd.Parameters.AddWithValue("@ro", u.Role);
            cmd.Parameters.AddWithValue("@fn", u.FullName);
            cmd.Parameters.AddWithValue("@em", (object?)u.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ac", u.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", u.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "Users", $"Updated user: {u.Username}");
        }

        public static void DeleteUser(int id, string username)
        {
            SimpleDelete("Users", id);
            Log("DELETE", "Users", $"Deleted user: {username}");
        }

        static User MapUser(SqlDataReader r) => new User
        {
            Id=r.GetInt32(0), Username=r.GetString(1), Password=r.GetString(2),
            Role=r.GetString(3), FullName=r.GetString(4),
            Email=r.IsDBNull(5)?"":r.GetString(5),
            IsActive=r.GetBoolean(6), CreatedDate=r.GetDateTime(7)
        };

        public static List<Donor> GetDonors(string? bloodType=null, string? status=null, string? search=null)
        {
            var list = new List<Donor>();
            using var con = GetConnection(); con.Open();
            var sql = @"SELECT d.Id,d.FullName,d.BloodType,d.Phone,d.Email,d.Address,d.Age,d.Gender,
                               d.Weight,d.LastDonationDate,d.Status,d.RegisteredDate,
                               (SELECT COUNT(*) FROM DonationRecords dr WHERE dr.DonorId=d.Id) AS TotalDonations,
                               CASE 
                                   WHEN d.Status = 'Inactive' THEN 'Inactive' 
                                   WHEN d.LastDonationDate IS NULL THEN 'Eligible' 
                                   WHEN DATEDIFF(day, d.LastDonationDate, GETDATE()) >= 56 THEN 'Eligible' 
                                   ELSE 'Wait ' + CAST(56 - DATEDIFF(day, d.LastDonationDate, GETDATE()) AS VARCHAR) + 'd' 
                               END
                        FROM Donors d WHERE 1=1";
            if (!string.IsNullOrEmpty(bloodType)) sql += " AND d.BloodType=@bt";
            if (!string.IsNullOrEmpty(status))    sql += " AND d.Status=@st";
            if (!string.IsNullOrEmpty(search))    sql += " AND (d.FullName LIKE @q OR d.Phone LIKE @q OR d.Email LIKE @q)";
            sql += " ORDER BY d.FullName";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(bloodType)) cmd.Parameters.AddWithValue("@bt", bloodType);
            if (!string.IsNullOrEmpty(status))    cmd.Parameters.AddWithValue("@st", status);
            if (!string.IsNullOrEmpty(search))    cmd.Parameters.AddWithValue("@q", $"%{search}%");
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Donor {
                    Id=r.GetInt32(0), FullName=r.GetString(1), BloodType=r.GetString(2),
                    Phone=r.IsDBNull(3)?"":r.GetString(3), Email=r.IsDBNull(4)?"":r.GetString(4),
                    Address=r.IsDBNull(5)?"":r.GetString(5), Age=r.GetInt32(6),
                    Gender=r.IsDBNull(7)?"":r.GetString(7),
                    Weight=r.IsDBNull(8)?0:double.Parse(r.GetDecimal(8).ToString()),
                    LastDonationDate=r.IsDBNull(9)?null:r.GetDateTime(9),
                    Status=r.GetString(10), RegisteredDate=r.GetDateTime(11),
                    TotalDonations=r.GetInt32(12),
                    EligibilityStatus=r.GetString(13)
                });
            return list;
        }

        public static void AddDonor(Donor d)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Donors(FullName,BloodType,Phone,Email,Address,Age,Gender,Weight,LastDonationDate,Status,RegisteredDate)
                VALUES(@fn,@bt,@ph,@em,@ad,@ag,@gen,@wt,@ld,@st,@rd)", con);
            SetDonorParams(cmd, d);
            cmd.Parameters.AddWithValue("@rd", d.RegisteredDate.Date);
            cmd.ExecuteNonQuery();
            Log("INSERT", "Donors", $"Added donor: {d.FullName} ({d.BloodType})");
            AppEvents.RaiseDonorsChanged();
        }

        public static void UpdateDonor(Donor d)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Donors SET FullName=@fn,BloodType=@bt,Phone=@ph,Email=@em,
                Address=@ad,Age=@ag,Gender=@gen,Weight=@wt,LastDonationDate=@ld,Status=@st WHERE Id=@id", con);
            SetDonorParams(cmd, d);
            cmd.Parameters.AddWithValue("@id", d.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "Donors", $"Updated donor: {d.FullName} (ID {d.Id})");
            AppEvents.RaiseDonorsChanged();
        }

        static void SetDonorParams(SqlCommand cmd, Donor d)
        {
            cmd.Parameters.AddWithValue("@fn", d.FullName);
            cmd.Parameters.AddWithValue("@bt", d.BloodType);
            cmd.Parameters.AddWithValue("@ph", (object?)d.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@em", (object?)d.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ad", (object?)d.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ag", d.Age);
            cmd.Parameters.AddWithValue("@gen", (object?)d.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@wt", d.Weight);
            cmd.Parameters.AddWithValue("@ld", d.LastDonationDate.HasValue?(object)d.LastDonationDate.Value:DBNull.Value);
            cmd.Parameters.AddWithValue("@st", d.Status);
        }

        public static void DeleteDonor(int id, string name)
        {
            SimpleDelete("Donors", id);
            Log("DELETE", "Donors", $"Deleted donor: {name} (ID {id})");
            AppEvents.RaiseDonorsChanged();
        }

        public static List<BloodStock> GetBloodStock(string? bloodType=null, string? status=null)
        {
            var list = new List<BloodStock>();
            using var con = GetConnection(); con.Open();
            var sql = "SELECT Id,BloodType,UnitsAvailable,ExpiryDate,Source,ReceivedDate,Status FROM BloodStock WHERE 1=1";
            if (!string.IsNullOrEmpty(bloodType)) sql += " AND BloodType=@bt";
            if (!string.IsNullOrEmpty(status))    sql += " AND Status=@st";
            sql += " ORDER BY BloodType,ExpiryDate";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(bloodType)) cmd.Parameters.AddWithValue("@bt", bloodType);
            if (!string.IsNullOrEmpty(status))    cmd.Parameters.AddWithValue("@st", status);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new BloodStock {
                    Id=r.GetInt32(0), BloodType=r.GetString(1), UnitsAvailable=r.GetInt32(2),
                    ExpiryDate=r.IsDBNull(3)?null:r.GetDateTime(3),
                    Source=r.IsDBNull(4)?"":r.GetString(4),
                    ReceivedDate=r.GetDateTime(5), Status=r.GetString(6)
                });
            return list;
        }

        public static void AddBloodStock(BloodStock b)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO BloodStock(BloodType,UnitsAvailable,ExpiryDate,Source,ReceivedDate,Status) VALUES(@bt,@un,@ex,@src,@rd,@st)", con);
            SetStockParams(cmd, b);
            cmd.Parameters.AddWithValue("@rd", b.ReceivedDate.Date);
            cmd.ExecuteNonQuery();
            Log("INSERT", "BloodStock", $"Added {b.UnitsAvailable} units of {b.BloodType} from {b.Source}");
        }

        public static void UpdateBloodStock(BloodStock b)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "UPDATE BloodStock SET BloodType=@bt,UnitsAvailable=@un,ExpiryDate=@ex,Source=@src,Status=@st WHERE Id=@id", con);
            SetStockParams(cmd, b);
            cmd.Parameters.AddWithValue("@id", b.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "BloodStock", $"Updated stock ID {b.Id}: {b.BloodType} - {b.UnitsAvailable} units");
        }

        static void SetStockParams(SqlCommand cmd, BloodStock b)
        {
            cmd.Parameters.AddWithValue("@bt", b.BloodType);
            cmd.Parameters.AddWithValue("@un", b.UnitsAvailable);
            cmd.Parameters.AddWithValue("@ex", b.ExpiryDate.HasValue?(object)b.ExpiryDate.Value:DBNull.Value);
            cmd.Parameters.AddWithValue("@src", (object?)b.Source ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@st", b.Status);
        }

        public static void DeleteBloodStock(int id)
        {
            SimpleDelete("BloodStock", id);
            Log("DELETE", "BloodStock", $"Deleted stock entry ID {id}");
        }

        public static List<Hospital> GetHospitals(string? search=null)
        {
            var list = new List<Hospital>();
            using var con = GetConnection(); con.Open();
            var sql = "SELECT Id,Name,ContactPerson,Phone,Email,Address,City FROM Hospitals WHERE 1=1";
            if (!string.IsNullOrEmpty(search)) sql += " AND (Name LIKE @q OR City LIKE @q)";
            sql += " ORDER BY Name";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@q", $"%{search}%");
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Hospital {
                    Id=r.GetInt32(0), Name=r.GetString(1),
                    ContactPerson=r.IsDBNull(2)?"":r.GetString(2), Phone=r.IsDBNull(3)?"":r.GetString(3),
                    Email=r.IsDBNull(4)?"":r.GetString(4), Address=r.IsDBNull(5)?"":r.GetString(5),
                    City=r.IsDBNull(6)?"":r.GetString(6)
                });
            return list;
        }

        public static void AddHospital(Hospital h)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Hospitals(Name,ContactPerson,Phone,Email,Address,City) VALUES(@n,@cp,@ph,@em,@ad,@ct)", con);
            SetHospitalParams(cmd, h); cmd.ExecuteNonQuery();
            Log("INSERT", "Hospitals", $"Added hospital: {h.Name}, {h.City}");
        }

        public static void UpdateHospital(Hospital h)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "UPDATE Hospitals SET Name=@n,ContactPerson=@cp,Phone=@ph,Email=@em,Address=@ad,City=@ct WHERE Id=@id", con);
            SetHospitalParams(cmd, h);
            cmd.Parameters.AddWithValue("@id", h.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "Hospitals", $"Updated hospital: {h.Name}");
        }

        static void SetHospitalParams(SqlCommand cmd, Hospital h)
        {
            cmd.Parameters.AddWithValue("@n", h.Name);
            cmd.Parameters.AddWithValue("@cp", (object?)h.ContactPerson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ph", (object?)h.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@em", (object?)h.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ad", (object?)h.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ct", (object?)h.City ?? DBNull.Value);
        }

        public static void DeleteHospital(int id, string name)
        {
            SimpleDelete("Hospitals", id);
            Log("DELETE", "Hospitals", $"Deleted hospital: {name}");
        }

        public static List<BloodRequest> GetBloodRequests(string? bt=null, string? urgency=null, string? status=null, string? search=null)
        {
            var list = new List<BloodRequest>();
            using var con = GetConnection(); con.Open();
            var sql = @"SELECT r.Id,r.HospitalId,r.PatientName,r.BloodType,r.UnitsNeeded,
                               r.UrgencyLevel,r.Status,r.RequestDate,ISNULL(r.Notes,''),
                               ISNULL(r.FulfilledBy,''),h.Name
                        FROM BloodRequests r LEFT JOIN Hospitals h ON r.HospitalId=h.Id WHERE 1=1";
            if (!string.IsNullOrEmpty(bt))     sql += " AND r.BloodType=@bt";
            if (!string.IsNullOrEmpty(urgency)) sql += " AND r.UrgencyLevel=@urg";
            if (!string.IsNullOrEmpty(status))  sql += " AND r.Status=@st";
            if (!string.IsNullOrEmpty(search))  sql += " AND (r.PatientName LIKE @q OR h.Name LIKE @q)";
            sql += " ORDER BY CASE r.UrgencyLevel WHEN 'Critical' THEN 1 WHEN 'High' THEN 2 WHEN 'Medium' THEN 3 ELSE 4 END, r.RequestDate DESC";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(bt))     cmd.Parameters.AddWithValue("@bt", bt);
            if (!string.IsNullOrEmpty(urgency)) cmd.Parameters.AddWithValue("@urg", urgency);
            if (!string.IsNullOrEmpty(status))  cmd.Parameters.AddWithValue("@st", status);
            if (!string.IsNullOrEmpty(search))  cmd.Parameters.AddWithValue("@q", $"%{search}%");
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new BloodRequest {
                    Id=r.GetInt32(0), HospitalId=r.GetInt32(1), PatientName=r.GetString(2),
                    BloodType=r.GetString(3), UnitsNeeded=r.GetInt32(4), UrgencyLevel=r.GetString(5),
                    Status=r.GetString(6), RequestDate=r.GetDateTime(7), Notes=r.GetString(8),
                    FulfilledBy=r.GetString(9), HospitalName=r.IsDBNull(10)?"":r.GetString(10)
                });
            return list;
        }

        public static void AddBloodRequest(BloodRequest req)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO BloodRequests(HospitalId,PatientName,BloodType,UnitsNeeded,UrgencyLevel,Status,RequestDate,Notes)
                VALUES(@hid,@pn,@bt,@un,@urg,@st,@rd,@notes)", con);
            SetRequestParams(cmd, req);
            cmd.Parameters.AddWithValue("@rd", req.RequestDate.Date);
            cmd.ExecuteNonQuery();
            Log("INSERT", "BloodRequests", $"New request: {req.PatientName} needs {req.UnitsNeeded} units {req.BloodType} ({req.UrgencyLevel})");
        }

        public static void UpdateBloodRequest(BloodRequest req)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                UPDATE BloodRequests SET HospitalId=@hid,PatientName=@pn,BloodType=@bt,
                UnitsNeeded=@un,UrgencyLevel=@urg,Status=@st,Notes=@notes WHERE Id=@id", con);
            SetRequestParams(cmd, req);
            cmd.Parameters.AddWithValue("@id", req.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "BloodRequests", $"Updated request for {req.PatientName} - Status: {req.Status}");
        }

        static void SetRequestParams(SqlCommand cmd, BloodRequest req)
        {
            cmd.Parameters.AddWithValue("@hid", req.HospitalId);
            cmd.Parameters.AddWithValue("@pn", req.PatientName);
            cmd.Parameters.AddWithValue("@bt", req.BloodType);
            cmd.Parameters.AddWithValue("@un", req.UnitsNeeded);
            cmd.Parameters.AddWithValue("@urg", req.UrgencyLevel);
            cmd.Parameters.AddWithValue("@st", req.Status);
            cmd.Parameters.AddWithValue("@notes", (object?)req.Notes ?? DBNull.Value);
        }

        public static void DeleteBloodRequest(int id, string patient)
        {
            SimpleDelete("BloodRequests", id);
            Log("DELETE", "BloodRequests", $"Deleted request for patient: {patient}");
        }

        public static (bool success, string message) FulfillRequest(BloodRequest req)
        {
            using var con = GetConnection(); con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                using var stockCmd = new SqlCommand(@"
                    SELECT Id, UnitsAvailable FROM BloodStock
                    WHERE BloodType=@bt AND Status='Available' AND UnitsAvailable>0
                    ORDER BY ExpiryDate ASC", con, tx);
                stockCmd.Parameters.AddWithValue("@bt", req.BloodType);
                var stocks = new List<(int Id, int Units)>();
                using (var r = stockCmd.ExecuteReader())
                    while (r.Read()) stocks.Add((r.GetInt32(0), r.GetInt32(1)));

                int totalAvailable = stocks.Sum(s => s.Units);
                if (totalAvailable < req.UnitsNeeded)
                {
                    tx.Rollback();
                    return (false, $"❌ Not enough {req.BloodType} blood in stock!\n\nAvailable: {totalAvailable} units\nNeeded: {req.UnitsNeeded} units\n\nAdd more blood stock first.");
                }

                int remaining = req.UnitsNeeded;
                foreach (var (id, units) in stocks)
                {
                    if (remaining <= 0) break;
                    int deduct = Math.Min(remaining, units);
                    using var upd = new SqlCommand(
                        "UPDATE BloodStock SET UnitsAvailable=UnitsAvailable-@d WHERE Id=@id", con, tx);
                    upd.Parameters.AddWithValue("@d", deduct);
                    upd.Parameters.AddWithValue("@id", id);
                    upd.ExecuteNonQuery();
                    using var chk = new SqlCommand(
                        "UPDATE BloodStock SET Status='Used' WHERE Id=@id AND UnitsAvailable=0", con, tx);
                    chk.Parameters.AddWithValue("@id", id);
                    chk.ExecuteNonQuery();
                    remaining -= deduct;
                }

                using var fulfill = new SqlCommand(
                    "UPDATE BloodRequests SET Status='Fulfilled',FulfilledBy=@by WHERE Id=@id", con, tx);
                fulfill.Parameters.AddWithValue("@by", Session.CurrentUser?.Username ?? "system");
                fulfill.Parameters.AddWithValue("@id", req.Id);
                fulfill.ExecuteNonQuery();

                tx.Commit();
                Log("FULFILL", "BloodRequests",
                    $"Fulfilled {req.UnitsNeeded} units of {req.BloodType} for {req.PatientName} at {req.HospitalName}");
                return (true, $"✅ Request fulfilled successfully!\n\n{req.UnitsNeeded} units of {req.BloodType} deducted from stock (FIFO).\nFulfilled by: {Session.CurrentUser?.Username}");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, $"❌ Fulfillment failed: {ex.Message}");
            }
        }

        public static (bool ok, string error, SqlConnection? con, SqlTransaction? tx)
            BeginFulfillTransaction(BloodRequest req)
        {
            var con = GetConnection();
            con.Open();
            var tx = con.BeginTransaction();
            try
            {
                using var stockCmd = new SqlCommand(@"
                    SELECT Id, UnitsAvailable FROM BloodStock
                    WHERE BloodType=@bt AND Status='Available' AND UnitsAvailable>0
                    ORDER BY ExpiryDate ASC", con, tx);
                stockCmd.Parameters.AddWithValue("@bt", req.BloodType);
                var stocks = new List<(int Id, int Units)>();
                using (var r = stockCmd.ExecuteReader())
                    while (r.Read()) stocks.Add((r.GetInt32(0), r.GetInt32(1)));

                int total = stocks.Sum(s => s.Units);
                if (total < req.UnitsNeeded)
                {
                    tx.Rollback(); con.Dispose();
                    return (false, $"❌ Not enough {req.BloodType} in stock.\n\nAvailable: {total}  |  Needed: {req.UnitsNeeded}", null, null);
                }

                int remaining = req.UnitsNeeded;
                foreach (var (id, units) in stocks)
                {
                    if (remaining <= 0) break;
                    int deduct = Math.Min(remaining, units);
                    using var upd = new SqlCommand(
                        "UPDATE BloodStock SET UnitsAvailable=UnitsAvailable-@d WHERE Id=@id", con, tx);
                    upd.Parameters.AddWithValue("@d", deduct);
                    upd.Parameters.AddWithValue("@id", id);
                    upd.ExecuteNonQuery();
                    using var chk = new SqlCommand(
                        "UPDATE BloodStock SET Status='Used' WHERE Id=@id AND UnitsAvailable=0", con, tx);
                    chk.Parameters.AddWithValue("@id", id);
                    chk.ExecuteNonQuery();
                    remaining -= deduct;
                }

                using var fulfill = new SqlCommand(
                    "UPDATE BloodRequests SET Status='Fulfilled',FulfilledBy=@by WHERE Id=@id", con, tx);
                fulfill.Parameters.AddWithValue("@by", Session.CurrentUser?.Username ?? "system");
                fulfill.Parameters.AddWithValue("@id", req.Id);
                fulfill.ExecuteNonQuery();

                return (true, "", con, tx);
            }
            catch (Exception ex)
            {
                tx.Rollback(); con.Dispose();
                return (false, $"❌ Fulfillment failed: {ex.Message}", null, null);
            }
        }

        public static void CommitFulfill(SqlConnection con, SqlTransaction tx, BloodRequest req)
        {
            tx.Commit();
            con.Dispose();
            Log("FULFILL", "BloodRequests",
                $"Fulfilled {req.UnitsNeeded} units of {req.BloodType} for {req.PatientName} at {req.HospitalName}");
        }

        public static void RollbackFulfill(SqlConnection con, SqlTransaction tx)
        {
            tx.Rollback();
            con.Dispose();
        }

        public static List<DonationRecord> GetDonationRecords(string? search=null, string? bt=null)
        {
            var list = new List<DonationRecord>();
            using var con = GetConnection(); con.Open();
            var sql = @"SELECT dr.Id,dr.DonorId,dr.BloodType,dr.UnitsDonated,dr.DonationDate,
                               ISNULL(dr.Notes,''),ISNULL(dr.RecordedBy,''),d.FullName
                        FROM DonationRecords dr LEFT JOIN Donors d ON dr.DonorId=d.Id WHERE 1=1";
            if (!string.IsNullOrEmpty(search)) sql += " AND d.FullName LIKE @q";
            if (!string.IsNullOrEmpty(bt))     sql += " AND dr.BloodType=@bt";
            sql += " ORDER BY dr.DonationDate DESC";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@q", $"%{search}%");
            if (!string.IsNullOrEmpty(bt))     cmd.Parameters.AddWithValue("@bt", bt);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new DonationRecord {
                    Id=r.GetInt32(0), DonorId=r.GetInt32(1), BloodType=r.GetString(2),
                    UnitsDonated=r.GetInt32(3), DonationDate=r.GetDateTime(4),
                    Notes=r.GetString(5), RecordedBy=r.GetString(6),
                    DonorName=r.IsDBNull(7)?"":r.GetString(7)
                });
            return list;
        }

        public static void AddDonationRecord(DonationRecord d)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO DonationRecords(DonorId,BloodType,UnitsDonated,DonationDate,Notes,RecordedBy) VALUES(@did,@bt,@un,@dd,@notes,@rb)", con);
            cmd.Parameters.AddWithValue("@did", d.DonorId);
            cmd.Parameters.AddWithValue("@bt", d.BloodType);
            cmd.Parameters.AddWithValue("@un", d.UnitsDonated);
            cmd.Parameters.AddWithValue("@dd", d.DonationDate.Date);
            cmd.Parameters.AddWithValue("@notes", (object?)d.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rb", Session.CurrentUser?.Username ?? "system");
            cmd.ExecuteNonQuery();
            Log("INSERT", "DonationRecords", $"Recorded donation: {d.DonorName} ({d.BloodType}, {d.UnitsDonated} unit(s))");
        }

        public static void DeleteDonationRecord(int id)
        {
            SimpleDelete("DonationRecords", id);
            Log("DELETE", "DonationRecords", $"Deleted donation record ID {id}");
        }

        public static List<Appointment> GetAppointments(string? status=null, DateTime? date=null)
        {
            var list = new List<Appointment>();
            using var con = GetConnection(); con.Open();
            var sql = @"SELECT a.Id,a.DonorId,a.AppointmentDate,a.AppointmentTime,a.Purpose,a.Status,
                               ISNULL(a.Notes,''),d.FullName,d.BloodType
                        FROM Appointments a LEFT JOIN Donors d ON a.DonorId=d.Id WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) sql += " AND a.Status=@st";
            if (date.HasValue)                 sql += " AND a.AppointmentDate=@dt";
            sql += " ORDER BY a.AppointmentDate,a.AppointmentTime";
            using var cmd = new SqlCommand(sql, con);
            if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@st", status);
            if (date.HasValue)                 cmd.Parameters.AddWithValue("@dt", date.Value.Date);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Appointment {
                    Id=r.GetInt32(0), DonorId=r.GetInt32(1),
                    AppointmentDate=r.GetDateTime(2), AppointmentTime=r.GetString(3),
                    Purpose=r.GetString(4), Status=r.GetString(5), Notes=r.GetString(6),
                    DonorName=r.IsDBNull(7)?"":r.GetString(7),
                    BloodType=r.IsDBNull(8)?"":r.GetString(8)
                });
            return list;
        }

        public static void AddAppointment(Appointment a)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Appointments(DonorId,AppointmentDate,AppointmentTime,Purpose,Status,Notes)
                VALUES(@did,@dt,@tm,@pu,@st,@notes)", con);
            SetApptParams(cmd, a); cmd.ExecuteNonQuery();
            Log("INSERT", "Appointments", $"Scheduled appointment: {a.DonorName} on {a.DateDisplay} at {a.AppointmentTime} ({a.Purpose})");
        }

        public static void UpdateAppointment(Appointment a)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Appointments SET DonorId=@did,AppointmentDate=@dt,AppointmentTime=@tm,
                Purpose=@pu,Status=@st,Notes=@notes WHERE Id=@id", con);
            SetApptParams(cmd, a);
            cmd.Parameters.AddWithValue("@id", a.Id);
            cmd.ExecuteNonQuery();
            Log("UPDATE", "Appointments", $"Updated appointment ID {a.Id}: {a.DonorName} - {a.Status}");
        }

        static void SetApptParams(SqlCommand cmd, Appointment a)
        {
            cmd.Parameters.AddWithValue("@did", a.DonorId);
            cmd.Parameters.AddWithValue("@dt", a.AppointmentDate.Date);
            cmd.Parameters.AddWithValue("@tm", a.AppointmentTime);
            cmd.Parameters.AddWithValue("@pu", a.Purpose);
            cmd.Parameters.AddWithValue("@st", a.Status);
            cmd.Parameters.AddWithValue("@notes", (object?)a.Notes ?? DBNull.Value);
        }

        public static void DeleteAppointment(int id)
        {
            SimpleDelete("Appointments", id);
            Log("DELETE", "Appointments", $"Deleted appointment ID {id}");
        }

        public static DashboardStats GetDashboardStats()
        {
            using var con = GetConnection(); con.Open();
            int donors    = Scalar<int>(con, "SELECT COUNT(*) FROM Donors WHERE Status='Active'");
            int units     = Scalar<int>(con, "SELECT ISNULL(SUM(UnitsAvailable),0) FROM BloodStock WHERE Status='Available'");
            int pending   = Scalar<int>(con, "SELECT COUNT(*) FROM BloodRequests WHERE Status='Pending'");
            int hospitals = Scalar<int>(con, "SELECT COUNT(*) FROM Hospitals");
            int donations = Scalar<int>(con, "SELECT COUNT(*) FROM DonationRecords");
            int appts     = Scalar<int>(con, "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate=CAST(GETDATE() AS DATE) AND Status='Scheduled'");
            int expiring  = Scalar<int>(con, "SELECT COUNT(*) FROM BloodStock WHERE Status='Available' AND ExpiryDate<=DATEADD(DAY,30,GETDATE()) AND ExpiryDate>=GETDATE()");
            int critical  = Scalar<int>(con, "SELECT COUNT(*) FROM BloodRequests WHERE UrgencyLevel='Critical' AND Status='Pending'");

            var stockLevels = new List<StockLevel>();
            using (var cmd2 = new SqlCommand("SELECT BloodType,SUM(UnitsAvailable) FROM BloodStock WHERE Status='Available' GROUP BY BloodType ORDER BY BloodType", con))
            using (var r = cmd2.ExecuteReader())
                while (r.Read()) stockLevels.Add(new StockLevel { BloodType=r.GetString(0), Units=r.GetInt32(1) });

            var recentActivity = new List<AuditLog>();
            using (var cmd3 = new SqlCommand("SELECT TOP 6 Id,Action,TableName,Details,Username,LogDate FROM AuditLogs ORDER BY LogDate DESC", con))
            using (var r = cmd3.ExecuteReader())
                while (r.Read())
                    recentActivity.Add(new AuditLog {
                        Id=r.GetInt32(0), Action=r.GetString(1), TableName=r.GetString(2),
                        Details=r.GetString(3), Username=r.GetString(4), LogDate=r.GetDateTime(5)
                    });

            return new DashboardStats {
                ActiveDonors=donors, TotalUnits=units, PendingRequests=pending,
                TotalHospitals=hospitals, TotalDonations=donations,
                TodayAppointments=appts, ExpiringItems=expiring, CriticalRequests=critical,
                StockLevels=stockLevels, RecentActivity=recentActivity
            };
        }

        public static List<BloodStock> GetLowStock(int threshold=5)
        {
            var list = new List<BloodStock>();
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id,BloodType,UnitsAvailable,ExpiryDate,Source,ReceivedDate,Status FROM BloodStock WHERE UnitsAvailable<=@t AND Status='Available' ORDER BY UnitsAvailable", con);
            cmd.Parameters.AddWithValue("@t", threshold);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new BloodStock {
                    Id=r.GetInt32(0), BloodType=r.GetString(1), UnitsAvailable=r.GetInt32(2),
                    ExpiryDate=r.IsDBNull(3)?null:r.GetDateTime(3),
                    Source=r.IsDBNull(4)?"":r.GetString(4),
                    ReceivedDate=r.GetDateTime(5), Status=r.GetString(6)
                });
            return list;
        }

        public static List<Donor> GetCompatibleDonors(string recipientType)
        {
            var compatible = CompatibleTypes(recipientType);
            if (compatible.Length == 0) return new List<Donor>();
            var list = new List<Donor>();
            using var con = GetConnection(); con.Open();
            var inClause = string.Join(",", compatible.Select((_, i) => $"@p{i}"));
            using var cmd = new SqlCommand(
                $"SELECT d.Id,d.FullName,d.BloodType,d.Phone,d.Email,d.Address,d.Age,d.Gender,d.Weight,d.LastDonationDate,d.Status,d.RegisteredDate,COUNT(dr.Id) AS TotalDonations, " +
                "CASE WHEN d.Status = 'Inactive' THEN 'Inactive' WHEN d.LastDonationDate IS NULL THEN 'Eligible' WHEN DATEDIFF(day, d.LastDonationDate, GETDATE()) >= 56 THEN 'Eligible' ELSE 'Wait ' + CAST(56 - DATEDIFF(day, d.LastDonationDate, GETDATE()) AS VARCHAR) + 'd' END AS EligibilityStatus " +
                $"FROM Donors d LEFT JOIN DonationRecords dr ON d.Id=dr.DonorId WHERE d.BloodType IN ({inClause}) AND d.Status='Active' GROUP BY d.Id,d.FullName,d.BloodType,d.Phone,d.Email,d.Address,d.Age,d.Gender,d.Weight,d.LastDonationDate,d.Status,d.RegisteredDate ORDER BY d.BloodType,d.FullName", con);
            for (int i = 0; i < compatible.Length; i++) cmd.Parameters.AddWithValue($"@p{i}", compatible[i]);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Donor {
                    Id=r.GetInt32(0), FullName=r.GetString(1), BloodType=r.GetString(2),
                    Phone=r.IsDBNull(3)?"":r.GetString(3), Email=r.IsDBNull(4)?"":r.GetString(4),
                    Address=r.IsDBNull(5)?"":r.GetString(5), Age=r.GetInt32(6),
                    Gender=r.IsDBNull(7)?"":r.GetString(7),
                    Weight=r.IsDBNull(8)?0:double.Parse(r.GetDecimal(8).ToString()),
                    LastDonationDate=r.IsDBNull(9)?null:r.GetDateTime(9),
                    Status=r.GetString(10), RegisteredDate=r.GetDateTime(11), 
                    TotalDonations=r.GetInt32(12),
                    EligibilityStatus=r.GetString(13)
                });
            return list;
        }

        public static string[] CompatibleTypes(string recipient) => recipient switch
        {
            "A+"  => new[]{"A+","A-","O+","O-"},
            "A-"  => new[]{"A-","O-"},
            "B+"  => new[]{"B+","B-","O+","O-"},
            "B-"  => new[]{"B-","O-"},
            "AB+" => new[]{"A+","A-","B+","B-","AB+","AB-","O+","O-"},
            "AB-" => new[]{"A-","B-","AB-","O-"},
            "O+"  => new[]{"O+","O-"},
            "O-"  => new[]{"O-"},
            _     => Array.Empty<string>()
        };

        public static List<BloodTypeReport> GetBloodTypeReport()
        {
            var list = new List<BloodTypeReport>();
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand(@"
                SELECT bt.BloodType,
                    ISNULL(d.TotalDonations,0),
                    ISNULL(s.UnitsInStock,0),
                    ISNULL(r.TotalRequests,0),
                    ISNULL(r.Fulfilled,0),
                    ISNULL(dn.ActiveDonors,0)
                FROM (VALUES('A+'),('A-'),('B+'),('B-'),('AB+'),('AB-'),('O+'),('O-')) AS bt(BloodType)
                LEFT JOIN (SELECT BloodType,COUNT(*) AS TotalDonations FROM DonationRecords GROUP BY BloodType) d ON bt.BloodType=d.BloodType
                LEFT JOIN (SELECT BloodType,SUM(UnitsAvailable) AS UnitsInStock FROM BloodStock WHERE Status='Available' GROUP BY BloodType) s ON bt.BloodType=s.BloodType
                LEFT JOIN (SELECT BloodType,COUNT(*) AS TotalRequests,SUM(CASE WHEN Status='Fulfilled' THEN 1 ELSE 0 END) AS Fulfilled FROM BloodRequests GROUP BY BloodType) r ON bt.BloodType=r.BloodType
                LEFT JOIN (SELECT BloodType,COUNT(*) AS ActiveDonors FROM Donors WHERE Status='Active' GROUP BY BloodType) dn ON bt.BloodType=dn.BloodType
                ORDER BY bt.BloodType", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new BloodTypeReport {
                    BloodType=r.GetString(0), TotalDonations=r.GetInt32(1),
                    UnitsInStock=r.GetInt32(2), TotalRequests=r.GetInt32(3),
                    FulfilledRequests=r.GetInt32(4), ActiveDonors=r.GetInt32(5)
                });
            return list;
        }

        public static (bool success, string message) BackupDatabase(string destinationPath)
        {
            try
            {
                using var con = GetConnection();
                con.Open();
                string sql = "BACKUP DATABASE BloodBankProDB TO DISK = @path";
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@path", destinationPath);
                cmd.ExecuteNonQuery();
                return (true, "Backup successful");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        static void SimpleDelete(string table, int id)
        {
            using var con = GetConnection(); con.Open();
            using var cmd = new SqlCommand($"DELETE FROM {table} WHERE Id=@id", con);
            cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
        }

    }

    public class DashboardStats
    {
        public int ActiveDonors      { get; set; }
        public int TotalUnits        { get; set; }
        public int PendingRequests   { get; set; }
        public int TotalHospitals    { get; set; }
        public int TotalDonations    { get; set; }
        public int TodayAppointments { get; set; }
        public int ExpiringItems     { get; set; }
        public int CriticalRequests  { get; set; }
        public List<StockLevel> StockLevels    { get; set; } = new();
        public List<AuditLog>   RecentActivity { get; set; } = new();
    }

    public class StockLevel
    {
        public string BloodType { get; set; } = "";
        public int    Units     { get; set; }
    }
}
