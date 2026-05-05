# Blood Bank Medical Suite


🚀 Key Features
Strict Eligibility Engine: Automatically enforces the 56-Day Rule using SQL DATEDIFF logic to ensure donor safety.  

Inventory Intelligence: Real-time stock monitoring with visual alerts for low inventory levels.  

Enterprise UI: Modern "Medical Suite" aesthetic featuring consistent branding and professional curved input fields.  

Security: Implements Parameterized SQL Queries to prevent SQL injection and uses Windows Integrated Security.  

🛠️ Technical Stack
Language: C#  

Framework: WPF (.NET)  

Database: Microsoft SQL Server LocalDB  

Architecture: Scoped Resource Management with automated connection disposal.  

👥 Development Team

Lead Architect 
UI/UX & Security:Charan

⚙️ How to Run the Project
To run the Blood Bank Medical Suite on your local machine, follow these steps:

1. Prerequisites
Visual Studio 2022: Ensure you have the ".NET Desktop Development" workload installed.

SQL Server LocalDB: This is required to host the BloodBankProDB.mdf database.

Target Framework: The project is built using WPF (.NET).

2. Installation Steps
Clone or Download: Download the project as a ZIP file from this GitHub repository and extract it to your PC.

Open the Solution: Navigate to the project folder and double-click the BloodBankPro.sln file to open it in Visual Studio.

Check the Database: Ensure the BloodBankProDB.mdf file is present within the Database folder in the Solution Explorer.

Restore Packages: If prompted, allow Visual Studio to restore any missing NuGet packages.

Build & Run: Press F5 or click the "Start" button in Visual Studio to compile and launch the application.

3. Troubleshooting Connection Issues
If the database fails to load, ensure that (localdb)\MSSQLLocalDB is running on your system.

The application uses Windows Integrated Security, so no additional database passwords are required for the initial setup.
