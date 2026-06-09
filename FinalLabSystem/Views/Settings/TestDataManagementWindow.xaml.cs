using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class TestDataManagementWindow : Window
{
    public TestDataManagementWindow(TestDataManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void AdvancedTubeButton_Click(object sender, RoutedEventArgs e)
    {
        bool isVisible = AdvancedTubeGrid.Visibility == Visibility.Visible;
        AdvancedTubeGrid.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
        AdvancedTubeButtons.Visibility = AdvancedTubeGrid.Visibility;
        AdvancedTubeButton.Content = isVisible ? "متقدم" : "بسيط";
    }
}
