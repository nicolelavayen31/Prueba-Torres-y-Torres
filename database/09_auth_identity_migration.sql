USE [CustomerOrdersDB];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Users', N'DisplayName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD DisplayName NVARCHAR(80) NULL;
END;
GO

ALTER TABLE dbo.Users ALTER COLUMN Email NVARCHAR(254) NOT NULL;
GO

UPDATE dbo.Users
SET DisplayName = LEFT
(
    CASE
        WHEN CHARINDEX(N'@', Email) > 1 THEN LEFT(Email, CHARINDEX(N'@', Email) - 1)
        ELSE N'User'
    END,
    80
)
WHERE DisplayName IS NULL OR LEN(LTRIM(RTRIM(DisplayName))) = 0;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE DisplayName IS NULL)
BEGIN
    THROW 51000, 'Could not derive a display name for every existing user.', 1;
END;

ALTER TABLE dbo.Users ALTER COLUMN DisplayName NVARCHAR(80) NOT NULL;

IF OBJECT_ID(N'dbo.RefreshSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshSessions
    (
        Id          BIGINT         IDENTITY(1,1) NOT NULL,
        UserId      INT            NOT NULL,
        TokenHash   CHAR(64)       NOT NULL,
        CreatedAt   DATETIME2(7)   NOT NULL,
        ExpiresAt   DATETIME2(7)   NOT NULL,
        RevokedAt   DATETIME2(7)   NULL,

        CONSTRAINT PK_RefreshSessions PRIMARY KEY (Id),
        CONSTRAINT UQ_RefreshSessions_TokenHash UNIQUE (TokenHash),
        CONSTRAINT FK_RefreshSessions_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_RefreshSessions_Expiry CHECK (ExpiresAt > CreatedAt)
    );

    CREATE INDEX IX_RefreshSessions_UserId_ExpiresAt
        ON dbo.RefreshSessions (UserId, ExpiresAt)
        INCLUDE (RevokedAt);
END;

COMMIT TRANSACTION;
GO