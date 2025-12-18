using Core.NumericalIntegration;
using Core.Vectors;

namespace Core.MasterElements;

public class MasterElementTriangleBarycentricQuadraticBase : IMasterElement<Vector2D>
{
    public static MasterElementTriangleBarycentricQuadraticBase Instance
    {
        get
        {
            field ??= new MasterElementTriangleBarycentricQuadraticBase();
            return field;
        }
    }

    public Func<Vector2D, double>[] BasesFuncs => BaseFuncs.TriangleBarycentricQuadraticBase.BasesFuncs;
    public Func<Vector2D, double>[,] GradientsBasesFuncs =>
        BaseFuncs.TriangleBarycentricQuadraticBase.GradientBasesFuncs;
    public double[,] ValuesBasicFuncs { get; }
    public double[,,] ValuesBasicFuncsGradients { get; }
    public QuadratureNodes<Vector2D> QuadratureNodes { get; }
    public IDictionary<(int, int), double[]> PsiProduct { get; }

    private MasterElementTriangleBarycentricQuadraticBase()
    {
        QuadratureNodes = new([.. NumericalIntegrationMethods.GaussQuadratureTriangleOrder6()], 6);
        ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc(QuadratureNodes, BasesFuncs);
        ValuesBasicFuncsGradients =
            MasterELementsAlgorithms.CalcValuesGradientsBasicFunc(QuadratureNodes, GradientsBasesFuncs);
        PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi(QuadratureNodes, ValuesBasicFuncs);
    }
}