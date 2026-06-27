using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class AttendanceWindow : Window
{
    public AttendanceWindow(AttendanceWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
