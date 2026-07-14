using System;
using System.Globalization;
using System.Windows.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Views.Converters;

/// <summary>
/// محوّل واحد لكل قيمة من قيم <see cref="AntibioticSensitivity"/>.
/// أسماء الحقول الثابتة أُبقيت مطابقة لأسماء قيم الـ enum الجديدة بلاحقة For
/// (القرار 22 / VS-04) — الاستخدام في XAML عبر {x:Static local:SensitivityToBoolConverter.HighlyFor} ...
/// </summary>
public sealed class SensitivityToBoolConverter : IValueConverter
{
    public static readonly SensitivityToBoolConverter HighlyFor = new(AntibioticSensitivity.HighlyFor);
    public static readonly SensitivityToBoolConverter ModerateFor = new(AntibioticSensitivity.ModerateFor);
    public static readonly SensitivityToBoolConverter LowFor = new(AntibioticSensitivity.LowFor);
    public static readonly SensitivityToBoolConverter ResistantFor = new(AntibioticSensitivity.ResistantFor);

    private readonly AntibioticSensitivity _targetValue;

    private SensitivityToBoolConverter(AntibioticSensitivity targetValue)
    {
        _targetValue = targetValue;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AntibioticSensitivity sensitivity)
            return sensitivity == _targetValue;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
            return _targetValue;
        return Binding.DoNothing;
    }
}
