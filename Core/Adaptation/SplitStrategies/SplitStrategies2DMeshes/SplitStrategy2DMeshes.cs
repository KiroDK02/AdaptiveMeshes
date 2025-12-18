using Core.FiniteElements.AlgorithmsForFE;
using Core.Adaptation.Adapters;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.Adaptation.SplitStrategies.SplitStrategies2DMeshes;

public class SplitStrategy2DMeshes : ISplitStrategy
{
    private readonly double[] _distanceFromMinForScaleDifferences;
    private readonly int[] _scaleSplits;
    private readonly IDictionary<(int i, int j), int> _amountOccurencesOfEdges;
    private double[] _scaleDifferences;

    /// <value>
    /// Элементы начальной сетки
    /// </value>
    public IEnumerable<IFiniteElement> Elements { get; }

    // для циклической адаптации, чтоб не пересоздавать новый экземпляр стратегии, придется устанавливать
    // Vertices на массив новых вершин после адаптации, так проще всего в плане реализации и в принципе логике,
    // более хорошего варианта пока не придумалось.
    /// <value>
    /// Вершины последней сетки, то есть если циклическая адаптация и дробление уже было, то вершины новой сетки.
    /// </value>
    public Vector2D[] Vertices { get; set; }

    public IDictionary<(int i, int j), int> EdgesSplits { get; set; }
    public IDictionary<(int i, int j), (int i, int j)> NumbersOldEdgesForNewEdges { get; set; }

    public SplitStrategy2DMeshes(IEnumerable<IFiniteElement> elements, Vector2D[] vertices)
        : this([0.0, 0.25, 0.5, 0.75, 1.0], [0, 1, 2, 3], elements, vertices)
    { }

    public SplitStrategy2DMeshes(double[] distanceFromMinForScaleDifferences, int[] scaleSplits,
        IEnumerable<IFiniteElement> elements, Vector2D[] vertices)
    {
        if (distanceFromMinForScaleDifferences.Length != scaleSplits.Length + 1)
            throw new ArgumentException("Invalid scales. Sizes of scales are not equal.");

        if (distanceFromMinForScaleDifferences.Any(x => x > 1 || x < 0))
            throw new ArgumentException(
                "Invalid set of distance from min. The values must be in the range from 0 to 1.");

        _distanceFromMinForScaleDifferences = distanceFromMinForScaleDifferences;
        _scaleSplits = scaleSplits;
        Elements = elements;
        Vertices = vertices;

        _amountOccurencesOfEdges = AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(Elements);
        _scaleDifferences = [];
        EdgesSplits = new Dictionary<(int i, int j), int>();
        NumbersOldEdgesForNewEdges = new Dictionary<(int i, int j), (int i, int j)>();
    }
    
    public IDictionary<(int i, int j), int> GetSplits(IDictionary<(int i, int j), double> errors)
    {
        GetScaleDifferences(errors);

        EdgesSplits = EdgesSplits.Count == 0
            ? FindEdgesSplitsFirstStep(errors)
            : FindEdgesSplits(errors);

        var splits = DistributeFoundSplits();
        SmoothOutSplits(splits);

        return splits;
    }

