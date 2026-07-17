using System;
using System.Globalization;
using System.Windows.Data;

namespace FinalLabSystem.Views.Converters;

public sealed class EnumBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is string parameterString)
        {
            return enumValue.ToString() == parameterString;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string parameterString)
        {
            if (targetType.IsEnum)
                return Enum.Parse(targetType, parameterString, true);
            return parameterString;
        }
        return Binding.DoNothing;
    }
}
