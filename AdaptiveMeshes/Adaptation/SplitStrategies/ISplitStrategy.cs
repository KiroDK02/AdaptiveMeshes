using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Adaptation.SplitStrategies
{
    public interface ISplitStrategy
    {
        enum SplitStrategyEnum
        {
            StrategyBasedOnScaleOfError
        }
        IDictionary<(int i, int j), int> GetSplits(IDictionary<(int i, int j), double> errors);
        IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesEdges(IDictionary<(int i, int j), int> splits, ref int countVertices);
    }
}
