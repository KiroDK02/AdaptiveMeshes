using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Solution.Interfaces;
using DataTransferObjects;

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

    public PointDto ToPointDto() => new()
    {
        X = this.X,
        Y = this.Y,
        Value = this.Value
    };

    public static PointViewModel FromDto(PointDto pointDto)
    {
        return new()
        {
            X = pointDto.X,
            Y = pointDto.Y,
            Value = pointDto.Value
        };
    }
}