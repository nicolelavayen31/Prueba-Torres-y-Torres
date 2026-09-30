USE [CustomerOrdersDB];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id           INT            IDENTITY(1,1) NOT NULL,
        Email        NVARCHAR(254)  NOT NULL,
        DisplayName  NVARCHAR(80)   NOT NULL,
        PasswordHash NVARCHAR(255)  NOT NULL,
        CreatedAt    DATETIME2(7)   NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END
GO