using System;
using System.Globalization;
using System.Windows.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Views.Converters;

public sealed class SensitivityToBoolConverter : IValueConverter
{
    public static readonly SensitivityToBoolConverter Highly = new(AntibioticSensitivity.Highly);
    public static readonly SensitivityToBoolConverter Moderate = new(AntibioticSensitivity.Moderate);
    public static readonly SensitivityToBoolConverter Low = new(AntibioticSensitivity.Low);
    public static readonly SensitivityToBoolConverter Resistant = new(AntibioticSensitivity.Resistant);

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
