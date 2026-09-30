USE [CustomerOrdersDB];
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        Id        INT            IDENTITY(1,1) NOT NULL,
        Name      NVARCHAR(150)  NOT NULL,
        Email     NVARCHAR(254)  NOT NULL,
        Phone     NVARCHAR(20)   NULL,
        Address   NVARCHAR(300)  NULL,
        IsActive  BIT            NOT NULL
            CONSTRAINT DF_Customers_IsActive DEFAULT (1),
        CreatedAt DATETIME2(7)   NOT NULL
            CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Customers PRIMARY KEY (Id),
        CONSTRAINT UQ_Customers_Email UNIQUE (Email)
    );

    CREATE INDEX IX_Customers_IsActive ON dbo.Customers (IsActive);
    CREATE INDEX IX_Customers_Name ON dbo.Customers (Name);
END
GO