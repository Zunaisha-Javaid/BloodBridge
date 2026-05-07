# 🩸 BloodBridge — Setup Guide

## Requirements
- Visual Studio 2022
- .NET 8 SDK
- SQL Server (LocalDB is fine — comes with VS)
- Gmail account (for email notifications)

---

## Steps to Run

### 1. Open Project
Open `BloodBridge.csproj` in Visual Studio 2022.

### 2. Configure Email (appsettings.json)
Edit `appsettings.json` and fill in your Gmail credentials:
```json
"EmailSettings": {
  "SenderName": "BloodBridge",
  "SenderEmail": "your-email@gmail.com",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-gmail-app-password"
}
```
> For Gmail App Password: Go to Google Account → Security → 2-Step Verification → App Passwords → Generate one.

### 3. Configure Database (appsettings.json)
Default connection string uses SQL Server LocalDB:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BloodBridgeDb;Trusted_Connection=True;"
```
Change this if you're using a different SQL Server instance.

### 4. Run Migrations
In Visual Studio → Tools → NuGet Package Manager → Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```

### 5. Run the Project
Press `F5` or click the green Play button.

---

## Default Admin Account
```
Email:    admin@bloodbridge.com
Password: Admin@123
```

---

## Project Structure
```
BloodBridge/
├── Controllers/
│   ├── AccountController.cs   — Register, Login, Profile
│   ├── DonorController.cs     — Donor dashboard, history, search
│   ├── RequestController.cs   — Blood request CRUD + notifications
│   ├── AdminController.cs     — Admin panel
│   └── HomeController.cs      — Landing page, notifications API
├── Models/
│   ├── ApplicationUser.cs
│   ├── DonorProfile.cs
│   ├── DonationHistory.cs
│   ├── BloodRequest.cs
│   └── RequestResponse.cs     — Also contains Notification model
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   ├── EmailService.cs        — MailKit SMTP
│   └── NotificationService.cs
├── ViewModels/
│   └── ViewModels.cs
├── Views/
│   ├── Account/
│   ├── Donor/
│   ├── Request/
│   ├── Admin/
│   ├── Home/
│   └── Shared/_Layout.cshtml
└── wwwroot/css/site.css
```

---

## Features Implemented
- ✅ Role-based auth (Donor, Patient, Hospital, Admin)
- ✅ Donor profile + availability toggle
- ✅ Donation history log + eligibility tracker (56-day rule)
- ✅ Blood request posting with urgency flag
- ✅ Request feed with blood type + city filters
- ✅ Donor search by blood type + city
- ✅ Email notifications (MailKit/Gmail SMTP)
- ✅ In-app notification bell with polling
- ✅ Admin dashboard with stats + CSV export
- ✅ User management (lock/unlock/delete)
- ✅ Blood type compatibility guide
