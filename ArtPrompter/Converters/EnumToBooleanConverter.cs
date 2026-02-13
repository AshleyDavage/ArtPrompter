using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ArtPrompter.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
            {
                return false;
            }

            var parameterString = parameter.ToString();
            return parameterString != null && value.ToString() == parameterString;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is null)
            {
                return null;
            }

            if (value is bool isChecked && isChecked)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }

            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
