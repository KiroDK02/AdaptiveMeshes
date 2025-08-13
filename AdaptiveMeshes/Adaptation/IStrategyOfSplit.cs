namespace AdaptiveMeshes.Adaptation
{
    public interface IStrategyOfSplit
    {
        IDictionary<(int i, int j), int> GetSplits(IDictionary<(int i, int j), double> errors);
    }
}
