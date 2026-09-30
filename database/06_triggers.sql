USE [CustomerOrdersDB];
GO

CREATE OR ALTER TRIGGER dbo.TR_Orders_UpdatedAt
ON dbo.Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF TRIGGER_NESTLEVEL() > 1
    BEGIN
        RETURN;
    END;

    UPDATE target
    SET UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Orders AS target
    INNER JOIN inserted AS changed ON changed.Id = target.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.TR_OrderItems_RecalcTotal
ON dbo.OrderItems
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AffectedOrders TABLE (OrderId INT PRIMARY KEY);

    INSERT INTO @AffectedOrders (OrderId)
    SELECT OrderId FROM inserted
    UNION
    SELECT OrderId FROM deleted;

    UPDATE target
    SET
        Total = COALESCE
        (
            (
                SELECT SUM(CONVERT(DECIMAL(18,2), item.Quantity) * item.UnitPrice)
                FROM dbo.OrderItems AS item
                WHERE item.OrderId = target.Id
            ),
            0.00
        ),
        UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Orders AS target
    INNER JOIN @AffectedOrders AS affected ON affected.OrderId = target.Id;
END
GO