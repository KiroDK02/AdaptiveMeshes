using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using ScottPlot;
using ScottPlot.WPF;
using ViewModels.MaterialViewModels;

namespace ViewModels.PlotViewModels;

public partial class MeshPlotViewModel : ObservableObject
{
    private IPlotControl? _plotControl;
    private IFiniteElementMesh? _lastMesh;
    private IEnumerable<MaterialViewModel>? _lastMaterials;
    
    [ObservableProperty] private bool drawMaterials = true;
    [ObservableProperty] private bool showDofs;
    
    public void SetWpfPlot(IPlotControl wpfPlot) => _plotControl = wpfPlot;

    public void DrawMesh(IFiniteElementMesh mesh, IEnumerable<MaterialViewModel> materials)
    {
        _lastMesh = mesh;
        _lastMaterials = materials;
        Redraw();
    }

    partial void OnDrawMaterialsChanged(bool value) => Redraw();
    partial void OnShowDofsChanged(bool value) => Redraw();

    private void Redraw()
    {
        if (_plotControl is null || _lastMesh is null)
            return;
        
        _plotControl.Plot.Clear();
        _plotControl.DrawElements(_lastMesh, _lastMaterials, DrawMaterials);
        
        if (ShowDofs)
            _plotControl.DrawDofs(_lastMesh);
        
        if (_plotControl is null)
            return;

        _plotControl.Plot.Axes.AutoScale();
        _plotControl.Refresh();
    }
    
}