using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Views.Converters;

public class ScottPlotColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ScottPlot.Color color)
            throw new ArgumentException($"Cannot convert {value} to {nameof(Color)}.");

        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
            throw new ArgumentException($"Cannot convert {value} to {nameof(ScottPlot.Color)}.");
        
        return new ScottPlot.Color(color.R, color.G, color.B, color.A);
    }
}