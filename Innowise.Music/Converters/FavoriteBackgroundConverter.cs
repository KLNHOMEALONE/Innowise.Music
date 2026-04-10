using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Innowise.Music.Converters
{
    public class FavoriteBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite
                    ? Application.Current?.Resources.TryGetValue("PrimaryRed", out var color) == true ? (Color)color : Colors.Transparent
                    : Colors.Transparent;
            }
            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
