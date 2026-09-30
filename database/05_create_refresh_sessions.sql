USE [CustomerOrdersDB];
GO

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
END
GO