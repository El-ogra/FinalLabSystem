using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class SecuritySettingsWindowViewModel : ViewModelBase
{
    private readonly ISensitiveScreenPasswordService _sensitivePasswordService;
    private readonly ICurrentUserSession _currentUserSession;

    private bool _cashDrawerPasswordSet;
    private bool _dbMaintenancePasswordSet;
    private bool _settingsPasswordSet;
    private string _statusMessage = string.Empty;

    public SecuritySettingsWindowViewModel(
        ISensitiveScreenPasswordService sensitivePasswordService,
        ICurrentUserSession currentUserSession)
    {
        _sensitivePasswordService = sensitivePasswordService;
        _currentUserSession = currentUserSession;

        ChangeCashDrawerPasswordCommand = new AsyncRelayCommand(ChangeCashDrawerPasswordAsync);
        ChangeDbMaintenancePasswordCommand = new AsyncRelayCommand(ChangeDbMaintenancePasswordAsync);
        ChangeSettingsPasswordCommand = new AsyncRelayCommand(ChangeSettingsPasswordAsync);

        _ = LoadStatusAsync();
    }

    public bool CashDrawerPasswordSet
    {
        get => _cashDrawerPasswordSet;
        private set => SetProperty(ref _cashDrawerPasswordSet, value);
    }

    public bool DbMaintenancePasswordSet
    {
        get => _dbMaintenancePasswordSet;
        private set => SetProperty(ref _dbMaintenancePasswordSet, value);
    }

    public bool SettingsPasswordSet
    {
        get => _settingsPasswordSet;
        private set => SetProperty(ref _settingsPasswordSet, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ChangeCashDrawerPasswordCommand { get; }
    public ICommand ChangeDbMaintenancePasswordCommand { get; }
    public ICommand ChangeSettingsPasswordCommand { get; }

    private async Task LoadStatusAsync()
    {
        try
        {
            CashDrawerPasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("CashDrawer");
            DbMaintenancePasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("DbMaintenance");
            SettingsPasswordSet = await _sensitivePasswordService.IsPasswordSetAsync("Settings");
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ في تحميل حالة كلمات المرور: {ex.Message}";
        }
    }

    private async Task ChangeCashDrawerPasswordAsync()
    {
        await ChangePasswordAsync("CashDrawer", "كلمة مرور درج النقدية");
    }

    private async Task ChangeDbMaintenancePasswordAsync()
    {
        await ChangePasswordAsync("DbMaintenance", "كلمة مرور صيانة قاعدة البيانات");
    }

    private async Task ChangeSettingsPasswordAsync()
    {
        await ChangePasswordAsync("Settings", "كلمة مرور إعدادات النظام");
    }

    private async Task ChangePasswordAsync(string screenType, string displayName)
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId;
        if (staffId == null)
        {
            StatusMessage = "يجب تسجيل الدخول أولاً";
            return;
        }

        try
        {
            var dialog = new ChangeSensitivePasswordDialog
            {
                Owner = Application.Current.MainWindow,
                TitleText = $"تغيير {displayName}",
                ScreenDisplayName = displayName
            };

            if (dialog.ShowDialog() != true || dialog.NewPassword is null)
                return;

            if (await _sensitivePasswordService.IsPasswordSetAsync(screenType))
            {
                if (dialog.CurrentPassword is null)
                {
                    StatusMessage = "يجب إدخال كلمة المرور الحالية";
                    return;
                }

                await _sensitivePasswordService.ChangePasswordAsync(screenType, dialog.CurrentPassword, dialog.NewPassword, staffId.Value);
            }
            else
            {
                await _sensitivePasswordService.SetPasswordAsync(screenType, dialog.NewPassword, staffId.Value);
            }

            StatusMessage = $"تم تغيير {displayName} بنجاح";
            await LoadStatusAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ: {ex.Message}";
        }
    }
}
