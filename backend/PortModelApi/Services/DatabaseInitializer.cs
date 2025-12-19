using Microsoft.Data.SqlClient;
using System.Text;

namespace PortModelApi.Services;

public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        _logger.LogInformation("Starting database initialization...");

        try
        {
            // Test connection
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("✅ Database connection successful!");

            // Check if schema exists
            var schemaExists = await CheckSchemaExistsAsync(connection);
            if (!schemaExists)
            {
                _logger.LogInformation("Creating schema [crd]...");
                await CreateSchemaAsync(connection);
            }

            // Check if tables exist
            var mainTableExists = await CheckTableExistsAsync(connection, "crd", "port_model_mapping");
            var auditTableExists = await CheckTableExistsAsync(connection, "crd", "port_model_mapping_audit");

            if (!mainTableExists || !auditTableExists)
            {
                _logger.LogWarning("Tables not found. Creating tables automatically...");
                await CreateTablesAsync(connection);
                _logger.LogInformation("✅ Tables created successfully!");
            }
            else
            {
                _logger.LogInformation("✅ All required tables already exist.");
                
                // Check for missing columns and add them
                _logger.LogInformation("Checking for missing columns...");
                await MigrateTableColumnsAsync(connection);
            }

            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Database initialization failed: {Message}", ex.Message);
            throw;
        }
    }

    private async Task<bool> CheckSchemaExistsAsync(SqlConnection connection)
    {
        var sql = "SELECT COUNT(*) FROM sys.schemas WHERE name = 'crd'";
        await using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        var count = result != null ? Convert.ToInt32(result) : 0;
        return count > 0;
    }

    private async Task CreateSchemaAsync(SqlConnection connection)
    {
        var sql = "CREATE SCHEMA [crd]";
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<bool> CheckTableExistsAsync(SqlConnection connection, string schema, string tableName)
    {
        var sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}'";
        await using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        var count = result != null ? Convert.ToInt32(result) : 0;
        return count > 0;
    }

    private async Task CreateTablesAsync(SqlConnection connection)
    {
        try
        {
            // Create schema first
            _logger.LogInformation("Creating schema [crd]...");
            await ExecuteSqlAsync(connection, @"
                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'crd')
                BEGIN
                    EXEC('CREATE SCHEMA [crd]');
                END
            ");

            // Create main table
            _logger.LogInformation("Creating table [crd].[port_model_mapping]...");
            await ExecuteSqlAsync(connection, @"
                IF OBJECT_ID('[crd].[port_model_mapping]', 'U') IS NULL
                BEGIN
                    CREATE TABLE [crd].[port_model_mapping](
                        [accno_sleeve] [varchar](50) NOT NULL,
                        [effectivedate] [date] NOT NULL,
                        [model_name] [nvarchar](100) NOT NULL,
                        [currency_model] [varchar](1) NULL,
                        [hedge_model_name] [nvarchar](100) NULL,
                        [is_deleted] [bit] NOT NULL DEFAULT 0,
                        [created_by] [nvarchar](50) NULL,
                        [created_at] [datetime2](7) NULL,
                        [updated_by] [nvarchar](50) NULL,
                        [updated_at] [datetime2](7) NULL,
                        [deleted_by] [nvarchar](50) NULL,
                        [deleted_at] [datetime2](7) NULL,
                        CONSTRAINT [PK_port_model_mapping] PRIMARY KEY CLUSTERED 
                        (
                            [accno_sleeve] ASC,
                            [effectivedate] ASC
                        )
                    );
                END
            ");

            // Create audit table
            _logger.LogInformation("Creating table [crd].[port_model_mapping_audit]...");
            await ExecuteSqlAsync(connection, @"
                IF OBJECT_ID('[crd].[port_model_mapping_audit]', 'U') IS NULL
                BEGIN
                    CREATE TABLE [crd].[port_model_mapping_audit](
                        [id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [accno_sleeve] [varchar](50) NOT NULL,
                        [effectivedate] [date] NOT NULL,
                        [model_name] [varchar](50) NULL,
                        [currency_model] [varchar](1) NULL,
                        [hedge_model_name] [varchar](50) NULL,
                        [action] [char](1) NOT NULL,
                        [changed_by] [nvarchar](50) NOT NULL,
                        [changed_at] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
                    );
                END
            ");

            // Create index
            _logger.LogInformation("Creating index on audit table...");
            await ExecuteSqlAsync(connection, @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE name = 'IX_port_model_mapping_audit_accno_date' 
                               AND object_id = OBJECT_ID('[crd].[port_model_mapping_audit]'))
                BEGIN
                    CREATE NONCLUSTERED INDEX [IX_port_model_mapping_audit_accno_date] 
                    ON [crd].[port_model_mapping_audit] ([accno_sleeve], [effectivedate]);
                END
            ");

            _logger.LogInformation("All database objects created successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tables: {Message}", ex.Message);
            throw;
        }
    }

    private async Task ExecuteSqlAsync(SqlConnection connection, string sql)
    {
        try
        {
            await using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 60;
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL execution failed: {Message}", ex.Message);
            throw;
        }
    }

    private async Task MigrateTableColumnsAsync(SqlConnection connection)
    {
        // Define expected columns for port_model_mapping
        var expectedMainTableColumns = new Dictionary<string, string>
        {
            { "accno_sleeve", "varchar(50)" },
            { "effectivedate", "date" },
            { "model_name", "nvarchar(100)" },
            { "currency_model", "varchar(1)" },
            { "hedge_model_name", "nvarchar(100)" },
            { "is_deleted", "bit" },
            { "created_by", "nvarchar(50)" },
            { "created_at", "datetime2(7)" },
            { "updated_by", "nvarchar(50)" },
            { "updated_at", "datetime2(7)" },
            { "deleted_by", "nvarchar(50)" },
            { "deleted_at", "datetime2(7)" }
        };

        // Define expected columns for port_model_mapping_audit
        var expectedAuditTableColumns = new Dictionary<string, string>
        {
            { "id", "bigint" },
            { "accno_sleeve", "varchar(50)" },
            { "effectivedate", "date" },
            { "model_name", "varchar(50)" },
            { "currency_model", "varchar(1)" },
            { "hedge_model_name", "varchar(50)" },
            { "action", "char(1)" },
            { "changed_by", "nvarchar(50)" },
            { "changed_at", "datetime2(7)" }
        };

        // Check and migrate main table
        await CheckAndAddMissingColumnsAsync(connection, "crd", "port_model_mapping", expectedMainTableColumns);
        
        // Check and migrate audit table
        await CheckAndAddMissingColumnsAsync(connection, "crd", "port_model_mapping_audit", expectedAuditTableColumns);
    }

    private async Task CheckAndAddMissingColumnsAsync(SqlConnection connection, string schema, string tableName, Dictionary<string, string> expectedColumns)
    {
        // Get existing columns
        var existingColumns = await GetExistingColumnsAsync(connection, schema, tableName);
        
        var missingColumns = expectedColumns.Keys.Except(existingColumns, StringComparer.OrdinalIgnoreCase).ToList();
        
        if (missingColumns.Any())
        {
            _logger.LogWarning($"Found {missingColumns.Count} missing column(s) in [{schema}].[{tableName}]: {string.Join(", ", missingColumns)}");
            
            foreach (var columnName in missingColumns)
            {
                var columnType = expectedColumns[columnName];
                await AddColumnAsync(connection, schema, tableName, columnName, columnType);
                _logger.LogInformation($"✅ Added column [{columnName}] to [{schema}].[{tableName}]");
            }
        }
        else
        {
            _logger.LogInformation($"✅ [{schema}].[{tableName}] - All columns present");
        }
    }

    private async Task<List<string>> GetExistingColumnsAsync(SqlConnection connection, string schema, string tableName)
    {
        var sql = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @tableName";
        
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@tableName", tableName);
        
        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }
        
        return columns;
    }

    private async Task AddColumnAsync(SqlConnection connection, string schema, string tableName, string columnName, string columnType)
    {
        // Determine if column should be nullable and have default value
        var nullable = "NULL";
        var defaultValue = "";
        
        // Special handling for specific columns
        if (columnName.Equals("is_deleted", StringComparison.OrdinalIgnoreCase))
        {
            nullable = "NOT NULL";
            defaultValue = " DEFAULT 0";
        }
        else if (columnName.Equals("changed_at", StringComparison.OrdinalIgnoreCase))
        {
            nullable = "NOT NULL";
            defaultValue = " DEFAULT GETUTCDATE()";
        }
        
        var sql = $"ALTER TABLE [{schema}].[{tableName}] ADD [{columnName}] {columnType} {nullable}{defaultValue}";
        
        try
        {
            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to add column [{columnName}] to [{schema}].[{tableName}]: {ex.Message}");
            throw;
        }
    }
}
