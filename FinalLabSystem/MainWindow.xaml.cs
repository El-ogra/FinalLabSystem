using System.Windows;
using FinalLabSystem.ViewModels;

namespace FinalLabSystem;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
