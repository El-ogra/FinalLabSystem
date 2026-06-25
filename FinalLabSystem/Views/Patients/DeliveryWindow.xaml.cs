using System.Windows;
using FinalLabSystem.ViewModels.Patients.Delivery;

namespace FinalLabSystem.Views.Patients;

public partial class DeliveryWindow : Window
{
    public DeliveryWindow(DeliveryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
