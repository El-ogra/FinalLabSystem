---
name: wpf
description: "مهارة متخصصة في wpf لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# WPF

> 🛑 **تحذير حرج — خاص بمشروع FinalLabSystem:**
>
> جميع أمثلة MVVM في هذه المهارة **تم تخصيصها** لمطابقة معايير المشروع:
>
> - **✅ ViewModelBase المخصص** (من `Infrastructure.ViewModelBase`) — **لا** `ObservableObject` من Toolkit
> - **✅ `SetProperty` يدوياً** — **لا** `[ObservableProperty]`
> - **✅ `RelayCommand`/`AsyncRelayCommand` من `Infrastructure`** — **لا** `[RelayCommand]` attribute
> - **✅ FlowDirection="RightToLeft"** على كل نافذة (التطبيق عربي أولاً)
> - **✅ DataContext** يُعيّن من `NavigationService.OpenTaskWindow<VM>()` — **لا** في XAML
>
> **المصدر المعتمد:** `wpf-mvvm-conventions` (إلزامي).
> **لأمثلة Toolkit الأصلية (للفهم فقط):** راجع `mvvm-toolkit` قسم "المرجع النظري".

## Trigger On

- working on WPF UI, MVVM, binding, commands, or desktop modernization in FinalLabSystem
- migrating WPF from .NET Framework to .NET
- integrating newer Windows capabilities into a WPF app
- implementing data binding, styles, templates, or control customization

## Documentation

- [WPF Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/)
- [Data Binding Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)
- [MVVM Toolkit Introduction](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — ℹ️ مرجع نظري فقط؛ FinalLabSystem لا يستخدم Toolkit patterns مباشرة — راجع `wpf-mvvm-conventions`.
- [Styles and Templates](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/styles-templates-overview)
- [Migration Guide](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/migration/)

### References

- [patterns.md](references/patterns.md) - MVVM patterns, binding patterns, command patterns, and reusable architectural approaches
- [anti-patterns.md](references/anti-patterns.md) - Common WPF mistakes and how to avoid them

## Workflow

1. **Confirm Windows-only scope** — WPF is Windows-only even when the wider .NET stack is cross-platform
2. **Apply MVVM pattern** — keep views dumb, logic in ViewModels, use commands
3. **Manage data binding explicitly** — choose correct binding modes, validate at runtime
4. **Use styles and templates deliberately** — keep UI composable, avoid page-specific hacks
5. **Handle threading correctly** — use Dispatcher for UI updates, async/await for long operations
6. **Validate both designer and runtime** — XAML composition failures often surface only at runtime

## Current Upstream Notes

- The refreshed WPF overview page reiterates WPF as a Windows desktop UI stack with XAML, data binding, styling, templates, resources, and vector/rich-media composition. Keep WPF-specific guidance separate from WinUI or MAUI unless the task is explicitly a migration or comparison.
- For modernization work, check both `.NET Framework` compatibility constraints and current .NET desktop migration docs before moving project files or XAML resource dictionaries.

## Project Structure

```
MyWpfApp/
├── MyWpfApp/
│   ├── App.xaml                # Application entry
│   ├── MainWindow.xaml         # Main window
│   ├── Views/                  # XAML views/windows
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Models/                 # Domain models
│   ├── Services/               # Business logic
│   ├── Converters/             # Value converters
│   ├── Resources/              # Styles, templates, dictionaries
│   └── Controls/               # Custom controls
└── MyWpfApp.Tests/
```

## MVVM Pattern (FinalLabSystem-specific)

### ViewModel — النمط المعتمد لـ FinalLabSystem

```csharp
using FinalLabSystem.Infrastructure;
using System.Collections.ObjectModel;

namespace FinalLabSystem.ViewModels.Customers;

public sealed class CustomersViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersViewModel> _logger;

    private ObservableCollection<Customer> _customers = new();
    public ObservableCollection<Customer> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                SaveAsyncCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshAsyncCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand RefreshAsyncCommand { get; }
    public AsyncRelayCommand SaveAsyncCommand    { get; }

    public CustomersViewModel(ICustomerService customerService,
                               ILogger<CustomersViewModel> logger)
    {
        _customerService     = customerService;
        _logger              = logger;

        RefreshAsyncCommand = new AsyncRelayCommand(RefreshAsync, CanRefresh);
        SaveAsyncCommand    = new AsyncRelayCommand(SaveAsync,    CanSave);
    }

    private bool CanRefresh() => !IsLoading;
    private bool CanSave()    => SelectedCustomer is not null && !HasErrors;

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var items = await _customerService.GetAllAsync();
            Customers = new ObservableCollection<Customer>(items);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (SelectedCustomer is null) return;
        await _customerService.SaveAsync(SelectedCustomer);
    }
}
```

> **لاحظ:**
> - `sealed class` وليس `partial`.
> - `SetProperty` تعيد `bool` — تُستخدم لاستدعاء `RaiseCanExecuteChanged` يدوياً.
> - الأوامر تُبنى في المُنشئ (ليس مولّدة تلقائياً).

### View Binding
```xml
<Window x:Class="MyWpfApp.Views.CustomersView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:MyWpfApp.ViewModels"
        d:DataContext="{d:DesignInstance Type=vm:CustomersViewModel}"
        FlowDirection="RightToLeft"
        Language="ar-SA">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <ToolBar Grid.Row="0">
            <Button Content="Refresh"
                    Command="{Binding RefreshAsyncCommand}"/>
            <Button Content="Save"
                    Command="{Binding SaveAsyncCommand}"/>
        </ToolBar>

        <DataGrid Grid.Row="1"
                  ItemsSource="{Binding Customers}"
                  SelectedItem="{Binding SelectedCustomer}"
                  AutoGenerateColumns="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Name"
                                    Binding="{Binding Name}"/>
                <DataGridTextColumn Header="Email"
                                    Binding="{Binding Email}"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

## Dependency Injection

```csharp
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<ICustomerService, CustomerService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // ViewModels
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<CustomerDetailViewModel>();

                // Views
                services.AddTransient<MainWindow>();
                services.AddTransient<CustomersView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
```

## Data Binding Modes

```xml
<!-- OneTime: Read once at initialization -->
<TextBlock Text="{Binding CreatedDate, Mode=OneTime}"/>

<!-- OneWay: Source to target only (default for most properties) -->
<TextBlock Text="{Binding Name, Mode=OneWay}"/>

<!-- TwoWay: Bidirectional synchronization -->
<TextBox Text="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- OneWayToSource: Target to source only -->
<TextBox Text="{Binding SearchFilter, Mode=OneWayToSource}"/>
```

## Value Converters

```csharp
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}

