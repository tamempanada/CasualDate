using CasualDate.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CasualDate.Services;

public class DatabaseSchemaService
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<string, bool> _tableExistsCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _columnExistsCache = new(StringComparer.OrdinalIgnoreCase);

    public DatabaseSchemaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        if (_tableExistsCache.TryGetValue(tableName, out var exists))
        {
            return exists;
        }

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction is not null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            command.CommandText = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = @tableName;
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            exists = Convert.ToInt32(result) > 0;
            _tableExistsCache[tableName] = exists;
            return exists;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<bool> ColumnExistsAsync(string tableName, string columnName)
    {
        var cacheKey = $"{tableName}.{columnName}";
        if (_columnExistsCache.TryGetValue(cacheKey, out var exists))
        {
            return exists;
        }

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction is not null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            command.CommandText = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = @tableName
                    AND COLUMN_NAME = @columnName;
                """;

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var columnParameter = command.CreateParameter();
            columnParameter.ParameterName = "@columnName";
            columnParameter.Value = columnName;
            command.Parameters.Add(columnParameter);

            var result = await command.ExecuteScalarAsync();
            exists = Convert.ToInt32(result) > 0;
            _columnExistsCache[cacheKey] = exists;
            return exists;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
