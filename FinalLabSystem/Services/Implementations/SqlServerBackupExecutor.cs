using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public sealed class SqlServerBackupExecutor : ISqlServerBackupExecutor
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerBackupExecutor> _logger;

    public SqlServerBackupExecutor(string connectionString, ILogger<SqlServerBackupExecutor> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task FullBackupAsync(string databaseName, string targetPath)
    {
        var sql = $@"
            BACKUP DATABASE [{databaseName}]
            TO DISK = @targetPath
            WITH INIT, COMPRESSION, CHECKSUM, STATS = 10,
                 NAME = 'FinalLab Full Backup';";

        await ExecuteBackupCommandAsync(sql, targetPath, databaseName);
    }

    public async Task DifferentialBackupAsync(string databaseName, string targetPath)
    {
        var sql = $@"
            BACKUP DATABASE [{databaseName}]
            TO DISK = @targetPath
            WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, STATS = 10,
                 NAME = 'FinalLab Differential Backup';";

        await ExecuteBackupCommandAsync(sql, targetPath, databaseName);
    }

    public async Task TransactionLogBackupAsync(string databaseName, string targetPath)
    {
        var sql = $@"
            BACKUP LOG [{databaseName}]
            TO DISK = @targetPath
            WITH COMPRESSION, CHECKSUM, STATS = 10,
                 NAME = 'FinalLab Transaction Log Backup';";

        await ExecuteBackupCommandAsync(sql, targetPath, databaseName);
    }

    private async Task ExecuteBackupCommandAsync(string sql, string targetPath, string databaseName)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@targetPath", targetPath);
            command.CommandTimeout = 600;

            await command.ExecuteNonQueryAsync();
            _logger.LogInformation("SQL Server backup completed: {Database} -> {Path}", databaseName, targetPath);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Server backup failed for database {Database}", databaseName);
            throw new InvalidOperationException(
                $"فشل النسخ الاحتياطي لقاعدة البيانات '{databaseName}'. تأكد من صلاحيات SQL Server. التفاصيل: {ex.Message}", ex);
        }
    }
}
