using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinalLabSystem.Views.Shared;

public partial class SpecialCharsBar : UserControl
{
    public SpecialCharsBar()
    {
        InitializeComponent();
    }

    private void CharButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string charStr &&
            Keyboard.FocusedElement is TextBox textBox && !string.IsNullOrEmpty(charStr))
        {
            int caretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text.Insert(caretIndex, charStr);
            textBox.CaretIndex = caretIndex + charStr.Length;
            textBox.Focus();
        }
    }
}
