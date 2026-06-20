-- ============================================================
-- FreelanceMarketplace Database Setup Script
-- SQL Server
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FreelanceMarketplace')
BEGIN
    CREATE DATABASE FreelanceMarketplace;
END
GO

USE FreelanceMarketplace;
GO

-- ============================================================
-- ASP.NET Identity Tables
-- ============================================================

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE AspNetRoles (
        Id nvarchar(450) NOT NULL PRIMARY KEY,
        Name nvarchar(256) NULL,
        NormalizedName nvarchar(256) NULL,
        ConcurrencyStamp nvarchar(max) NULL
    );
    CREATE INDEX RoleNameIndex ON AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
END
GO

-- Users Table (with custom ApplicationUser fields)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE AspNetUsers (
        Id nvarchar(450) NOT NULL PRIMARY KEY,
        FullName nvarchar(100) NOT NULL,
        Bio nvarchar(500) NULL,
        Skills nvarchar(300) NULL,
        ProfilePicture nvarchar(200) NULL,
        HourlyRate decimal(18,2) NOT NULL DEFAULT 0,
        PortfolioUrl nvarchar(200) NULL,
        YearsExperience int NOT NULL DEFAULT 0,
        CompanyName nvarchar(100) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        IsActive bit NOT NULL DEFAULT 1,
        LastLoginAt datetime2 NULL,
        UserName nvarchar(256) NULL,
        NormalizedUserName nvarchar(256) NULL,
        Email nvarchar(256) NULL,
        NormalizedEmail nvarchar(256) NULL,
        EmailConfirmed bit NOT NULL DEFAULT 0,
        PasswordHash nvarchar(max) NULL,
        SecurityStamp nvarchar(max) NULL,
        ConcurrencyStamp nvarchar(max) NULL,
        PhoneNumber nvarchar(max) NULL,
        PhoneNumberConfirmed bit NOT NULL DEFAULT 0,
        TwoFactorEnabled bit NOT NULL DEFAULT 0,
        LockoutEnd datetimeoffset NULL,
        LockoutEnabled bit NOT NULL DEFAULT 0,
        AccessFailedCount int NOT NULL DEFAULT 0
    );
    CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
    CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
END
GO

