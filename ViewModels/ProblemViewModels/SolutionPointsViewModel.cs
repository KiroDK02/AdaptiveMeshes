using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Solution.Interfaces;
using DataTransferObjects;

namespace ViewModels.ProblemViewModels;

public partial class SolutionPointsViewModel : ObservableObject
{
    [ObservableProperty] ObservableCollection<PointViewModel> points = [];

    private ISolution? _solution;

    public void SetSolution(ISolution solution)
    {
        if (_solution != null)
            foreach (var point in Points)
                point.Value = null;

        _solution = solution;
    }

    public void LoadFromDto(IEnumerable<PointDto> pointDtos)
    {
        Points.Clear();
        foreach (var point in pointDtos)
            Points.Add(PointViewModel.FromDto(point));
    }

    [RelayCommand]
    private void Calculate()
    {
        if (_solution == null)
            return;

        foreach (var point in Points)
            point.CalculateValue(_solution);
    }

    [RelayCommand]
    private void AddPoint() => Points.Add(new PointViewModel());

    [RelayCommand]
    private void RemovePoint(PointViewModel point) => Points.Remove(point);

    [RelayCommand]
    private void ClearPoints() => Points = [];
}