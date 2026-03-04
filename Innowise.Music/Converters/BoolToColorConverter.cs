using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Innowise.Music.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                if (parameter is string colorString && !string.IsNullOrEmpty(colorString))
                {
                    if (Application.Current?.Resources.TryGetValue(colorString, out var colorResource) == true && colorResource is Color color)
                    {
                        return color;
                    }
                }
            }

            if (Application.Current?.Resources.TryGetValue("InputBackgroundColor", out var defaultColorResource) == true && defaultColorResource is Color defaultColor)
            {
                return defaultColor;
            }

            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
