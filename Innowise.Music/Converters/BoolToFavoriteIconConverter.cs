using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Innowise.Music.Converters
{
    public class BoolToFavoriteIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFavorited)
            {
                return isFavorited ? "favorite_icon.svg" : "add_icon.svg";
            }
            return "add_icon.svg";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
