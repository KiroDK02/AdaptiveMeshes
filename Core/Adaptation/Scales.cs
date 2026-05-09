using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Adaptation;

public class Scales
{
    private readonly double[] _distanceFromMinForScaleDifferences;
    private readonly IDictionary<(int i, int j), int> _amountOccurrencesOfEdges;
    
    public double[] ScaleDifferences { get; }
    public int[] ScaleDistribution { get; }

    public Scales(IDictionary<(int i, int j), 
        int> amountOccurrencesOfEdges)
        : this([0.0, 0.25, 0.5, 0.75, 1.0], [0, 1, 2, 3], amountOccurrencesOfEdges) { }
    
    public Scales(double[] distanceFromMinForScaleDifferences, int[] scaleDistribution, IDictionary<(int i, int j), 
            int> amountOccurrencesOfEdges)
    {
        if (distanceFromMinForScaleDifferences.Length != scaleDistribution.Length + 1)
            throw new ArgumentException("Invalid scales. Sizes of scales are not equal.");
            
        if (distanceFromMinForScaleDifferences.Any(x => x is > 1 or < 0))
            throw new ArgumentException(
                "Invalid set of distance from min. The values must be in the range from 0 to 1.");

        _distanceFromMinForScaleDifferences = distanceFromMinForScaleDifferences;
        ScaleDistribution = scaleDistribution;
        ScaleDifferences = new double[_distanceFromMinForScaleDifferences.Length];
        _amountOccurrencesOfEdges = amountOccurrencesOfEdges;
    }
    
    public void CalculateScaleDifferences(IDictionary<(int i, int j), double> errors)
    {
        var maxError = errors.Values.Max();
        var minError = errors
            .Where(edge => _amountOccurrencesOfEdges[edge.Key] != 1)
            .MinBy(edge => edge.Value)
            .Value;
        var step = maxError - minError;
        
        ScaleDifferences[0] = minError;

        for (int i = 1; i < ScaleDifferences.Length - 1; i++)
            ScaleDifferences[i] = minError + step * _distanceFromMinForScaleDifferences[i];

        ScaleDifferences[^1] = maxError;
    }
    
    public IDictionary<(int i, int j), int> FindEdgeDistribution(IDictionary<(int i, int j), double> errors)
    {
        Dictionary<(int i, int j), int> distribution = [];

        foreach ((var edge, double error) in errors)
        {
            for (int i = 0; i < ScaleDistribution.Length; i++)
                if (error <= ScaleDifferences[i + 1])
                {
                    distribution[edge] = ScaleDistribution[i];
                    break;
                }
        }

        return distribution;
    }
}