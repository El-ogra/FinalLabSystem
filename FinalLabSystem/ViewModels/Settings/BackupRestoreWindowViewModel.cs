using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class BackupRestoreWindowViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;

    private string _targetFolder = string.Empty;
    private DateTime? _lastBackupAt;
    private bool _isBusy;

    public BackupRestoreWindowViewModel(
        IBackupService backupService,
        IDialogService dialogService,
        ICurrentUserSession currentUserSession)
    {
        _backupService = backupService;
        _dialogService = dialogService;
        _currentUserSession = currentUserSession;

        Backups = new ObservableCollection<BackupRowViewModel>();

        LoadBackupsCommand = new AsyncRelayCommand(LoadBackupsAsync, () => !IsBusy);
        CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync, () => !IsBusy);
        RestoreCommand = new AsyncRelayCommand(RestoreAsync, () => !IsBusy);
        BrowseFolderCommand = new AsyncRelayCommand(BrowseFolderAsync);
        OpenFolderCommand = new RelayCommand(_ => OpenFolder());

        _ = InitializeAsync();
    }

    public Action? RequestShutdown { get; set; }

    public ObservableCollection<BackupRowViewModel> Backups { get; }

    public string TargetFolder
    {
        get => _targetFolder;
        set => SetProperty(ref _targetFolder, value);
    }

    public DateTime? LastBackupAt
    {
        get => _lastBackupAt;
        private set => SetProperty(ref _lastBackupAt, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand LoadBackupsCommand { get; }

    public ICommand CreateBackupCommand { get; }

    public ICommand RestoreCommand { get; }

    public ICommand BrowseFolderCommand { get; }

    public ICommand OpenFolderCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            TargetFolder = await _backupService.GetBackupOutputFolderAsync();
        }
        catch
        {
            TargetFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "FinalLabBackups");
        }

        await LoadBackupsAsync();
    }

    private async Task LoadBackupsAsync()
    {
        try
        {
            IsBusy = true;
            var dtos = await _backupService.ListBackupsAsync(TargetFolder);

            Backups.Clear();
            foreach (var dto in dtos)
                Backups.Add(new BackupRowViewModel(dto));

            LastBackupAt = dtos.Count > 0
                ? dtos.Max(d => d.CreatedAt)
                : null;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحميل النسخ الاحتياطية: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateBackupAsync()
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
        {
            _dialogService.ShowError("فقط المسؤولون يمكنهم إنشاء نسخ احتياطية");
            return;
        }

        try
        {
            IsBusy = true;

            var dialog = new BackupPasswordDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true || dialog.EnteredPassword is null)
            {
                IsBusy = false;
                return;
            }

            await _backupService.CreateBackupAsync(TargetFolder, dialog.EnteredPassword, BackupType.Full);

            _dialogService.ShowMessage("تم إنشاء النسخة الاحتياطية بنجاح");
            await LoadBackupsAsync();
        }
        catch (DirectoryNotFoundException)
        {
            _dialogService.ShowError("المجلد غير موجود");
        }
        catch (IOException ex)
        {
            _dialogService.ShowError($"خطأ في الكتابة: {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            _dialogService.ShowError("لا توجد صلاحية كتابة");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ غير متوقع: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RestoreAsync()
    {
        if (_currentUserSession.CurrentUser?.IsAdmin != true)
        {
            _dialogService.ShowError("فقط المسؤولون يمكنهم الاستعادة");
            return;
        }

        var selected = Backups.FirstOrDefault(b => b.IsSelected);
        if (selected is null)
        {
            _dialogService.ShowWarning("يُرجى تحديد نسخة احتياطية");
            return;
        }

        if (!_dialogService.ShowConfirmation(
                "هل أنت متأكد من استعادة هذه النسخة؟ سيتم استبدال جميع البيانات الحالية.",
                "تأكيد الاستعادة"))
            return;

        try
        {
            IsBusy = true;

            var dialog = new BackupPasswordDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true || dialog.EnteredPassword is null)
            {
                IsBusy = false;
                return;
            }

            var success = await _backupService.RestoreBackupAsync(selected.FilePath, dialog.EnteredPassword);

            if (success)
            {
                _dialogService.ShowMessage("تمت الاستعادة بنجاح. يُرجى إعادة تشغيل التطبيق.");

                if (_dialogService.ShowConfirmation("هل تريد إغلاق التطبيق الآن؟"))
                    RequestShutdown?.Invoke();
            }
            else
            {
                _dialogService.ShowError("كلمة المرور غير صحيحة أو الملف تالف");
            }
        }
        catch (DirectoryNotFoundException)
        {
            _dialogService.ShowError("ملف النسخة الاحتياطية غير موجود");
        }
        catch (IOException ex)
        {
            _dialogService.ShowError($"خطأ في القراءة: {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            _dialogService.ShowError("لا توجد صلاحية قراءة");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ غير متوقع: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BrowseFolderAsync()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "اختر مجلد النسخ الاحتياطي",
                FolderName = TargetFolder
            };

            if (dialog.ShowDialog() == true)
            {
                var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
                await _backupService.SaveBackupOutputFolderAsync(dialog.FolderName, staffId);
                TargetFolder = dialog.FolderName;
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تصفح المجلد: {ex.Message}");
        }
    }

    private void OpenFolder()
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", TargetFolder)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"تعذّر فتح المجلد: {ex.Message}");
        }
    }
}
