using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class TestResultsWindow : Window
{
    private TestResultsViewModel? _viewModel;

    public TestResultsWindow(TestResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TestResultsViewModel vm)
            await vm.LoadAsync();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TestResultsViewModel.IsInlineEditing) && _viewModel?.IsInlineEditing == true)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TestsDataGrid.Focus();
                if (TestsDataGrid.SelectedItem != null)
                {
                    TestsDataGrid.BeginEdit();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private void CopyCode_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is TestResultsViewModel vm && vm.CurrentPatientInfo != null)
            System.Windows.Clipboard.SetText(vm.CurrentPatientInfo.PatientCode);
    }

    private void ResultEditTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.Focus();
            textBox.SelectAll();
        }
    }
}
