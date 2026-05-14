# 🩸 BloodBridge

**ASP.NET Core Blood Donation Management System**

> Role-based platform connecting donors, patients, and hospitals — with real-time notifications, eligibility tracking, and an admin dashboard.

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/4283b0fb-3a4d-4859-b04a-561ad0d3dde4" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/995fc47d-4362-4e55-9464-b6b1edf1230b" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/544a7856-ea8b-4d8d-87aa-1c5b4ec68c96" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/a5cba056-aa60-4f15-9579-b85641c97e51" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/cb81cac6-b4ef-475b-a110-cc161a41841c" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/016bdaf5-9ef5-429a-83f2-5735c0e2822b" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/6ddfbe89-0de4-4264-9a71-93e2a1d40c35" />





---

## ✨ Features

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

---

## ⚙️ Requirements

- Visual Studio 2022
- .NET 8 SDK
- SQL Server (LocalDB is fine — comes with VS)
- Gmail account (for email notifications)

---

## 🚀 Setup Guide

### 1. Open Project

Open `BloodBridge.csproj` in Visual Studio 2022.

---

### 2. Configure Email

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

> **Gmail App Password:** Google Account → Security → 2-Step Verification → App Passwords → Generate one.

---

### 3. Configure Database

Default connection string uses SQL Server LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BloodBridgeDb;Trusted_Connection=True;"
```

Change this if you're using a different SQL Server instance.

---

### 4. Run Migrations

In Visual Studio → **Tools → NuGet Package Manager → Package Manager Console:**

```powershell
Add-Migration InitialCreate
Update-Database
```

---

### 5. Run the Project

Press `F5` or click the green **Play** button.

---

## 🔐 Default Admin Account

```
Email:    admin@bloodbridge.com
Password: Admin@123
```

---

## 📁 Project Structure

```
BloodBridge/
├── Controllers/
│   ├── AccountController.cs     — Register, Login, Profile
│   ├── DonorController.cs       — Donor dashboard, history, search
│   ├── RequestController.cs     — Blood request CRUD + notifications
│   ├── AdminController.cs       — Admin panel
│   └── HomeController.cs        — Landing page, notifications API
├── Models/
│   ├── ApplicationUser.cs
│   ├── DonorProfile.cs
│   ├── DonationHistory.cs
│   ├── BloodRequest.cs
│   └── RequestResponse.cs       — Also contains Notification model
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   ├── EmailService.cs          — MailKit SMTP
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

## 🛠️ Tech Stack

| Layer | Technologies |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 8) |
| **Database** | SQL Server / LocalDB · Entity Framework Core |
| **Auth** | ASP.NET Core Identity |
| **Email** | MailKit · Gmail SMTP |
| **Frontend** | Razor Views · Bootstrap · CSS |

---

## 📄 License

[MIT](LICENSE)

---

*BloodBridge — Connecting donors to those in need.* 🩸
