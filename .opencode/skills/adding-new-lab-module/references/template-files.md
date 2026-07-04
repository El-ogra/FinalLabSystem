# Module template files

Minimal copy-paste starters for each artifact. Replace `<Entity>` and `<Area>` placeholders.

## Entity template

```csharp
using FinalLabSystem.Data;
namespace FinalLabSystem.Models;

[Auditable]
public partial class <Entity>
{
    public int <Entity>Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

## DTO template

```csharp
namespace FinalLabSystem.Models.DTOs;
public record <Entity>Dto(
    int <Entity>Id,
    string Code,
    string NameAr,
    string? NameEn,
    string Status);
```

## Service interface template

```csharp
using FinalLabSystem.Models.DTOs;
namespace FinalLabSystem.Services.Interfaces;

public interface I<Entity>Service
{
    Task<List<<Entity>Dto>> GetAllAsync();
    Task<<Entity>Dto?> GetByIdAsync(int id);
    Task<int> CreateAsync(<Entity>Dto dto, int staffId);
    Task UpdateAsync(int id, <Entity>Dto dto, int staffId);
    Task DeleteAsync(int id, int staffId);
}
```

## Service implementation template

```csharp
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public sealed class <Entity>Service : I<Entity>Service
{
    private readonly FinalLabDbContext _db;
    private readonly ICurrentUserSession _session;
    private readonly IAuditService _audit;
    private readonly ILogger<<Entity>Service> _log;

    public <Entity>Service(FinalLabDbContext db, ICurrentUserSession session,
        IAuditService audit, ILogger<<Entity>Service> log)
    { _db = db; _session = session; _audit = audit; _log = log; }

    public async Task<List<<Entity>Dto>> GetAllAsync() =>
        await _db.<Entity>s.AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new <Entity>Dto(x.<Entity>Id, x.Code, x.NameAr, x.NameEn, x.Status))
            .ToListAsync();

    public async Task<<Entity>Dto?> GetByIdAsync(int id) =>
        await _db.<Entity>s.AsNoTracking()
            .Where(x => x.<Entity>Id == id)
            .Select(x => new <Entity>Dto(x.<Entity>Id, x.Code, x.NameAr, x.NameEn, x.Status))
            .FirstOrDefaultAsync();

    public async Task<int> CreateAsync(<Entity>Dto dto, int staffId)
    {
        var entity = new <Entity>
        {
            Code = dto.Code,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Status = dto.Status
        };
        _db.<Entity>s.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogActionAsync(nameof(<Entity>), entity.<Entity>Id, "INSERT", staffId);
        return entity.<Entity>Id;
    }

    public async Task UpdateAsync(int id, <Entity>Dto dto, int staffId)
    {
        var entity = await _db.<Entity>s.FindAsync(id)
            ?? throw new InvalidOperationException($"<Entity> {id} not found");
        entity.Code = dto.Code;
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.Status = dto.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.LogActionAsync(nameof(<Entity>), id, "UPDATE", staffId);
    }

    public async Task DeleteAsync(int id, int staffId)
    {
        var entity = await _db.<Entity>s.FindAsync(id)
            ?? throw new InvalidOperationException($"<Entity> {id} not found");
        _db.<Entity>s.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogActionAsync(nameof(<Entity>), id, "DELETE", staffId);
    }
}
```

## ViewModel template

```csharp
using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class <Entity>WindowViewModel : ViewModelBase
{
    private readonly I<Entity>Service _service;
    private readonly ICurrentUserSession _session;
    private readonly ILogger<<Entity>WindowViewModel> _log;

    public ObservableCollection<<Entity>Dto> Items { get; } = new();
    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveAsyncCommand { get; }
    public AsyncRelayCommand DeleteAsyncCommand { get; }

    public <Entity>WindowViewModel(I<Entity>Service service, ICurrentUserSession session,
        ILogger<<Entity>WindowViewModel> log)
    {
        _service = service;
        _session = session;
        _log = log;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveAsyncCommand = new AsyncRelayCommand(SaveAsync, () => !HasErrors && SelectedItem is not null);
        DeleteAsyncCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem is not null);
    }

    private <Entity>Dto? _selectedItem;
    public <Entity>Dto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    private async Task LoadAsync()
    {
        BeginBusy("جاري التحميل...");
        try
        {
            Items.Clear();
            foreach (var x in await _service.GetAllAsync())
                Items.Add(x);
        }
        finally { EndBusy(); }
    }

    private async Task SaveAsync()
    {
        if (SelectedItem is null || _session.CurrentUser is null) return;
        await _service.UpdateAsync(SelectedItem.<Entity>Id, SelectedItem, _session.CurrentUser.StaffId);
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem is null || _session.CurrentUser is null) return;
        await _service.DeleteAsync(SelectedItem.<Entity>Id, _session.CurrentUser.StaffId);
        await LoadAsync();
    }
}
```

## View template (minimal)

```xml
<Window x:Class="FinalLabSystem.Views.Settings.<Entity>Window"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:FinalLabSystem.ViewModels.Settings"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=vm:<Entity>WindowViewModel}"
        FlowDirection="RightToLeft" Language="ar-SA"
        Title="إدارة <Entity>" Height="600" Width="900">
    <DockPanel>
        <ToolBar DockPanel.Dock="Top">
            <Button Content="تحميل" Command="{Binding LoadCommand}" />
            <Button Content="حفظ" Command="{Binding SaveAsyncCommand}" />
            <Button Content="حذف" Command="{Binding DeleteAsyncCommand}" />
        </ToolBar>
        <DataGrid ItemsSource="{Binding Items}"
                  SelectedItem="{Binding SelectedItem}"
                  AutoGenerateColumns="False" IsReadOnly="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="الكود" Binding="{Binding Code}" />
                <DataGridTextColumn Header="الاسم بالعربية" Binding="{Binding NameAr}" />
                <DataGridTextColumn Header="الاسم بالإنجليزية" Binding="{Binding NameEn}" />
                <DataGridTextColumn Header="الحالة" Binding="{Binding Status}" />
            </DataGrid.Columns>
        </DataGrid>
    </DockPanel>
</Window>
```