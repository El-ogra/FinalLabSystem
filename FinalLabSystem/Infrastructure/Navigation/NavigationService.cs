using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Infrastructure.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _viewModelToWindowMap = new();

    private Window? _bootstrapWindow;
    private Window? _mainWindow;
    private Window? _activeTaskWindow;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RegisterWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window
    {
        _viewModelToWindowMap[typeof(TViewModel)] = typeof(TWindow);
    }

    public void ShowLogin()
    {
        SwapBootstrapWindow(_serviceProvider.GetRequiredService<Views.LoginWindow>());
    }

    public void ShowFirstRunSetup()
    {
        SwapBootstrapWindow(_serviceProvider.GetRequiredService<Views.FirstRunSetupWindow>());
    }

    public void ShowMain()
    {
        _mainWindow ??= _serviceProvider.GetRequiredService<MainWindow>();

        Application.Current.MainWindow = _mainWindow;
        _mainWindow.Show();
        _mainWindow.Activate();

        if (_bootstrapWindow is not null)
        {
            _bootstrapWindow.Close();
            _bootstrapWindow = null;
        }
    }

    public void OpenTaskWindow<TViewModel>() where TViewModel : class
    {
        OpenTaskWindow<TViewModel>(null);
    }

    public void OpenTaskWindow<TViewModel>(Action<TViewModel>? configure) where TViewModel : class
    {
        if (!_viewModelToWindowMap.TryGetValue(typeof(TViewModel), out var windowType))
            throw new InvalidOperationException(
                $"No window is registered for ViewModel '{typeof(TViewModel).Name}'.");

        if (_serviceProvider.GetService(windowType) is not Window window)
            throw new InvalidOperationException(
                $"Window '{windowType.Name}' is not registered in the DI container.");

        if (window.DataContext is TViewModel vm)
            configure?.Invoke(vm);

        _mainWindow?.Hide();
        _activeTaskWindow = window;
        window.Closed += OnTaskWindowClosed;
        window.Show();
        window.Activate();
    }

    public void ReturnToMain()
    {
        if (_activeTaskWindow is not null)
        {
            _activeTaskWindow.Closed -= OnTaskWindowClosed;
            _activeTaskWindow.Close();
            _activeTaskWindow = null;
        }

        if (_mainWindow is not null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    public void Shutdown() => Application.Current.Shutdown();

    private void SwapBootstrapWindow(Window window)
    {
        var previous = _bootstrapWindow;
        _bootstrapWindow = window;
        Application.Current.MainWindow = window;
        window.Show();
        previous?.Close();
    }

    private void OnTaskWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window closed)
            closed.Closed -= OnTaskWindowClosed;

        _activeTaskWindow = null;
        _mainWindow?.Show();
        _mainWindow?.Activate();
    }
}
