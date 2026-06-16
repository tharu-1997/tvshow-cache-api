

-- database creation 
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TvShowCacheDb')
BEGIN
    CREATE DATABASE TvShowCacheDb;
    PRINT 'Database TvShowCacheDb created.';
END
GO

USE TvShowCacheDb;
GO

-- TvShows table creation
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TvShows') AND type = 'U')
BEGIN
    CREATE TABLE dbo.TvShows
    (
        Id              INT             NOT NULL PRIMARY KEY,   
        Name            NVARCHAR(200)   NOT NULL,
        Status          NVARCHAR(50)    NOT NULL DEFAULT '',    
        Language        NVARCHAR(50)    NOT NULL DEFAULT '',
        Genres          NVARCHAR(500)   NOT NULL DEFAULT '',   
        Network         NVARCHAR(200)   NOT NULL DEFAULT '',
        Rating          FLOAT           NULL,                   
        OfficialSite    NVARCHAR(500)   NOT NULL DEFAULT '',
        Summary         NVARCHAR(MAX)   NOT NULL DEFAULT '',    
        ImageUrl        NVARCHAR(500)   NOT NULL DEFAULT '',
        CachedAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE()
    );

    -- Index on Name for fast name lookups
    CREATE INDEX IX_TvShows_Name ON dbo.TvShows (Name);

    PRINT 'Table dbo.TvShows created with index.';
END
GO

PRINT 'Schema setup complete. Database: TvShowCacheDb, Table: dbo.TvShows';
GO