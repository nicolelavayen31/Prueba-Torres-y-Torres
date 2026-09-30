USE [CustomerOrdersDB];
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id         INT            IDENTITY(1,1) NOT NULL,
        CustomerId INT            NOT NULL,
        Status     NVARCHAR(20)   NOT NULL
            CONSTRAINT DF_Orders_Status DEFAULT N'Pending',
        Total      DECIMAL(10,2)  NOT NULL
            CONSTRAINT DF_Orders_Total DEFAULT (0.00),
        Notes      NVARCHAR(500)  NULL,
        CreatedAt  DATETIME2(7)   NOT NULL
            CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt  DATETIME2(7)   NOT NULL
            CONSTRAINT DF_Orders_UpdatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Orders PRIMARY KEY (Id),
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_Orders_Status CHECK
            (Status IN (N'Pending', N'InProgress', N'Completed', N'Cancelled')),
        CONSTRAINT CK_Orders_Total CHECK (Total >= 0)
    );

    CREATE INDEX IX_Orders_CustomerId ON dbo.Orders (CustomerId);
    CREATE INDEX IX_Orders_Status ON dbo.Orders (Status);
    CREATE INDEX IX_Orders_CreatedAt ON dbo.Orders (CreatedAt);
END
GO