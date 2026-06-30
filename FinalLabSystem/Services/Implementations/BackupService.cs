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
        ILogger<BackupService> logger)
    {
        _context = context;
        _currentUserSession = currentUserSession;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type)
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
            throw new UnauthorizedAccessException("Only administrators can perform backup operations.");

        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var entityTypes = _context.Model.GetEntityTypes()
            .Where(t => t.GetTableName() != null && t.GetViewName() == null)
            .ToList();

        var backupData = new Dictionary<string, List<Dictionary<string, object?>>>();

        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName()!;
            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)!
                .MakeGenericMethod(entityType.ClrType);
            var dbSet = setMethod.Invoke(_context, null) as IEnumerable<object>;

            if (dbSet == null) continue;

            var rows = new List<Dictionary<string, object?>>();
            foreach (var entity in dbSet)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var prop in entityType.GetProperties())
                {
                    var value = prop.PropertyInfo?.GetValue(entity);
                    dict[prop.Name] = value;
                }
                rows.Add(dict);
            }
            backupData[tableName] = rows;
        }

        var json = JsonSerializer.Serialize(backupData, JsonOptions);
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        var encryptedBytes = AesEncryptionHelper.Encrypt(jsonBytes, adminPassword);

        var fileName = $"FinalLabSystem_{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.bak.enc";
        var filePath = Path.Combine(targetFolder, fileName);
        await File.WriteAllBytesAsync(filePath, encryptedBytes);

        var staffId = _currentUserSession.CurrentUser!.StaffId;
        await _auditService.LogActionAsync(
            tableName: "Backup",
            recordId: 0,
            action: "B",
            staffId: staffId,
            notes: $"Backup created to {filePath}");

        _logger.LogInformation("Backup created: {FilePath}", filePath);
        return filePath;
    }

    public async Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword)
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
            throw new UnauthorizedAccessException("Only administrators can perform restore operations.");

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
                    notes: $"Backup restored from {backupFilePath}");

                _logger.LogInformation("Backup restored: {BackupFilePath}", backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore backup from {BackupFilePath}", backupFilePath);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup from {BackupFilePath}", backupFilePath);
            return false;
        }
    }

    public Task<List<BackupMetadataDto>> ListBackupsAsync(string folder)
    {
        var backups = new List<BackupMetadataDto>();

        if (!Directory.Exists(folder))
            return Task.FromResult(backups);

        var files = Directory.GetFiles(folder, "*.bak.enc");
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupMetadataDto
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                CreatedAt = fileInfo.CreationTimeUtc,
                FileSizeBytes = fileInfo.Length,
                IsEncrypted = true,
                SchemaVersion = "1.0"
            });
        }

        return Task.FromResult(backups);
    }

    public async Task<bool> ValidateBackupFileAsync(string backupFilePath, string adminPassword)
    {
        try
        {
            if (!File.Exists(backupFilePath))
                return false;

            var encryptedBytes = await File.ReadAllBytesAsync(backupFilePath);
            var jsonBytes = AesEncryptionHelper.Decrypt(encryptedBytes, adminPassword);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var backupData = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object?>>>>(json, JsonOptions);

            return backupData != null;
        }
        catch
        {
            return false;
        }
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
