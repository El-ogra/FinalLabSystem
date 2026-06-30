using System;
using System.Windows;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Implementations;

public sealed class ReceiptDialogFactory : IReceiptDialogFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ReceiptDialogFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool Show(VisitFullDto dto, Window? owner = null)
    {
        var viewModel = (ReceiptDialogViewModel)_serviceProvider.GetService(typeof(ReceiptDialogViewModel))!;
        viewModel.InitializeAsync(dto).GetAwaiter().GetResult();

        if (!viewModel.CanPrint)
            return false;

        var dialog = new Views.Patients.ReceiptDialog(viewModel)
        {
            Owner = owner ?? Application.Current.MainWindow
        };
        dialog.ShowDialog();
        return true;
    }
}