-- User Roles Join Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE AspNetUserRoles (
        UserId nvarchar(450) NOT NULL,
        RoleId nvarchar(450) NOT NULL,
        PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
END
GO

-- Role Claims Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE AspNetRoleClaims (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId nvarchar(450) NOT NULL,
        ClaimType nvarchar(max) NULL,
        ClaimValue nvarchar(max) NULL,
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
END
GO

-- User Claims Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE AspNetUserClaims (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId nvarchar(450) NOT NULL,
        ClaimType nvarchar(max) NULL,
        ClaimValue nvarchar(max) NULL,
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
END
GO

-- User Logins Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE AspNetUserLogins (
        LoginProvider nvarchar(450) NOT NULL,
        ProviderKey nvarchar(450) NOT NULL,
        ProviderDisplayName nvarchar(max) NULL,
        UserId nvarchar(450) NOT NULL,
        PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
END
GO

-- User Tokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE AspNetUserTokens (
        UserId nvarchar(450) NOT NULL,
        LoginProvider nvarchar(450) NOT NULL,
        Name nvarchar(450) NOT NULL,
        Value nvarchar(max) NULL,
        PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

-- ============================================================
-- Application Tables
-- ============================================================

-- Categories Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name nvarchar(50) NOT NULL,
        Description nvarchar(200) NULL,
        IconClass nvarchar(50) NULL DEFAULT 'bi-folder',
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Categories_Name UNIQUE (Name)
    );
    CREATE UNIQUE INDEX IX_Categories_Name ON Categories (Name);
END
GO

-- Projects Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Projects')
BEGIN
    CREATE TABLE Projects (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title nvarchar(150) NOT NULL,
        Description nvarchar(max) NOT NULL,
        Budget decimal(10,2) NOT NULL,
        MaxBudget decimal(10,2) NULL,
        Deadline datetime2 NOT NULL,
        Status int NOT NULL DEFAULT 0,  -- 0=Open, 1=InProgress, 2=Completed, 3=Cancelled
        RequiredSkills nvarchar(300) NULL,
        AttachmentPath nvarchar(200) NULL,
        ViewCount int NOT NULL DEFAULT 0,
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt datetime2 NULL,
        ClientId nvarchar(450) NOT NULL,
        CategoryId int NOT NULL,
        AwardedFreelancerId nvarchar(450) NULL,
        CONSTRAINT FK_Projects_Client FOREIGN KEY (ClientId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Projects_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Projects_AwardedFreelancer FOREIGN KEY (AwardedFreelancerId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Projects_ClientId ON Projects (ClientId);
    CREATE INDEX IX_Projects_CategoryId ON Projects (CategoryId);
    CREATE INDEX IX_Projects_AwardedFreelancerId ON Projects (AwardedFreelancerId);
END
GO

-- Bids Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bids')
BEGIN
    CREATE TABLE Bids (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Amount decimal(10,2) NOT NULL,
        ProposalMessage nvarchar(1000) NOT NULL,
        DeliveryDays int NOT NULL,
        Status int NOT NULL DEFAULT 0,  -- 0=Pending, 1=Accepted, 2=Rejected
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        ProjectId int NOT NULL,
        FreelancerId nvarchar(450) NOT NULL,
        CONSTRAINT FK_Bids_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Bids_Freelancer FOREIGN KEY (FreelancerId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Bids_ProjectId ON Bids (ProjectId);
    CREATE INDEX IX_Bids_FreelancerId ON Bids (FreelancerId);
END
GO

-- Reviews Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment nvarchar(500) NOT NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        ProjectId int NOT NULL,
        ReviewerId nvarchar(450) NOT NULL,
        RevieweeId nvarchar(450) NOT NULL,
        CONSTRAINT FK_Reviews_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Reviews_Reviewer FOREIGN KEY (ReviewerId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Reviews_Reviewee FOREIGN KEY (RevieweeId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Reviews_ProjectId ON Reviews (ProjectId);
    CREATE INDEX IX_Reviews_ReviewerId ON Reviews (ReviewerId);
    CREATE INDEX IX_Reviews_RevieweeId ON Reviews (RevieweeId);
END
GO

-- Notifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Message nvarchar(300) NOT NULL,
        IsRead bit NOT NULL DEFAULT 0,
        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
        Link nvarchar(200) NULL,
        Type nvarchar(50) NULL,
        UserId nvarchar(450) NOT NULL,
        CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_Notifications_UserId ON Notifications (UserId);
END
GO

-- ProjectAttachments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectAttachments')
BEGIN
    CREATE TABLE ProjectAttachments (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FileName nvarchar(200) NOT NULL,
        FilePath nvarchar(300) NOT NULL,
        FileSize bigint NOT NULL,
        UploadedAt datetime2 NOT NULL DEFAULT GETDATE(),
        ProjectId int NOT NULL,
        CONSTRAINT FK_ProjectAttachments_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProjectAttachments_ProjectId ON ProjectAttachments (ProjectId);
END
GO

-- ============================================================
-- Seed Data
-- ============================================================

-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles)
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES
        (NEWID(), 'Admin', 'ADMIN', NEWID()),
        (NEWID(), 'Client', 'CLIENT', NEWID()),
        (NEWID(), 'Freelancer', 'FREELANCER', NEWID());
END
GO

-- Seed Admin User
-- Password: Admin@123 (hashed, actual value set by application during seeding)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@freelancehub.com')
BEGIN
    DECLARE @AdminId nvarchar(450) = NEWID();
    DECLARE @AdminRoleId nvarchar(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');

    INSERT INTO AspNetUsers (Id, FullName, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, IsActive, CreatedAt, LockoutEnabled, AccessFailedCount)
    VALUES (@AdminId, 'System Administrator', 'admin@freelancehub.com', 'ADMIN@FREELANCEHUB.COM', 'admin@freelancehub.com', 'ADMIN@FREELANCEHUB.COM', 1, 1, GETDATE(), 0, 0);

    -- Password hash is set by UserManager during seeding
    -- This is a placeholder reference

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId);
END
GO

-- Seed Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name, Description, IconClass, CreatedAt)
    VALUES
        ('Web Development', 'Websites, web apps, and backend systems', 'bi-globe', GETDATE()),
        ('Mobile Apps', 'iOS and Android mobile applications', 'bi-phone', GETDATE()),
        ('Graphic Design', 'Logos, branding, and visual design', 'bi-palette', GETDATE()),
        ('Content Writing', 'Articles, blogs, copywriting, and editing', 'bi-pencil', GETDATE()),
        ('Digital Marketing', 'SEO, social media, and advertising', 'bi-megaphone', GETDATE()),
        ('Data Science', 'Data analysis, ML, and AI solutions', 'bi-bar-chart', GETDATE());
END
GO

PRINT 'Database setup completed successfully.';
GO