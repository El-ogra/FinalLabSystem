using System;
using System.Windows;
using System.Windows.Documents;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.Views.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Services.Implementations;

public sealed class PrintPreviewDialogService : IPrintPreviewDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public PrintPreviewDialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Show(FlowDocument document, string description, Window? owner = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<PrintPreviewViewModel>();
        var window = scope.ServiceProvider.GetRequiredService<PrintPreviewWindow>();

        vm.Document = document;
        vm.Description = description;
        window.DataContext = vm;
        window.Owner = owner ?? Application.Current.MainWindow;

        vm.RequestClose = () => window.Close();

        window.ShowDialog();
    }
}
