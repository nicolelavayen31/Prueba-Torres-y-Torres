USE [CustomerOrdersDB];
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id          INT            IDENTITY(1,1) NOT NULL,
        OrderId     INT            NOT NULL,
        Description NVARCHAR(300)  NOT NULL,
        Quantity    INT            NOT NULL
            CONSTRAINT DF_OrderItems_Quantity DEFAULT (1),
        UnitPrice   DECIMAL(10,2)  NOT NULL
            CONSTRAINT DF_OrderItems_UnitPrice DEFAULT (0.00),

        CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0)
    );

    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems (OrderId);
END
GO