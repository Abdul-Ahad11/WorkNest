# WorkNest — Freelance Project Marketplace

## Overview

WorkNest is a full-featured web application built with ASP.NET Core MVC that connects talented freelancers with clients who need projects completed. Clients can post projects with detailed requirements, budgets, and deadlines. Freelancers can browse open projects, submit competitive bids, and get awarded work. The platform includes real-time notifications, an admin dashboard with analytics, and role-based dashboards for each user type.

Built with a modern tech stack and following best practices for security (ASP.NET Identity, CSRF protection, role-based authorization), this application is ready for production deployment after configuration.

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8)
- **Language:** C# 12
- **Database:** SQL Server (LocalDB for development)
- **ORM:** Entity Framework Core 8
- **Authentication:** ASP.NET Core Identity with Roles (Admin, Client, Freelancer)
- **Frontend:** Bootstrap 5, jQuery, Chart.js
- **AJAX:** jQuery AJAX for real-time updates
- **UI Icons:** Bootstrap Icons

## Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community Edition or higher)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (comes with Visual Studio)
- [Node.js](https://nodejs.org/) (optional, for client-side package management)

## Installation Steps

### 1. Clone the Repository

```bash
git clone <repository-url>
cd WorkNest
```

### 2. Open in Visual Studio

Open `WorkNest.slnx` in Visual Studio 2022.

### 3. Update Connection String

The default connection string in `appsettings.json` uses LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkNest;Trusted_Connection=True;MultipleActiveResultSets=true"
```

If you use a different SQL Server instance, update this connection string.

### 4. Apply Database Migrations

Open **Package Manager Console** (Tools → NuGet Package Manager → Package Manager Console) and run:

```powershell
Update-Database
```

This will create the `WorkNest` database with all tables.

### 5. Run the Application

Press **Ctrl+F5** to run without debugging, or **F5** to debug.

On first startup, the database seeder will automatically:
- Create the roles: Admin, Client, Freelancer
- Create sample user accounts
- Seed 6 project categories
- Create 5 sample projects

## Default Credentials

| Role       | Email                       | Password   |
|------------|-----------------------------|------------|
| Admin      | admin@freelancehub.com      | Admin@123  |
| Client     | client1@test.com            | Client@123 |
| Client     | client2@test.com            | Client@123 |
| Client     | client3@test.com            | Client@123 |
| Freelancer | freelancer1@test.com        | Free@123   |
| Freelancer | freelancer2@test.com        | Free@123   |
| Freelancer | freelancer3@test.com        | Free@123   |

## Features

### General
- Landing page with platform statistics and recent projects
- User registration with role selection (Client/Freelancer)
- User login with role-based redirect
- Profile management with avatar upload
- Real-time notification system (AJAX polling)
- Responsive Bootstrap 5 design

### Client Features
- Post new projects with budget, deadline, and category
- Edit and cancel open projects
- View bids received on projects
- Award projects to freelancers
- Mark projects as completed
- Manage projects from a dedicated dashboard

### Freelancer Features
- Browse open projects with search and filter
- Submit bids on projects (AJAX, no page reload)
- View and withdraw pending bids
- Track bid status (pending, accepted, rejected)

### Admin Features
- Full dashboard with Chart.js analytics
  - Projects by category (bar chart)
  - Project status distribution (doughnut chart)
  - User registration trends (line chart)
- User management with search and toggle active/inactive
- Project management with delete capability
- Category CRUD management

### Security
- ASP.NET Core Identity with password policies
- Role-based authorization (Admin, Client, Freelancer)
- CSRF protection on all POST requests
- Anti-forgery tokens on all forms
- SQL injection protection via Entity Framework
- XSS protection via Razor HTML encoding

## Project Structure

```
WorkNest/
├── Controllers/
│   ├── AccountController.cs      # Registration, login, profile
│   ├── AdminController.cs        # Admin dashboard and management
│   ├── BidController.cs          # Bid submission and management
│   ├── ClientController.cs       # Client dashboard
│   ├── FreelancerController.cs   # Freelancer dashboard
│   ├── HomeController.cs         # Home page and error pages
│   ├── NotificationController.cs # Notification AJAX endpoints
│   └── ProjectController.cs      # Project CRUD and browsing
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   └── DbSeeder.cs               # Database seeder
├── Models/
│   ├── ApplicationUser.cs        # Extended Identity user
│   ├── Bid.cs                    # Bid entity
│   ├── Category.cs               # Category entity
│   ├── Notification.cs           # Notification entity
│   ├── Project.cs                # Project entity
│   ├── ProjectAttachment.cs      # Project file attachment
│   └── Review.cs                 # Review entity
├── Services/
│   ├── INotificationService.cs   # Notification interface
│   └── NotificationService.cs    # Notification implementation
├── ViewModels/
│   ├── Admin/
│   │   └── AdminDashboardViewModel.cs
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   ├── ProfileViewModel.cs
│   │   └── RegisterViewModel.cs
│   ├── Bid/
│   │   └── BidSubmitViewModel.cs
│   └── Project/
│       ├── CreateProjectViewModel.cs
│       ├── ProjectBrowseViewModel.cs
│       ├── ProjectDetailsViewModel.cs
│       └── ProjectEditViewModel.cs
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Profile.cshtml
│   │   └── Register.cshtml
│   ├── Admin/
│   │   ├── Categories.cshtml
│   │   ├── Dashboard.cshtml
│   │   ├── Projects.cshtml
│   │   └── Users.cshtml
│   ├── Bid/
│   │   └── MyBids.cshtml
│   ├── Client/
│   │   └── Dashboard.cshtml
│   ├── Freelancer/
│   │   └── Dashboard.cshtml
│   ├── Home/
│   │   ├── AccessDenied.cshtml
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Project/
│   │   ├── Browse.cshtml
│   │   ├── Create.cshtml
│   │   ├── Details.cshtml
│   │   ├── Edit.cshtml
│   │   ├── MyProjects.cshtml
│   │   └── _ProjectCard.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
├── wwwroot/
│   ├── css/site.css
│   └── js/
│       ├── bid-submit.js
│       ├── notifications.js
│       ├── project-browse.js
│       └── site.js
├── appsettings.json
├── Program.cs
└── README.md
```

## Database

To create the database from scratch without EF migrations, run the SQL script located at:

```
Database/WorkNest.sql
```

This script creates all tables and seeds initial data (roles, categories).

## Deployment

For production deployment:

1. Update the connection string in `appsettings.json` to point to your production SQL Server
2. Set `ASPNETCORE_ENVIRONMENT` to `Production`
3. Configure HTTPS certificate
4. Update the `AllowedHosts` setting in `appsettings.json`
5. Remove or secure the development exception page
6. Use a secure password for the admin account (change from default)

## License

This project is for educational purposes.
