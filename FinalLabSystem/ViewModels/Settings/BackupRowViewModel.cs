using System.ComponentModel;
using System.Runtime.CompilerServices;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class BackupRowViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BackupRowViewModel(BackupMetadataDto dto)
    {
        FileName = dto.FileName;
        FilePath = dto.FilePath;
        DisplaySize = FormatBytes(dto.FileSizeBytes);
        DisplayCreatedAt = dto.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    public string FileName { get; }

    public string FilePath { get; }

    public string DisplaySize { get; }

    public string DisplayCreatedAt { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB"
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
