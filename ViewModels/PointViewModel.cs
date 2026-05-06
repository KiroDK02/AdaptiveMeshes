using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Solution.Interfaces;

namespace ViewModels;

public partial class PointViewModel : ObservableObject
{
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double? value;

    public void CalculateValue(ISolution solution)
    {
        try
        {
            Value = solution.Value(new(X, Y));
        }
        catch (ArgumentException)
        {
            Value = double.NaN;
        }
    }
}