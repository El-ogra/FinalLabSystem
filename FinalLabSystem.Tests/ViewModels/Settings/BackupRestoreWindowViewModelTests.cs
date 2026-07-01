using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class BackupRestoreWindowViewModelTests
{
    private static Staff CreateAdminStaff() => new()
    {
        StaffId = 1,
        DisplayName = "Admin",
        IsAdmin = true
    };

    private static Staff CreateNonAdminStaff() => new()
    {
        StaffId = 2,
        DisplayName = "Staff",
        IsAdmin = false
    };

    private static (BackupRestoreWindowViewModel vm, Mock<IBackupService> mockBackup,
        Mock<IDialogService> mockDialog, Mock<ICurrentUserSession> mockSession,
        Mock<IProcessService> mockProcess)
        CreateViewModel(Staff? staff = null)
    {
        var mockBackup = new Mock<IBackupService>();
        var mockDialog = new Mock<IDialogService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockProcess = new Mock<IProcessService>();

        mockBackup.Setup(s => s.GetBackupOutputFolderAsync())
            .ReturnsAsync(@"C:\TestBackups");
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupMetadataDto>());

        mockSession.Setup(s => s.CurrentUser).Returns(staff);

        var vm = new BackupRestoreWindowViewModel(
            mockBackup.Object,
            mockDialog.Object,
            mockSession.Object,
            mockProcess.Object);

        return (vm, mockBackup, mockDialog, mockSession, mockProcess);
    }

    [Fact]
    public void LoadBackupsCommand_PopulatesCollection_FromService()
    {
        var (vm, mockBackup, _, _, _) = CreateViewModel();
        var dtos = new List<BackupMetadataDto>
        {
            new() { FileName = "b1.bak", FilePath = @"C:\b1.bak", FileSizeBytes = 100, CreatedAt = DateTime.UtcNow },
            new() { FileName = "b2.bak", FilePath = @"C:\b2.bak", FileSizeBytes = 200, CreatedAt = DateTime.UtcNow },
            new() { FileName = "b3.bak", FilePath = @"C:\b3.bak", FileSizeBytes = 300, CreatedAt = DateTime.UtcNow }
        };
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(dtos);

        vm.LoadBackupsCommand.Execute(null);

        Assert.Equal(3, vm.Backups.Count);
        Assert.Equal("b1.bak", vm.Backups[0].FileName);
        Assert.Equal("b2.bak", vm.Backups[1].FileName);
        Assert.Equal("b3.bak", vm.Backups[2].FileName);
    }

    [Fact]
    public void CreateBackupCommand_NonAdmin_ShowsError_DoesNotCallService()
    {
        var (vm, mockBackup, mockDialog, _, _) = CreateViewModel(CreateNonAdminStaff());

        vm.CreateBackupCommand.Execute(null);

        mockDialog.Verify(d => d.ShowError(
            It.Is<string>(s => s.Contains("المسؤولون")),
            It.IsAny<string>()), Times.Once);
        mockBackup.Verify(s => s.CreateBackupAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BackupType>()), Times.Never);
    }

    [Fact]
    public void RestoreCommand_NoSelection_ShowsWarning()
    {
        var (vm, _, mockDialog, _, _) = CreateViewModel(CreateAdminStaff());

        vm.RestoreCommand.Execute(null);

        mockDialog.Verify(d => d.ShowWarning(
            It.Is<string>(s => s.Contains("تحديد")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void RestoreCommand_NonAdmin_ShowsError()
    {
        var (vm, _, mockDialog, _, _) = CreateViewModel(CreateNonAdminStaff());

        vm.RestoreCommand.Execute(null);

        mockDialog.Verify(d => d.ShowError(
            It.Is<string>(s => s.Contains("المسؤولون")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void RestoreCommand_UserCancelsConfirmation_DoesNotCallService()
    {
        var (vm, mockBackup, mockDialog, _, _) = CreateViewModel(CreateAdminStaff());

        vm.Backups.Add(new BackupRowViewModel(new BackupMetadataDto
        {
            FileName = "test.bak",
            FilePath = @"C:\test.bak",
            FileSizeBytes = 100,
            CreatedAt = DateTime.UtcNow
        }));
        vm.Backups[0].IsSelected = true;

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.RestoreCommand.Execute(null);

        mockBackup.Verify(s => s.RestoreBackupAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OpenFolderCommand_DoesNotThrow()
    {
        var (vm, _, _, _, mockProcess) = CreateViewModel();
        vm.TargetFolder = @"C:\Windows";

        vm.OpenFolderCommand.Execute(null);

        mockProcess.Verify(p => p.OpenFolder(@"C:\Windows"), Times.Once);
    }

    [Fact]
    public void IsBusy_FalseAfterInitializationCompletes()
    {
        var (vm, _, _, _, _) = CreateViewModel();

        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void TargetFolder_SetsCorrectly()
    {
        var (vm, _, _, _, _) = CreateViewModel();

        vm.TargetFolder = @"D:\NewBackups";

        Assert.Equal(@"D:\NewBackups", vm.TargetFolder);
    }

    [Fact]
    public void Backups_InitiallyEmpty()
    {
        var (vm, _, _, _, _) = CreateViewModel();

        Assert.Empty(vm.Backups);
    }

    [Fact]
    public void RequestShutdown_DefaultIsNull()
    {
        var (vm, _, _, _, _) = CreateViewModel();

        Assert.Null(vm.RequestShutdown);
    }

    [Fact]
    public void LoadBackupsCommand_SetsLastBackupAt()
    {
        var (vm, mockBackup, _, _, _) = CreateViewModel();
        var later = DateTime.UtcNow;
        var earlier = later.AddHours(-1);
        var dtos = new List<BackupMetadataDto>
        {
            new() { FileName = "old.bak", FilePath = @"C:\old.bak", FileSizeBytes = 100, CreatedAt = earlier },
            new() { FileName = "new.bak", FilePath = @"C:\new.bak", FileSizeBytes = 200, CreatedAt = later }
        };
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(dtos);

        vm.LoadBackupsCommand.Execute(null);

        Assert.NotNull(vm.LastBackupAt);
    }

    [Fact]
    public void LoadBackupsCommand_EmptyList_LastBackupAtIsNull()
    {
        var (vm, mockBackup, _, _, _) = CreateViewModel();
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupMetadataDto>());

        vm.LoadBackupsCommand.Execute(null);

        Assert.Null(vm.LastBackupAt);
    }

    [Fact]
    public void LoadBackupsCommand_ServiceException_ShowsError()
    {
        var (vm, mockBackup, mockDialog, _, _) = CreateViewModel();
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ThrowsAsync(new IOException("disk error"));

        vm.LoadBackupsCommand.Execute(null);

        mockDialog.Verify(d => d.ShowError(
            It.Is<string>(s => s.Contains("خطأ")),
            It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void InitializeAsync_TargetFolderSetFromService()
    {
        var mockBackup = new Mock<IBackupService>();
        mockBackup.Setup(s => s.GetBackupOutputFolderAsync())
            .ReturnsAsync(@"E:\CustomBackup");
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupMetadataDto>());

        var vm = new BackupRestoreWindowViewModel(
            mockBackup.Object,
            Mock.Of<IDialogService>(),
            Mock.Of<ICurrentUserSession>(),
            Mock.Of<IProcessService>());

        Assert.Equal(@"E:\CustomBackup", vm.TargetFolder);
    }

    [Fact]
    public void InitializeAsync_ServiceFallsBackToDefaultFolder()
    {
        var mockBackup = new Mock<IBackupService>();
        mockBackup.Setup(s => s.GetBackupOutputFolderAsync())
            .ThrowsAsync(new Exception("service error"));
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupMetadataDto>());

        var vm = new BackupRestoreWindowViewModel(
            mockBackup.Object,
            Mock.Of<IDialogService>(),
            Mock.Of<ICurrentUserSession>(),
            Mock.Of<IProcessService>());

        Assert.Contains("FinalLabBackups", vm.TargetFolder);
    }

    [Fact]
    public void RestoreCommand_AdminWithSelection_NoError()
    {
        var (vm, mockBackup, mockDialog, _, _) = CreateViewModel(CreateAdminStaff());

        vm.Backups.Add(new BackupRowViewModel(new BackupMetadataDto
        {
            FileName = "test.bak",
            FilePath = @"C:\test.bak",
            FileSizeBytes = 100,
            CreatedAt = DateTime.UtcNow
        }));
        vm.Backups[0].IsSelected = true;

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.RestoreCommand.Execute(null);

        mockDialog.Verify(d => d.ShowError(
            It.Is<string>(s => s.Contains("المسؤولون")),
            It.IsAny<string>()), Times.Never);
    }
}
