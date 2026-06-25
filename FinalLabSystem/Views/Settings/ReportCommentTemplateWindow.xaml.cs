using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class ReportCommentTemplateWindow : Window
{
    public ReportCommentTemplateWindow(ReportCommentTemplateViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }
}
