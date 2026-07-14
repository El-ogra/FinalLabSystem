using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public sealed class SqlServerRestoreExecutor : ISqlServerRestoreExecutor
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerRestoreExecutor> _logger;

    public SqlServerRestoreExecutor(string connectionString, ILogger<SqlServerRestoreExecutor> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task SetSingleUserAsync(string databaseName)
    {
        var sql = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 60;

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Database {Database} set to single-user mode", databaseName);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to set database {Database} to single-user mode", databaseName);
            throw new InvalidOperationException(
                $"فشل تحويل قاعدة البيانات '{databaseName}' إلى وضع المستخدم الواحد. تأكد من عدم وجود اتصالات نشطة. التفاصيل: {ex.Message}", ex);
        }
    }

    public async Task RestoreAsync(string databaseName, string sourcePath)
    {
        var sql = $@"
            RESTORE DATABASE [{databaseName}]
            FROM DISK = @sourcePath
            WITH REPLACE, RECOVERY;";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@sourcePath", sourcePath);
            command.CommandTimeout = 600;

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Database {Database} restored from {Path}", databaseName, sourcePath);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to restore database {Database} from {Path}", databaseName, sourcePath);
            throw new InvalidOperationException(
                $"فشل استعادة قاعدة البيانات '{databaseName}'. التفاصيل: {ex.Message}", ex);
        }
    }

    public async Task SetMultiUserAsync(string databaseName)
    {
        var sql = $"ALTER DATABASE [{databaseName}] SET MULTI_USER;";

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 60;

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("Database {Database} set to multi-user mode", databaseName);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to set database {Database} to multi-user mode", databaseName);
            throw new InvalidOperationException(
                $"فشل تحويل قاعدة البيانات '{databaseName}' إلى وضع متعدد المستخدمين. التفاصيل: {ex.Message}", ex);
        }
    }
}
