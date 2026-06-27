using System.Windows;

namespace FinalLabSystem.Views.Settings;

public partial class StockAdjustmentDialog : Window
{
    public int Adjustment { get; private set; }
    public string? Reason { get; private set; }

    public StockAdjustmentDialog(string materialName, int currentStock)
    {
        InitializeComponent();
        MaterialNameText.Text = materialName;
        CurrentStockText.Text = currentStock.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(AdjustmentBox.Text.Trim(), out var adjustment))
        {
            MessageBox.Show("يرجى إدخال رقم صحيح للتعديل.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Adjustment = adjustment;
        Reason = string.IsNullOrWhiteSpace(ReasonBox.Text) ? null : ReasonBox.Text.Trim();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
