using System;
using System.Windows;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views.Settings;

namespace FinalLabSystem.Services.Implementations;

public sealed class NormalRangesWindowFactory : INormalRangesWindowFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NormalRangesWindowFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Open(TestType editableTest, Window? owner = null)
    {
        var window = (NormalRangesWindow)_serviceProvider.GetService(typeof(NormalRangesWindow))!;
        if (window.DataContext is NormalRangeWindowViewModel vm)
            vm.InitializeAsync(editableTest).GetAwaiter().GetResult();
        window.Owner = owner ?? Application.Current.MainWindow;
        window.ShowDialog();
    }
}
