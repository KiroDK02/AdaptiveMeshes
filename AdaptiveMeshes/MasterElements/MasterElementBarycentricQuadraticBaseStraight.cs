using AdaptiveMeshes.NumericalIntegration;

namespace AdaptiveMeshes.MasterElements
{
    public class MasterElementBarycentricQuadraticBaseStraight : IMasterElement<double>
    {
        private static MasterElementBarycentricQuadraticBaseStraight? instance;
        public static MasterElementBarycentricQuadraticBaseStraight Instance
        {
            get
            {
                instance ??= new MasterElementBarycentricQuadraticBaseStraight();
                return instance;
            }
        }

        public Func<double, double>[] BasesFuncs => BaseFuncs.QuadraticBaseStraight.BasesFuncs;

        public Func<double, double>[,] GradientsBasesFuncs => throw new NotSupportedException();

        public double[,] ValuesBasicFuncs { get; } = new double[3, 4];

        public double[,,] ValuesBasicFuncsGradients => throw new NotSupportedException();

        public QuadratureNodes<double> QuadratureNodes { get; }

        public IDictionary<(int, int), double[]> PsiProduct { get; }

        private MasterElementBarycentricQuadraticBaseStraight()
        {
            QuadratureNodes = new(NumericalIntegrationMethods.GaussQuadrature1DOrder7().ToArray(), 7);
            ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc1D(QuadratureNodes, BasesFuncs);
            PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi1D(QuadratureNodes, ValuesBasicFuncs);
        }
    }
}
