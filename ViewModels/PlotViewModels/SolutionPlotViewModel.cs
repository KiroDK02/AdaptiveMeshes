using CommunityToolkit.Mvvm.ComponentModel;
using Core.Solution.Interfaces;
using ScottPlot.WPF;

namespace ViewModels.PlotViewModels;

public partial class SolutionPlotViewModel : ObservableObject
{
    private WpfPlot? _wpfPlot;
    private ISolution?  _solution;

    [ObservableProperty] private bool showColorMap = true;
    [ObservableProperty] private bool showIsolines = true;

    public void SetPlot(WpfPlot wpfPlot) => _wpfPlot = wpfPlot;
    
    public void SetSolution(ISolution solution) => _solution = solution;

    public void DrawSolution()
    {
        if (_wpfPlot is null || _solution is null)
            return;
        
        _wpfPlot.Plot.Clear();
        
        _wpfPlot.DrawElements(_solution.Mesh, null);
        
        // TODO: добавить отрисовку цветового градиента и изолиний
        
        _wpfPlot.Plot.Axes.AutoScale();
        _wpfPlot.Refresh();
    }
}