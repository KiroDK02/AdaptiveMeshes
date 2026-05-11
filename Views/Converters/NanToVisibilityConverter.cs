using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Views.Converters;

public class NanToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double and not double.NaN
        ? Visibility.Visible
        : Visibility.Collapsed;
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}