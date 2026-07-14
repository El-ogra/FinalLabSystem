using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public class BackupService : IBackupService
{
    private readonly FinalLabDbContext _context;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IAuditService _auditService;
    private readonly ISensitiveScreenPasswordService _sensitivePasswordService;
    private readonly ISqlServerBackupExecutor _backupExecutor;
    private readonly ISqlServerRestoreExecutor _restoreExecutor;
    private readonly IBackupFileNameStrategy _fileNameStrategy;
    private readonly ILogger<BackupService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    public BackupService(
        FinalLabDbContext context,
        ICurrentUserSession currentUserSession,
        IAuditService auditService,
        ISensitiveScreenPasswordService sensitivePasswordService,
        ISqlServerBackupExecutor backupExecutor,
        ISqlServerRestoreExecutor restoreExecutor,
        IBackupFileNameStrategy fileNameStrategy,
        ILogger<BackupService> logger)
    {
        _context = context;
        _currentUserSession = currentUserSession;
        _auditService = auditService;
        _sensitivePasswordService = sensitivePasswordService;
        _backupExecutor = backupExecutor;
        _restoreExecutor = restoreExecutor;
        _fileNameStrategy = fileNameStrategy;
        _logger = logger;
    }

    protected virtual string GetDatabaseName()
    {
        return _context.Database.GetDbConnection().Database;
    }

    public async Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type)
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
            throw new UnauthorizedAccessException("Only administrators can perform backup operations.");

        var dbPasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("DbMaintenance");
        if (dbPasswordSet)
        {
            var isValid = await _sensitivePasswordService.VerifyAsync("DbMaintenance", adminPassword);
            if (!isValid)
                throw new UnauthorizedAccessException("كلمة مرور صيانة قاعدة البيانات غير صحيحة.");
        }

        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var databaseName = GetDatabaseName();
        var fileName = _fileNameStrategy.GenerateFileName(type);
        var targetPath = Path.Combine(targetFolder, fileName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            switch (type)
            {
                case BackupType.Full:
                    await _backupExecutor.FullBackupAsync(databaseName, targetPath);
                    break;
                case BackupType.Incremental:
                    await _backupExecutor.DifferentialBackupAsync(databaseName, targetPath);
                    break;
                default:
                    await _backupExecutor.FullBackupAsync(databaseName, targetPath);
                    break;
            }

            stopwatch.Stop();

            long fileSize = 0;
            if (File.Exists(targetPath))
                fileSize = new FileInfo(targetPath).Length;

            var staffId = _currentUserSession.CurrentUser!.StaffId;
            await _auditService.LogActionAsync(
                tableName: "Backup",
                recordId: 0,
                action: "B",
                staffId: staffId,
                notes: $"SQL Server backup ({type}) created to {targetPath}. Size: {fileSize} bytes. Duration: {stopwatch.Elapsed.TotalSeconds:F1}s.");

            _logger.LogInformation("SQL Server backup created: {FilePath} ({Type})", targetPath, type);
            return targetPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var staffId = _currentUserSession.CurrentUser!.StaffId;
            await _auditService.LogActionAsync(
                tableName: "Backup",
                recordId: 0,
                action: "B",
                staffId: staffId,
                notes: $"SQL Server backup FAILED ({type}). Error: {ex.Message}. Duration: {stopwatch.Elapsed.TotalSeconds:F1}s.");

            _logger.LogError(ex, "SQL Server backup failed ({Type})", type);
            throw;
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword)
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
            throw new UnauthorizedAccessException("Only administrators can perform restore operations.");

        var dbPasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("DbMaintenance");
        if (dbPasswordSet)
        {
            var isValid = await _sensitivePasswordService.VerifyAsync("DbMaintenance", adminPassword);
            if (!isValid)
                throw new UnauthorizedAccessException("كلمة مرور صيانة قاعدة البيانات غير صحيحة.");
        }

        if (!File.Exists(backupFilePath))
            return false;

        var databaseName = GetDatabaseName();

        try
        {
            await _restoreExecutor.SetSingleUserAsync(databaseName);
            await _restoreExecutor.RestoreAsync(databaseName, backupFilePath);
            await _restoreExecutor.SetMultiUserAsync(databaseName);

            var staffId = _currentUserSession.CurrentUser!.StaffId;
            await _auditService.LogActionAsync(
                tableName: "Backup",
                recordId: 0,
                action: "R",
                staffId: staffId,
                notes: $"SQL Server restore completed from {backupFilePath}");

            _logger.LogInformation("SQL Server restore completed: {BackupFilePath}", backupFilePath);
            return true;
        }
        catch (Exception ex)
        {
            try
            {
                await _restoreExecutor.SetMultiUserAsync(databaseName);
            }
            catch (Exception restoreEx)
            {
                _logger.LogError(restoreEx, "Failed to set database back to multi-user mode after restore failure");
            }

            var staffId = _currentUserSession.CurrentUser!.StaffId;
            await _auditService.LogActionAsync(
                tableName: "Backup",
                recordId: 0,
                action: "R",
                staffId: staffId,
                notes: $"SQL Server restore FAILED from {backupFilePath}. Error: {ex.Message}");

            _logger.LogError(ex, "SQL Server restore failed from {BackupFilePath}", backupFilePath);
            return false;
        }
    }

    public async Task<bool> RestoreLegacyJsonBackupAsync(string backupFilePath, string adminPassword)
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
            throw new UnauthorizedAccessException("Only administrators can perform restore operations.");

        var dbPasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("DbMaintenance");
        if (dbPasswordSet)
        {
            var isValid = await _sensitivePasswordService.VerifyAsync("DbMaintenance", adminPassword);
            if (!isValid)
                throw new UnauthorizedAccessException("كلمة مرور صيانة قاعدة البيانات غير صحيحة.");
        }

        if (!File.Exists(backupFilePath))
            return false;

        try
        {
            var encryptedBytes = await File.ReadAllBytesAsync(backupFilePath);
            var jsonBytes = AesEncryptionHelper.Decrypt(encryptedBytes, adminPassword);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var backupData = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object?>>>>(json, JsonOptions);

            if (backupData == null)
                return false;

            var entityTypes = _context.Model.GetEntityTypes()
                .Where(t => t.GetTableName() != null && t.GetViewName() == null)
                .ToList();

            var sortedTypes = TopologicalSort(entityTypes);

            try
            {
                foreach (var entityType in sortedTypes.AsEnumerable().Reverse())
                {
                    var tableName = entityType.GetTableName()!;
                    var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)!
                        .MakeGenericMethod(entityType.ClrType);
                    var dbSet = setMethod.Invoke(_context, null);

                    if (dbSet == null) continue;

                    var toListMethod = typeof(Enumerable).GetMethod("ToList")!
                        .MakeGenericMethod(entityType.ClrType);
                    var allItems = toListMethod.Invoke(null, new[] { dbSet });

                    if (allItems != null)
                    {
                        var removeMethod = dbSet.GetType().GetMethod("Remove")!;
                        var enumerator = (allItems as System.Collections.IEnumerable)!.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current != null)
                                removeMethod.Invoke(dbSet, new[] { enumerator.Current });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();

                foreach (var entityType in sortedTypes)
                {
                    var tableName = entityType.GetTableName()!;
                    if (!backupData.ContainsKey(tableName)) continue;

                    var rows = backupData[tableName];
                    var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)!
                        .MakeGenericMethod(entityType.ClrType);
                    var dbSet = setMethod.Invoke(_context, null);

                    if (dbSet == null) continue;

                    var addMethod = dbSet.GetType().GetMethod("Add")!;
                    foreach (var row in rows)
                    {
                        var entity = Activator.CreateInstance(entityType.ClrType);
                        foreach (var prop in entityType.GetProperties())
                        {
                            if (row.TryGetValue(prop.Name, out var value) && value != null)
                            {
                                try
                                {
                                    object? convertedValue;
                                    if (value is JsonElement jsonElement)
                                    {
                                        var targetType = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
                                        convertedValue = jsonElement.Deserialize(targetType);
                                    }
                                    else
                                    {
                                        var targetType = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
                                        convertedValue = Convert.ChangeType(value, targetType);
                                    }
                                    prop.PropertyInfo?.SetValue(entity, convertedValue);
                                }
                                catch
                                {
                                    prop.PropertyInfo?.SetValue(entity, value);
                                }
                            }
                        }
                        addMethod.Invoke(dbSet, new[] { entity! });
                    }

                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                }

                var staffId = _currentUserSession.CurrentUser!.StaffId;
                await _auditService.LogActionAsync(
                    tableName: "Backup",
                    recordId: 0,
                    action: "R",
                    staffId: staffId,
                    notes: $"Legacy JSON backup restored from {backupFilePath}");

                _logger.LogInformation("Legacy JSON backup restored: {BackupFilePath}", backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore legacy JSON backup from {BackupFilePath}", backupFilePath);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt/deserialize legacy backup from {BackupFilePath}", backupFilePath);
            return false;
        }
    }

    public Task<List<BackupMetadataDto>> ListBackupsAsync(string folder)
    {
        var backups = new List<BackupMetadataDto>();

        if (!Directory.Exists(folder))
            return Task.FromResult(backups);

        var files = Directory.GetFiles(folder, "*.bak");
        files = files.Concat(Directory.GetFiles(folder, "*.bak.enc")).ToArray();

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupMetadataDto
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                CreatedAt = fileInfo.CreationTimeUtc,
                FileSizeBytes = fileInfo.Length,
                IsEncrypted = fileInfo.Extension == ".enc",
                SchemaVersion = fileInfo.Extension == ".enc" ? "1.0" : "2.0"
            });
        }

        return Task.FromResult(backups);
    }

    public async Task<string> GetBackupOutputFolderAsync()
    {
        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "DefaultBackupPath");
        return setting?.SettingValue
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalLabBackups");
    }

    public async Task SaveBackupOutputFolderAsync(string folderPath, int staffId)
    {
        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "DefaultBackupPath");

        if (setting is null)
        {
            setting = new Models.LabSetting
            {
                SettingKey = "DefaultBackupPath",
                SettingValue = folderPath,
                LastUpdatedBy = staffId,
                LastUpdatedAt = DateTime.UtcNow
            };
            _context.LabSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = folderPath;
            setting.LastUpdatedBy = staffId;
            setting.LastUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private List<IEntityType> TopologicalSort(List<IEntityType> entityTypes)
    {
        var sorted = new List<IEntityType>();
        var visited = new HashSet<Type>();
        var visiting = new HashSet<Type>();

        foreach (var entityType in entityTypes)
        {
            VisitEntityType(entityType, sorted, visited, visiting, entityTypes);
        }

        return sorted;
    }

    private void VisitEntityType(
        IEntityType entityType,
        List<IEntityType> sorted,
        HashSet<Type> visited,
        HashSet<Type> visiting,
        List<IEntityType> allTypes)
    {
        var clrType = entityType.ClrType;
        if (visited.Contains(clrType))
            return;

        if (visiting.Contains(clrType))
            return;

        visiting.Add(clrType);

        var foreignKeys = entityType.GetForeignKeys();
        foreach (var fk in foreignKeys)
        {
            var principalType = fk.PrincipalEntityType;
            if (principalType != entityType && allTypes.Contains(principalType))
            {
                VisitEntityType(principalType, sorted, visited, visiting, allTypes);
            }
        }

        visiting.Remove(clrType);
        visited.Add(clrType);
        sorted.Add(entityType);
    }
}
