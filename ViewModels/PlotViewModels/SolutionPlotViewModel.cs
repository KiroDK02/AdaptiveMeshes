using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Solution.Interfaces;
using Core.Vectors;
using ScottPlot;
using ScottPlot.Panels;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace ViewModels.PlotViewModels;

// TODO: Добавить квадро дерево, без него отрисовка долгая
public partial class SolutionPlotViewModel : ObservableObject
{
    private WpfPlot? _wpfPlot;
    private ISolution? _solution;
    private ColorBar? _colorBar;
    private Heatmap? _heatmap;

    [ObservableProperty] private int gridSize = 500;
    [ObservableProperty] private bool showColorMap = true;
    [ObservableProperty] private bool showIsolines = true;

    public void SetPlot(WpfPlot wpfPlot) => _wpfPlot = wpfPlot;

    public void SetSolution(ISolution solution) => _solution = solution;

    public void DrawSolution()
    {
        if (_wpfPlot is null || _solution is null)
            return;

        _wpfPlot.Plot.Clear();

        if (ShowColorMap)
        {
            DrawColorMap();
        }

        if (ShowIsolines)
        {
            // TODO: реализовать отрисовку изолиний
        }

        _wpfPlot.DrawElements(_solution.Mesh, null);

        _wpfPlot.Plot.Axes.AutoScale();
        _wpfPlot.Refresh();
    }

    partial void OnShowColorMapChanged(bool value)
    {
        if (_wpfPlot is null || _heatmap is null)
            return;

        _heatmap?.IsVisible = value;
        _colorBar?.IsVisible = value;
        _wpfPlot.Refresh();
    }

    private void DrawColorMap()
    {
        if (_wpfPlot is null || _solution is null)
            return;

        if (_colorBar is not null)
        {
            _wpfPlot.Plot.Axes.Remove(_colorBar);
            _colorBar = null;
        }

        if (_heatmap is not null)
        {
            _wpfPlot.Plot.Remove(_heatmap);
            _heatmap = null;
        }

        var heatMapData = CreateHeatMap(GridSize);
        _heatmap = _wpfPlot.Plot.Add.Heatmap(heatMapData.HeatMap);
        _heatmap.Extent = new CoordinateRect(
            heatMapData.Min.X,
            heatMapData.Max.X,
            heatMapData.Min.Y,
            heatMapData.Max.Y);
        _heatmap.Colormap = new ScottPlot.Colormaps.Turbo();
        _colorBar = _wpfPlot.Plot.Add.ColorBar(_heatmap);
    }

    private HeatMapData CreateHeatMap(int gridSize = 500)
    {
        if (_solution is null)
            return new HeatMapData(Vector2D.Zero, Vector2D.Zero, new Coordinates3d[0, 0]);

        var vertices = _solution.Mesh.Vertex;

        var heatMap = new Coordinates3d[gridSize, gridSize];
        
        var verticesX = vertices
            .Select(vertex => vertex.X)
            .ToArray();
        
        var verticesY = vertices
            .Select(vertex => vertex.Y)
            .ToArray();

        var min = new Vector2D(verticesX.Min(), verticesY.Min());
        var max = new Vector2D(verticesX.Max(), verticesY.Max());

        for (int i = 0; i < gridSize; i++)
        for (int j = 0; j < gridSize; j++)
        {
            var x = min.X + (max.X - min.X) * j / (gridSize - 1.0);
            var y = min.Y + (max.Y - min.Y) * (gridSize - 1 - i) / (gridSize - 1.0);
            var point = new Vector2D(x, y);

            if (_solution.Mesh.TryFindElementWithPoint(point, out var element)
                && element is not null)
                heatMap[i, j] = new Coordinates3d(x, y,
                    element.GetValueAtPoint(vertices, _solution.SolutionVector, point));
            else
                heatMap[i, j] = new Coordinates3d(x, y, double.NaN);
        }

        return new(min, max, heatMap);
    }
}

public class HeatMapData(Vector2D min, Vector2D max, Coordinates3d[,] heatMap)
{
    public Vector2D Min { get; } = min;
    public Vector2D Max { get; } = max;
    public Coordinates3d[,] HeatMap { get; } = heatMap;
}