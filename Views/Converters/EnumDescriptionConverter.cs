using System;
using System.Globalization;
using System.Windows.Data;
using Core.Adaptation.CalculationErrorStrategies;
using ViewModels.AdaptationViewModels;
using ViewModels.ProblemViewModels;

namespace Views.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            CalculationErrorStrategyType cesType => CalculationErrorStrategyTypeHelper.GetDescription(cesType),
            ErrorType errorType => ErrorTypeHelper.GetDescription(errorType),
        
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}