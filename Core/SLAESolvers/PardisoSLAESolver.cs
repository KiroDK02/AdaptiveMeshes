using Core.SLAE;
using Quasar.Native;

namespace Core.SLAESolvers;

public class PardisoSLAESolver : ISLAESolver
{
    public ISLAE SLAE { get; }
    Pardiso<double> Pardiso { get; }

    public PardisoSLAESolver(ISLAE slae)
    {
        SLAE = slae;
        Pardiso = new Pardiso<double>((IPardisoMatrix<double>)slae.Matrix);
    }

    public void Prepare()
    {
        Pardiso.Analysis();
        Pardiso.Factorization();
    }

    public double[] Solve()
    {
        var solution = new double[SLAE.Matrix.N];

        Pardiso.Solve(SLAE.RightPart, solution);

        return solution;
    }

    private bool disposed = false;

    ~PardisoSLAESolver()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        Pardiso.Dispose();
        disposed = true;
    }
}