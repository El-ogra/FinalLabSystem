using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public sealed class SensitiveScreenPasswordService : ISensitiveScreenPasswordService
{
    private const string LegacyCashDrawerKey = "CashDrawer.PasswordHash";

    private readonly FinalLabDbContext _context;
    private readonly ISettingsService _settingsService;

    public SensitiveScreenPasswordService(FinalLabDbContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    public async Task<bool> IsPasswordSetAsync(string screenType)
    {
        var record = await _context.SensitiveScreenPasswords
            .FirstOrDefaultAsync(s => s.ScreenType == screenType);

        if (record != null)
            return !string.IsNullOrEmpty(record.PasswordHash);

        if (screenType == "CashDrawer")
        {
            var legacyHash = await _settingsService.GetSettingValueAsync(LegacyCashDrawerKey);
            return !string.IsNullOrEmpty(legacyHash);
        }

        return false;
    }

    public async Task<bool> VerifyAsync(string screenType, string password)
    {
        var record = await _context.SensitiveScreenPasswords
            .FirstOrDefaultAsync(s => s.ScreenType == screenType);

        if (record != null)
            return PasswordHasher.Verify(password, record.PasswordHash);

        if (screenType == "CashDrawer")
        {
            var legacyHash = await _settingsService.GetSettingValueAsync(LegacyCashDrawerKey);
            if (!string.IsNullOrEmpty(legacyHash))
                return PasswordHasher.Verify(password, legacyHash);
        }

        return false;
    }

    public async Task SetPasswordAsync(string screenType, string newPassword, int staffId)
    {
        var hash = PasswordHasher.Hash(newPassword);
        var existing = await _context.SensitiveScreenPasswords
            .FirstOrDefaultAsync(s => s.ScreenType == screenType);

        if (existing != null)
        {
            existing.PasswordHash = hash;
            existing.LastUpdatedBy = staffId;
            existing.LastUpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.SensitiveScreenPasswords.Add(new SensitiveScreenPassword
            {
                ScreenType = screenType,
                PasswordHash = hash,
                LastUpdatedBy = staffId,
                LastUpdatedAt = DateTime.UtcNow
            });

            if (screenType == "CashDrawer")
            {
                var legacy = await _context.LabSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == LegacyCashDrawerKey);
                if (legacy != null)
                    _context.LabSettings.Remove(legacy);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(string screenType, string currentPassword, string newPassword, int staffId)
    {
        var isValid = await VerifyAsync(screenType, currentPassword);
        if (!isValid)
            throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

        await SetPasswordAsync(screenType, newPassword, staffId);
    }
}
