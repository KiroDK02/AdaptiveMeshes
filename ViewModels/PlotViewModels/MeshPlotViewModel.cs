using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using ScottPlot.WPF;
using ViewModels.MaterialViewModels;

namespace ViewModels.PlotViewModels;

public partial class MeshPlotViewModel : ObservableObject
{
    private WpfPlot? _wpfPlot;

    [ObservableProperty] private bool drawMaterials = true;
    
    public void SetWpfPlot(WpfPlot wpfPlot) => _wpfPlot = wpfPlot;

    public void DrawMesh(IFiniteElementMesh mesh, IEnumerable<MaterialViewModel> materials)
    {
        if (_wpfPlot is null)
            return;
        
        _wpfPlot.Plot.Clear();
        
        _wpfPlot.DrawElements(mesh, materials, DrawMaterials);
        
        _wpfPlot.Plot.Axes.AutoScale();
        _wpfPlot.Refresh();
    }
}