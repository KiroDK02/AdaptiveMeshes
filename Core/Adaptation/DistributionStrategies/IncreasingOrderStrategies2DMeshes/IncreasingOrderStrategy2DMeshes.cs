using System;
using System.Collections.Generic;
using System.Linq;
using Core.Adaptation.Adapters;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.Adaptation.DistributionStrategies.IncreasingOrderStrategies2DMeshes;

public class IncreasingOrderStrategy2DMeshes : IDistributionStrategy
{
    private readonly IDictionary<(int i, int j), int> _amountOccurrencesOfEdges;
    private readonly Scales _scales;
    
    public double[] ScaleDifferences => _scales.ScaleDifferences;
    public int[] ScaleIncreasingOrders => _scales.ScaleDistribution;
    
    public IEnumerable<IFiniteElement> Elements { get; }
    public Vector2D[] Vertices { get; }

    public IncreasingOrderStrategy2DMeshes(
        double[] distanceFromMinForScaleDifferences,
        int[] scaleIncreasingOrders,
        IEnumerable<IFiniteElement> elements,
        Vector2D[] vertices)
        : this(elements, vertices)
    {
        _scales = new(
            distanceFromMinForScaleDifferences,
            scaleIncreasingOrders,
            _amountOccurrencesOfEdges);
    }   
    
    public IncreasingOrderStrategy2DMeshes(IEnumerable<IFiniteElement> elements, Vector2D[] vertices)
    {
        Elements = elements;
        Vertices = vertices;
        _amountOccurrencesOfEdges = AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(Elements);
        
        _scales = new Scales(_amountOccurrencesOfEdges);
    }
    
    public IDictionary<(int i, int j), int> GetDistribution(IDictionary<(int i, int j), double> errors)
    {
        _scales.CalculateScaleDifferences(errors);
        
        var orders = _scales.FindEdgeDistribution(errors);
        GetSmoothedOutOrdersDistribution(orders);

        return orders;
    }

    public IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesEdges(
        IDictionary<(int i, int j), int> splits,
        ref int countVertices) => throw new NotSupportedException();
    
    private void GetSmoothedOutOrdersDistribution(IDictionary<(int i, int j), int> orders)
    {
        var stop = false;

        while (!stop)
        {
            stop = true;
            foreach (var element in Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                var maxOrder = FindMaxOrderInElement(orders, element);

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);

                    var differenceOrders = maxOrder - orders[edge];
                    
                    if (differenceOrders > 1)
                    {
                        stop = false;
                        orders[edge] = maxOrder - 1;
                    }
                }
            }
        }
    }

    private static int FindMaxOrderInElement(IDictionary<(int i, int j), int> orders, IFiniteElement element)
        => Enumerable
            .Range(0, element.NumberOfEdges)
            .Select(element.GlobalEdge)
            .Max(edge => orders[edge]);
}