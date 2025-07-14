namespace AdaptiveMeshes.Interfaces
{
    public interface ISLAESolver : IDisposable
    {
        ISLAE SLAE { get; }
        double[] Solve();
    }
}
