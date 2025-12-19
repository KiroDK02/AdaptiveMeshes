using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using ScottPlot.WPF;

namespace ViewModels.PlotViewModels;

public partial class MeshPlotViewModel : ObservableObject
{
    private WpfPlot? _wpfPlot;
    
    public void SetWpfPlot(WpfPlot wpfPlot) => _wpfPlot = wpfPlot;

    public void DrawMesh(IFiniteElementMesh mesh)
    {
        if (_wpfPlot is null)
            return;
        
        _wpfPlot.Plot.Clear();
        
        _wpfPlot.DrawElements(mesh);
        
        _wpfPlot.Plot.Axes.AutoScale();
        _wpfPlot.Refresh();
    }
}