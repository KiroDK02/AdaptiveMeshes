using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
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
    
    [ObservableProperty] private bool _drawMaterials = true;
    [ObservableProperty] private bool _showDofs;
    
    public void SetWpfPlot(IPlotControl wpfPlot) => _plotControl = wpfPlot;

    public async Task DrawMeshAsync(IFiniteElementMesh mesh, IEnumerable<MaterialViewModel> materials)
    {
        _lastMesh = mesh;
        _lastMaterials = materials;
        
        await Redraw();
    }

    partial void OnDrawMaterialsChanged(bool value) => _ = Redraw();
    partial void OnShowDofsChanged(bool value) => _ = Redraw();

    private async Task Redraw()
    {
        if (_plotControl is null || _lastMesh is null)
            return;
        try
        {
            _plotControl.Plot.Clear();
            await Task.Run(() => _plotControl.DrawElements(_lastMesh, _lastMaterials, DrawMaterials));

            if (ShowDofs)
                await Task.Run(() => _plotControl.DrawDofs(_lastMesh));

            if (_plotControl is null)
                return;

            _plotControl.Plot.Axes.AutoScale();
            _plotControl.Refresh();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Redraw failed: {ex.Message}");
        }
    }
}