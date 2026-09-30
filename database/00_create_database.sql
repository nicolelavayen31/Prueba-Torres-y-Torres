USE master;
GO

IF DB_ID(N'CustomerOrdersDB') IS NULL
BEGIN
    CREATE DATABASE [CustomerOrdersDB];
END
GO

USE [CustomerOrdersDB];
GO