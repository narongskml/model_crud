using PortModelApi.Data;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PortModelApi.Services;

public interface IColumnLengthProvider
{
    Task<int?> GetMaxLengthAsync(Type entityType, string propertyName, CancellationToken ct = default);
    int? GetMaxLengthFromModel(Type entityType, string propertyName);
}

public class ColumnLengthProvider : IColumnLengthProvider
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<string, int?> _cache = new();

    public ColumnLengthProvider(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public int? GetMaxLengthFromModel(Type entityType, string propertyName)
    {
        var entity = _context.Model.FindEntityType(entityType);
        var prop = entity?.FindProperty(propertyName);
        return prop?.GetMaxLength();
    }

    public async Task<int?> GetMaxLengthAsync(Type entityType, string propertyName, CancellationToken ct = default)
    {
        var key = $"{entityType.FullName}.{propertyName}";
        if (_cache.TryGetValue(key, out var cached)) return cached;

        // 1) Try EF Core metadata
        var modelLen = GetMaxLengthFromModel(entityType, propertyName);
        if (modelLen.HasValue)
        {
            _cache[key] = modelLen;
            return modelLen;
        }

        // 2) Fall back to database schema (INFORMATION_SCHEMA.COLUMNS) for SQL Server
        var entity = _context.Model.FindEntityType(entityType);
        if (entity == null)
        {
            _cache[key] = null;
            return null;
        }

        var tableName = entity.GetTableName();
        var schema = entity.GetSchema() ?? "dbo";
        var prop = entity.FindProperty(propertyName);
        var columnName = prop?.GetColumnName();
        if (tableName == null || columnName == null)
        {
            _cache[key] = null;
            return null;
        }

        const string sql = @"
SELECT CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table AND COLUMN_NAME = @column";

        var conn = _context.Database.GetDbConnection();
        try
        {
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var pSchema = cmd.CreateParameter(); pSchema.ParameterName = "@schema"; pSchema.Value = schema; cmd.Parameters.Add(pSchema);
            var pTable = cmd.CreateParameter(); pTable.ParameterName = "@table"; pTable.Value = tableName; cmd.Parameters.Add(pTable);
            var pColumn = cmd.CreateParameter(); pColumn.ParameterName = "@column"; pColumn.Value = columnName; cmd.Parameters.Add(pColumn);

            var result = await cmd.ExecuteScalarAsync(ct);
            if (result == null || result == DBNull.Value)
            {
                _cache[key] = null;
                return null;
            }

            if (!int.TryParse(result.ToString(), out var len))
            {
                _cache[key] = null;
                return null;
            }

            // SQL Server returns -1 for MAX (nvarchar(max)); treat as unlimited (null).
            if (len == -1)
            {
                _cache[key] = null;
                return null;
            }

            _cache[key] = len;
            return len;
        }
        finally
        {
            if (conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }
    }
}