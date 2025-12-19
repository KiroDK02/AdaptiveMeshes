using CommunityToolkit.Mvvm.ComponentModel;
using Core.Solution.Interfaces;
using ScottPlot.WPF;

namespace ViewModels.PlotViewModels;

public partial class SolutionPlotViewModel : ObservableObject
{
    private WpfPlot? _wpfPlot;

    [ObservableProperty] private bool showColorMap = true;
    [ObservableProperty] private bool showIsolines = true;

    public void SetPlot(WpfPlot wpfPlot) => _wpfPlot = wpfPlot;

    public void DrawSolution(ISolution solution)
    {
        if (_wpfPlot is null)
            return;
        
        _wpfPlot.Plot.Clear();
        
        _wpfPlot.DrawElements(solution.Mesh);
        
        // TODO: добавить отрисовку цветового градиента и изолиний
        
        _wpfPlot.Plot.Axes.AutoScale();
        _wpfPlot.Refresh();
    }
}