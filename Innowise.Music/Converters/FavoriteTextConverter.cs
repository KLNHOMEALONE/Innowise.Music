using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Innowise.Music.Converters
{
    public class FavoriteTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                // Use text variant selector (U+FE0E) to prevent emoji rendering on Android
                return isFavorite ? "\u2665\uFE0E" : "\u2661\uFE0E";
            }
            return "\u2661\uFE0E";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