    public IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesEdges(
        IDictionary<(int i, int j), int> splits, ref int countVertices)
    {
        Dictionary<(int i, int j), (Vector2D vert, int num)[]> verticesEdges = [];
        NumbersOldEdgesForNewEdges.Clear();

        foreach (var element in Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);

                if (verticesEdges.ContainsKey(edge))
                    continue;

                var v0 = Vertices[edge.i];
                var v1 = Vertices[edge.j];

                var split = (int)Math.Pow(2, splits[edge]);
                var h = (v1 - v0) / split;

                var newVertices = new (Vector2D vert, int num)[split + 1];

                newVertices[0] = (v0, edge.i);
                for (int k = 1; k < split; k++)
                {
                    var newVertex = v0 + h * k;
                    newVertices[k] = (newVertex, countVertices++);
                }

                newVertices[split] = (v1, edge.j);

                verticesEdges[edge] = newVertices;

                if (split == 1)
                    continue;

                for (int k = 0; k < newVertices.Length - 1; k++)
                {
                    (int i, int j) newEdge = (newVertices[k].num, newVertices[k + 1].num);
                    if (newEdge.i > newEdge.j)
                        newEdge = (newEdge.j, newEdge.i);

                    NumbersOldEdgesForNewEdges[newEdge] = edge;
                }
            }
        }

        return verticesEdges;
    }

    private void GetScaleDifferences(IDictionary<(int i, int j), double> errors)
    {
        var maxError = errors.Values.Max();
        var minError = errors
            .Where(edge => _amountOccurencesOfEdges[edge.Key] != 1)
            .MinBy(edge => edge.Value)
            .Value;
        var step = maxError - minError;

        _scaleDifferences = new double[_distanceFromMinForScaleDifferences.Length];

        _scaleDifferences[0] = minError;

        for (int i = 1; i < _scaleDifferences.Length - 1; i++)
            _scaleDifferences[i] = minError + step * _distanceFromMinForScaleDifferences[i];

        _scaleDifferences[^1] = maxError;
    }

    private IDictionary<(int i, int j), int> FindEdgesSplitsFirstStep(IDictionary<(int i, int j), double> errors)
    {
        Dictionary<(int i, int j), int> splits = [];

        foreach ((var edge, double error) in errors)
        {
            for (int i = 0; i < _scaleSplits.Length; i++)
                if (error <= _scaleDifferences[i + 1])
                {
                    splits[edge] = _scaleSplits[i];
                    break;
                }
        }

        return splits;
    }

    private IDictionary<(int i, int j), int> FindEdgesSplits(IDictionary<(int i, int j), double> errors)
    {
        Dictionary<(int i, int j), int> splits = [];
        var maxNumber = EdgesSplits.Keys
            .SelectMany(edge => new[] { edge.i, edge.j })
            .Max();

        foreach ((var edge, double error) in errors)
        {
            var split = 0;
            for (int i = 0; i < _scaleSplits.Length; i++)
            {
                if (error < _scaleDifferences[i + 1])
                {
                    split = _scaleSplits[i];
                    break;
                }
            }

            if (edge.i <= maxNumber && edge.j <= maxNumber)
            {
                splits[edge] = EdgesSplits[edge] + split;
            }
            else if (NumbersOldEdgesForNewEdges.TryGetValue(edge, out var oldEdge))
            {
                if (!splits.TryGetValue(oldEdge, out int currentSplit) || currentSplit < split + EdgesSplits[oldEdge])
                    splits[oldEdge] = EdgesSplits[oldEdge] + split;
            }
            else
                AddSplitsToElementWithEdge(edge, split, splits);
        }

        return splits;
    }

    private IDictionary<(int i, int j), int> DistributeFoundSplits()
    {
        Dictionary<(int i, int j), int> splits = [];

        foreach (var element in Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var curEdge = element.GlobalEdge(edgei);
                var splitFromCurEdge = EdgesSplits[curEdge];

                for (int edgej = 0; edgej < element.NumberOfEdges; edgej++)
                {
                    if (edgei == edgej)
                        continue;

                    var edge = element.GlobalEdge(edgej);
                    if (!splits.TryGetValue(edge, out int currentSplit) || currentSplit < splitFromCurEdge)
                        splits[edge] = splitFromCurEdge;
                }
            }
        }

        return splits;
    }

    private void SmoothOutSplits(IDictionary<(int i, int j), int> splits)
    {
        var stop = false;

        while (!stop)
        {
            stop = true;
            foreach (var element in Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                var maxSplit = FindMaxSplitInElement(splits, element);

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);

                    var differenceBtwSplits = maxSplit - splits[edge];
                    if (differenceBtwSplits > 1)
                    {
                        stop = false;
                        splits[edge] = maxSplit - 1;
                    }
                }
            }
        }
    }

    private void AddSplitsToElementWithEdge((int i, int j) edge, int split, IDictionary<(int i, int j), int> splits)
    {
        var middleOfEdge = (Vertices[edge.i] + Vertices[edge.j]) / 2.0;

        foreach (var element in Elements)
        {
            if (element.VertexNumbers.Length != 2 && element.IsPointOnElement(Vertices, middleOfEdge))
            {
                for (int i = 0; i < element.NumberOfEdges; i++)
                {
                    var edgei = element.GlobalEdge(i);

                    if (!splits.TryGetValue(edgei, out int currentSplit) || currentSplit < EdgesSplits[edgei] + split)
                        splits[edgei] = EdgesSplits[edgei] + split;
                }

                return;
            }
        }
    }

    private int FindMaxSplitInElement(IDictionary<(int i, int j), int> splits, IFiniteElement element)
    {
        int max = 0;

        for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
        {
            var edge = element.GlobalEdge(edgei);
            max = int.Max(max, splits[edge]);
        }

        return max;
    }
}