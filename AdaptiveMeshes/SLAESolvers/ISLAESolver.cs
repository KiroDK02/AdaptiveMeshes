using AdaptiveMeshes.SLAE;

namespace AdaptiveMeshes.SLAESolvers
{
    public interface ISLAESolver : IDisposable
    {
        ISLAE SLAE { get; }
        double[] Solve();
    }
}