// Multi-value converter
public class MultiplyConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double a && values[1] is double b)
        {
            return a * b;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

## Styles and Templates

### Resource Dictionary
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Implicit style for all Buttons -->
    <Style TargetType="Button">
        <Setter Property="Padding" Value="10,5"/>
        <Setter Property="Margin" Value="5"/>
        <Setter Property="Background" Value="#0078D4"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Named style -->
    <Style x:Key="DangerButton" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
        <Setter Property="Background" Value="#D32F2F"/>
    </Style>
</ResourceDictionary>
```

## Threading and Dispatcher

```csharp
// Update UI from background thread
await Task.Run(async () =>
{
    var data = await LoadDataAsync();

    // Must use Dispatcher to update UI
    Application.Current.Dispatcher.Invoke(() =>
    {
        Items.Clear();
        foreach (var item in data)
        {
            Items.Add(item);
        }
    });
});

// Better: Use async/await properly
private async Task LoadDataAsync()
{
    IsLoading = true;
    try
    {
        // This runs on background thread
        var data = await _service.GetDataAsync();

        // This automatically marshals to UI thread
        Items = new ObservableCollection<Item>(data);
    }
    finally
    {
        IsLoading = false;
    }
}
```

## Anti-Patterns to Avoid

| Anti-Pattern | Why It's Bad | Better Approach |
|--------------|--------------|-----------------|
| Logic in code-behind | Hard to test, tight coupling | Use MVVM with ViewModels |
| Synchronous blocking calls | UI freezes | Use async/await |
| Manual INotifyPropertyChanged | Boilerplate, error-prone | في FinalLabSystem: استخدم `SetProperty` من `Infrastructure.ViewModelBase` (لا Toolkit attributes) |
| `[ObservableProperty]` / `[RelayCommand]` in FinalLabSystem | يخالف معايير المشروع | ورث `ViewModelBase`، واستخدم `SetProperty` و `Infrastructure.RelayCommand` مباشرة |
| Hardcoded colors/sizes | Inconsistent, hard to theme | Use resource dictionaries |
| Direct Dispatcher.Invoke everywhere | Complex, error-prone | Prefer async/await marshaling |
| God ViewModel | Unmaintainable | Split into focused ViewModels |
| Skipping binding validation | Runtime errors hidden | Use ValidatesOnDataErrors |
| Event handlers for everything | Memory leaks, coupling | Use commands and bindings |

## Best Practices

1. **Use compiled bindings in .NET 5+:**
   - Enable `x:CompileBindings="True"` for performance

2. **Implement INotifyDataErrorInfo for validation (FinalLabSystem pattern):**

   `Infrastructure.ViewModelBase` already implements `INotifyDataErrorInfo` — use its `AddError` / `ClearErrors` instead of `[NotifyDataErrorInfo]`.

   ```csharp
   private string _name = string.Empty;
   public string Name
   {
       get => _name;
       set
       {
           if (SetProperty(ref _name, value))
           {
               ValidateName();
           }
       }
   }

   private void ValidateName()
   {
       ClearErrors(nameof(Name));
       if (string.IsNullOrWhiteSpace(_name))
           AddError(nameof(Name), "الاسم مطلوب");
       else if (_name.Length < 2)
           AddError(nameof(Name), "الاسم يجب أن يكون حرفين على الأقل");
   }
   ```

   > ❌ لا تستخدم `[ObservableProperty]` أو `[NotifyDataErrorInfo]` في ملفات FinalLabSystem — الميزة جاهزة يدوياً في `ViewModelBase`.

3. **Use weak event patterns for long-lived subscriptions:**
   ```csharp
   WeakEventManager<Source, EventArgs>.AddHandler(source, "EventName", Handler);
   ```

4. **Virtualize large collections:**
   ```xml
   <ListBox VirtualizingPanel.IsVirtualizing="True"
            VirtualizingPanel.VirtualizationMode="Recycling"
            ItemsSource="{Binding LargeCollection}"/>
   ```

5. **Freeze Freezables when possible:**
   ```csharp
   var brush = new SolidColorBrush(Colors.Blue);
   brush.Freeze(); // Thread-safe, better performance
   ```

6. **Use design-time data:**
   ```xml
   <Window d:DataContext="{d:DesignInstance Type=vm:MainViewModel, IsDesignTimeCreatable=True}">
   ```

## Testing

```csharp
[Fact]
public async Task RefreshCommand_LoadsCustomers()
{
    var mockService = new Mock<ICustomerService>();
    mockService.Setup(s => s.GetAllAsync())
        .ReturnsAsync(new[] { new Customer { Name = "Test" } });

    var viewModel = new CustomersViewModel(mockService.Object);

    await viewModel.RefreshCommand.ExecuteAsync(null);

    Assert.Single(viewModel.Customers);
    Assert.Equal("Test", viewModel.Customers[0].Name);
}

[Fact]
public void SaveCommand_CannotExecute_WhenNoSelection()
{
    var mockService = new Mock<ICustomerService>();
    var viewModel = new CustomersViewModel(mockService.Object);

    viewModel.SelectedCustomer = null;

    Assert.False(viewModel.SaveCommand.CanExecute(null));
}
```

## Deliver

- cleaner WPF views and view-model boundaries
- safer binding and threading behavior
- migration guidance grounded in actual Windows constraints
- MVVM pattern with testable ViewModels

## Validate

- binding and command flows are explicit
- code-behind is not carrying hidden business logic
- Windows-only assumptions are acknowledged
- threading and dispatcher usage is correct
- styles and resources are properly organized
