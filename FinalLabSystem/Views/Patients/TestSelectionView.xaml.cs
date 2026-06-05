using System.Windows.Controls;
using System.Windows.Input;

namespace FinalLabSystem.Views.Patients;

public partial class TestSelectionView : UserControl
{
    public TestSelectionView()
    {
        InitializeComponent();
    }

    private void AvailableTests_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.Patients.TestSelectionViewModel viewModel)
            viewModel.AddTestCommand.Execute(viewModel.SelectedAvailableTest);
    }
}
