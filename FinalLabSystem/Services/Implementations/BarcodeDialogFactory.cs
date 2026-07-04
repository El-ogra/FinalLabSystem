using System;
using System.Windows;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Implementations;

public sealed class BarcodeDialogFactory : IBarcodeDialogFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BarcodeDialogFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public BarcodeDialogResult Show(int visitId, int patientId, Window? owner = null)
    {
        var viewModel = (BarcodeDialogViewModel)_serviceProvider.GetService(typeof(BarcodeDialogViewModel))!;
        viewModel.LoadBarcodesAsync(visitId, patientId).GetAwaiter().GetResult();
        var dialog = new Views.Patients.BarcodeDialog(viewModel)
        {
            Owner = owner ?? Application.Current.MainWindow
        };
        dialog.ShowDialog();
        return BarcodeDialogResult.Printed;
    }
}
