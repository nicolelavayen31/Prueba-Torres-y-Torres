USE [CustomerOrdersDB];
GO

CREATE OR ALTER VIEW dbo.VW_OrdersSummary
AS
SELECT
    Status,
    COUNT_BIG(*) AS TotalOrders,
    SUM(Total) AS Revenue
FROM dbo.Orders
GROUP BY Status;
GO

CREATE OR ALTER VIEW dbo.VW_CustomersActivity
AS
SELECT
    customer.Id,
    customer.Name,
    customer.Email,
    customer.IsActive,
    COUNT_BIG(orders.Id) AS TotalOrders,
    COALESCE(SUM(orders.Total), 0.00) AS TotalSpent
FROM dbo.Customers AS customer
LEFT JOIN dbo.Orders AS orders ON orders.CustomerId = customer.Id
GROUP BY customer.Id, customer.Name, customer.Email, customer.IsActive;
GO

CREATE OR ALTER VIEW dbo.VW_OrdersByDay
AS
SELECT
    CONVERT(DATE, CreatedAt) AS OrderDate,
    COUNT_BIG(*) AS TotalOrders,
    SUM(Total) AS DailyRevenue
FROM dbo.Orders
WHERE CreatedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY CONVERT(DATE, CreatedAt);
GO