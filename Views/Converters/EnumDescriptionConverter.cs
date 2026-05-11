using System;
using System.Globalization;
using System.Windows.Data;
using Core.Adaptation.CalculationErrorStrategies;
using ViewModels.AdaptationViewModels;

namespace Views.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CalculationErrorStrategyType type)
            return CalculationErrorStrategyTypeHelper.GetDescription(type);
        
        return value?.ToString() ?? string.Empty; 
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}