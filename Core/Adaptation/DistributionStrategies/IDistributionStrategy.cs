using System.Collections.Generic;
using Core.Vectors;

namespace Core.Adaptation.DistributionStrategies;

public interface IDistributionStrategy
{
    IDictionary<(int i, int j), int> GetDistribution(IDictionary<(int i, int j), double> errors);

    public IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesEdges(
        IDictionary<(int i, int j), int> splits,
        ref int countVertices);
}