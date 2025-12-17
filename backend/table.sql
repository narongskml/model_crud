-- ========================================================
-- Create schema 'crd' if it doesn't exist
-- ========================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'crd')
BEGIN
    EXEC('CREATE SCHEMA [crd]');
END
GO

-- ========================================================
-- Drop existing tables (optional: only if you want to reset)
-- ========================================================
-- ⚠️ Uncomment the lines below ONLY if you want to RECREATE from scratch
/*
IF OBJECT_ID('[crd].[port_model_mapping_audit]', 'U') IS NOT NULL
    DROP TABLE [crd].[port_model_mapping_audit];
GO

IF OBJECT_ID('[crd].[port_model_mapping]', 'U') IS NOT NULL
    DROP TABLE [crd].[port_model_mapping];
GO
*/

-- ========================================================
-- Main table: port_model_mapping
-- ========================================================
IF OBJECT_ID('[crd].[port_model_mapping]', 'U') IS NULL
BEGIN
    CREATE TABLE [crd].[port_model_mapping](
        [accno_sleeve] [varchar](50) NOT NULL,
        [effectivedate] [date] NOT NULL,
        [model_name] [varchar](50) NOT NULL,
        [currency_model] [varchar](1) NULL,
        [hedge_model_name] [varchar](50) NULL,

        -- Soft delete flag (your preference)
        [is_deleted] [bit] NOT NULL DEFAULT 0,

        -- Audit columns
        [created_by] [nvarchar](100) NULL,
        [created_at] [datetime2](7) NULL,
        [updated_by] [nvarchar](100) NULL,
        [updated_at] [datetime2](7) NULL,

        CONSTRAINT [PK_port_model_mapping] PRIMARY KEY CLUSTERED 
        (
            [accno_sleeve] ASC,
            [effectivedate] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
END
GO

-- ========================================================
-- Add audit columns if table exists but is missing them
-- (safe to run on existing table)
-- ========================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'is_deleted' 
               AND Object_ID = Object_ID(N'[crd].[port_model_mapping]'))
BEGIN
    ALTER TABLE [crd].[port_model_mapping] 
    ADD [is_deleted] [bit] NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'created_by' 
               AND Object_ID = Object_ID(N'[crd].[port_model_mapping]'))
BEGIN
    ALTER TABLE [crd].[port_model_mapping] 
    ADD [created_by] [nvarchar](100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'created_at' 
               AND Object_ID = Object_ID(N'[crd].[port_model_mapping]'))
BEGIN
    ALTER TABLE [crd].[port_model_mapping] 
    ADD [created_at] [datetime2](7) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'updated_by' 
               AND Object_ID = Object_ID(N'[crd].[port_model_mapping]'))
BEGIN
    ALTER TABLE [crd].[port_model_mapping] 
    ADD [updated_by] [nvarchar](100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE Name = N'updated_at' 
               AND Object_ID = Object_ID(N'[crd].[port_model_mapping]'))
BEGIN
    ALTER TABLE [crd].[port_model_mapping] 
    ADD [updated_at] [datetime2](7) NULL;
END
GO

-- ========================================================
-- Audit log table: port_model_mapping_audit
-- ========================================================
IF OBJECT_ID('[crd].[port_model_mapping_audit]', 'U') IS NULL
BEGIN
    CREATE TABLE [crd].[port_model_mapping_audit](
        [id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [accno_sleeve] [varchar](50) NOT NULL,
        [effectivedate] [date] NOT NULL,
        [model_name] [varchar](50) NULL,
        [currency_model] [varchar](1) NULL,
        [hedge_model_name] [varchar](50) NULL,
        [action] [char](1) NOT NULL, -- 'I'=Insert, 'U'=Update, 'D'=Delete
        [changed_by] [nvarchar](100) NOT NULL,
        [changed_at] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
    ) ON [PRIMARY];
END
GO

-- ========================================================
-- Optional: Add index on audit table for performance
-- ========================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_port_model_mapping_audit_accno_date' 
               AND object_id = OBJECT_ID('[crd].[port_model_mapping_audit]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_port_model_mapping_audit_accno_date] 
    ON [crd].[port_model_mapping_audit] ([accno_sleeve], [effectivedate]);
END
GO

-- ========================================================
-- Success message
-- ========================================================
PRINT '✅ Tables [crd].[port_model_mapping] and [crd].[port_model_mapping_audit] are ready.';