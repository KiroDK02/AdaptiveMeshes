using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.NumericalIntegration;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.MasterElements
{
    public class MasterElementTriangleBarycentricQuadraticBase : IMasterElement<Vector2D>
    {
        private static MasterElementTriangleBarycentricQuadraticBase? instance;

        public static MasterElementTriangleBarycentricQuadraticBase Instance
        {
            get
            {
                instance ??= new MasterElementTriangleBarycentricQuadraticBase();
                return instance;
            }
        }

        private MasterElementTriangleBarycentricQuadraticBase()
        {
            QuadratureNodes = new([.. NumericalIntegrationMethods.GaussQuadratureTriangleOrder6()], 6);
            ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc(QuadratureNodes, BasesFuncs);
            ValuesBasicFuncsGradients = MasterELementsAlgorithms.CalcValuesGradientsBasicFunc(QuadratureNodes, GradientsBasesFuncs);
            PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi(QuadratureNodes, ValuesBasicFuncs);
        }

        public Func<Vector2D, double>[] BasesFuncs => BaseFuncs.TriangleBarycentricQuadraticBase.BasesFuncs;
        public Func<Vector2D, double>[,] GradientsBasesFuncs => BaseFuncs.TriangleBarycentricQuadraticBase.GradientBasesFuncs;
        public double[,] ValuesBasicFuncs { get; } = new double[6, 12];
        public double[,,] ValuesBasicFuncsGradients { get; } = new double[6, 2, 12];
        public QuadratureNodes<Vector2D> QuadratureNodes { get; }
        public IDictionary<(int, int), double[]> PsiProduct { get; }
    }
}
