using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class TestResultsWindow : Window
{
    public TestResultsWindow(TestResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        KeyDown += Window_KeyDown;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TestResultsViewModel vm)
            await vm.LoadAsync();
    }

    private async void ResultTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        if (DataContext is TestResultsViewModel vm && vm.SelectedTest != null)
        {
            vm.SaveInlineResultCommand.Execute(null);
        }

        await System.Threading.Tasks.Task.Delay(100);

        int currentIndex = TestsDataGrid.SelectedIndex;
        if (currentIndex < TestsDataGrid.Items.Count - 1)
        {
            TestsDataGrid.SelectedIndex = currentIndex + 1;
            TestsDataGrid.ScrollIntoView(TestsDataGrid.SelectedItem);

            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                var container = TestsDataGrid.ItemContainerGenerator
                    .ContainerFromIndex(currentIndex + 1) as DataGridRow;
                if (container != null)
                {
                    var textBox = FindVisualChild<TextBox>(container);
                    textBox?.Focus();
                }
            }), DispatcherPriority.Background);
        }

        e.Handled = true;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found)
                return found;

            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            if (TestsDataGrid.IsFocused)
            {
                var patientList = FindVisualChild<System.Windows.Controls.ListView>(
                    (DependencyObject)VisualTreeHelper.GetChild(this, 0));
                patientList?.Focus();
            }
            else
            {
                TestsDataGrid.Focus();
            }
            e.Handled = true;
        }
    }
}
