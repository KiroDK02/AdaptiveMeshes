using AdaptiveMeshes.NumericalIntegration;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Interfaces
{
    public interface IMasterElement<T>
    {
        Func<T, double>[] BasesFuncs { get; }
        Func<T, double>[,] GradientsBasesFuncs { get; }
        
        double[,] ValuesBasicFuncs { get; }
        double[,,] ValuesBasicFuncsGradients { get; }

        QuadratureNodes<T> QuadratureNodes { get; }
        IDictionary<(int, int), double[]> PsiProduct { get; }
    }
}
