using System.Windows;
using FinalLabSystem.Models;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class AddExtraChargeDialog : Window
{
    private readonly AddExtraChargeViewModel _viewModel;

    public AddExtraChargeDialog()
    {
        InitializeComponent();
        _viewModel = new AddExtraChargeViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AddExtraChargeViewModel.DialogResult))
                DialogResult = _viewModel.DialogResult;
        };
    }

    public VisitCharge? ChargeResult => _viewModel.Result;
}
