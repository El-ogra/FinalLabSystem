using System;
using System.Collections.Generic;
using System.Windows;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Implementations;

public sealed class AuditTrailDialogService : IAuditTrailDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public AuditTrailDialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowGeneralAudit(string title, List<AuditLog> entries)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = new AuditTrailViewModel(title, entries);
            var window = new Views.Patients.AuditTrailWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        });
    }

    public void ShowResultAudit(string title, List<VResultAuditTrail> entries)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = new AuditTrailViewModel(title, entries);
            var window = new Views.Patients.AuditTrailWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        });
    }
}
