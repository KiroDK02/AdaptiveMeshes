using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Solution.Interfaces;

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