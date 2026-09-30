USE [CustomerOrdersDB];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO dbo.Customers (Name, Email, Phone, Address, IsActive)
SELECT source.Name, source.Email, source.Phone, source.Address, source.IsActive
FROM
(
    VALUES
        (N'Juan Perez', N'juan@mail.com', N'0991234567', N'Av. Principal 123, Guayaquil', CONVERT(BIT, 1)),
        (N'Maria Garcia', N'maria@mail.com', N'0987654321', N'Calle Flores 456, Quito', CONVERT(BIT, 1)),
        (N'Carlos Lopez', N'carlos@mail.com', N'0976543210', N'Av. 9 de Octubre, Guayaquil', CONVERT(BIT, 1)),
        (N'Ana Rodriguez', N'ana@mail.com', N'0965432109', N'Av. Amazonas 789, Quito', CONVERT(BIT, 0))
) AS source (Name, Email, Phone, Address, IsActive)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Customers AS existing
    WHERE existing.Email = source.Email
);

DECLARE @OrderSeed TABLE
(
    CustomerEmail NVARCHAR(254) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    Notes NVARCHAR(500) NOT NULL
);

INSERT INTO @OrderSeed (CustomerEmail, Status, Notes)
VALUES
    (N'juan@mail.com', N'Pending', N'Demo: office delivery'),
    (N'juan@mail.com', N'Completed', N'Demo: delivered'),
    (N'maria@mail.com', N'InProgress', N'Demo: special packaging'),
    (N'carlos@mail.com', N'Cancelled', N'Demo: out of stock'),
    (N'maria@mail.com', N'Pending', N'Demo: waiting for dispatch');

INSERT INTO dbo.Orders (CustomerId, Status, Notes)
SELECT customer.Id, source.Status, source.Notes
FROM @OrderSeed AS source
INNER JOIN dbo.Customers AS customer ON customer.Email = source.CustomerEmail
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Orders AS existing
    WHERE existing.CustomerId = customer.Id
      AND existing.Status = source.Status
      AND existing.Notes = source.Notes
);

DECLARE @ItemSeed TABLE
(
    CustomerEmail NVARCHAR(254) NOT NULL,
    OrderNotes NVARCHAR(500) NOT NULL,
    Description NVARCHAR(300) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL
);

INSERT INTO @ItemSeed (CustomerEmail, OrderNotes, Description, Quantity, UnitPrice)
VALUES
    (N'juan@mail.com', N'Demo: office delivery', N'Laptop HP 15', 1, 899.99),
    (N'juan@mail.com', N'Demo: office delivery', N'Wireless mouse', 2, 25.00),
    (N'juan@mail.com', N'Demo: delivered', N'Monitor LG 24', 1, 350.00),
    (N'maria@mail.com', N'Demo: special packaging', N'Mechanical keyboard', 1, 120.00),
    (N'carlos@mail.com', N'Demo: out of stock', N'Sony headphones', 2, 75.00),
    (N'maria@mail.com', N'Demo: waiting for dispatch', N'Logitech webcam', 1, 89.99);

INSERT INTO dbo.OrderItems (OrderId, Description, Quantity, UnitPrice)
SELECT orders.Id, source.Description, source.Quantity, source.UnitPrice
FROM @ItemSeed AS source
INNER JOIN dbo.Customers AS customer ON customer.Email = source.CustomerEmail
INNER JOIN dbo.Orders AS orders
    ON orders.CustomerId = customer.Id
   AND orders.Notes = source.OrderNotes
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.OrderItems AS existing
    WHERE existing.OrderId = orders.Id
      AND existing.Description = source.Description
);

COMMIT TRANSACTION;
GO