using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class BarcodeDialog : Window
{
    private Point _dragStartPoint;
    private bool _isDragging;

    public BarcodeDialog(BarcodeDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is BarcodeLabel label)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;

            var vm = (BarcodeDialogViewModel)DataContext;
            vm.DraggedLabel = label;
        }
    }

    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
        {
            var currentPos = e.GetPosition(null);
            var diff = _dragStartPoint - currentPos;

            if (SystemParameters.MinimumHorizontalDragDistance < diff.X ||
                SystemParameters.MinimumVerticalDragDistance < diff.Y)
            {
                _isDragging = true;
                if (sender is Border border && border.DataContext is BarcodeLabel label)
                {
                    var data = new DataObject("BarcodeLabel", label);
                    DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                }
            }
        }
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        var vm = (BarcodeDialogViewModel)DataContext;
        vm.DraggedLabel = null;

        if (e.Data.GetDataPresent("BarcodeLabel") &&
            sender is Border border &&
            border.DataContext is BarcodeLabel destLabel)
        {
            var sourceLabel = (BarcodeLabel)e.Data.GetData("BarcodeLabel");
            if (sourceLabel != null && sourceLabel != destLabel)
            {
                _ = vm.MoveTestToLabelAsync(sourceLabel, destLabel);
            }
        }

        _isDragging = false;
    }

    private void Border_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            var vm = (BarcodeDialogViewModel)DataContext;
            vm.DraggedLabel = null;
        }
    }
}
