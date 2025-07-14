using AdaptiveMeshes.NumericalIntegration;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.MasterElements
{
    public static class MasterELementsAlgorithms
    {
        public static double[,] CalcValuesBasicFunc1D(QuadratureNodes<double> quadratures, Func<double, double>[] basicFuncs)
        {
            double[,] values = new double[basicFuncs.Length, quadratures.Nodes.Length];

            for (int i = 0; i < basicFuncs.Length; i++)
                for (int j = 0; j < quadratures.Nodes.Length; j++)
                    values[i, j] = basicFuncs[i](quadratures.Nodes[j].Node);

            return values;
        }

        public static double[,] CalcValuesBasicFunc(QuadratureNodes<Vector2D> quadratures, Func<Vector2D, double>[] basicFuncs)
        {
            double[,] values = new double[basicFuncs.Length, quadratures.Nodes.Length];

            for (int i = 0; i < basicFuncs.Length; i++)
                for (int j = 0; j < quadratures.Nodes.Length; j++)
                    values[i, j] = basicFuncs[i](quadratures.Nodes[j].Node);

            return values;
        }

        public static Dictionary<(int, int), double[]> CalcPsiMultPsi1D(QuadratureNodes<double> quadratures, double[,] valuesBasicFuncs)
        {
            Dictionary<(int, int), double[]> result = new();
            int NumFunc = valuesBasicFuncs.GetLength(0);

            for (int i = 0; i < NumFunc; i++)
                for (int j = 0; j < NumFunc; j++)
                {
                    var values = new double[quadratures.Nodes.Length];

                    for (int k = 0; k < quadratures.Nodes.Length; k++)
                        values[k] = quadratures.Nodes[k].Weight * valuesBasicFuncs[i, k] * valuesBasicFuncs[j, k];

                    result.Add((i, j), values);
                }

            return result;
        }

        public static Dictionary<(int, int), double[]> CalcPsiMultPsi(QuadratureNodes<Vector2D> quadratures, double[,] valuesBaseFuncs)
        {
            Dictionary<(int, int), double[]> result = new();
            int numFunc = valuesBaseFuncs.GetLength(0);

            for (int i = 0; i < numFunc; i++)
                for (int j = 0; j < numFunc; j++)
                {
                    double[] values = new double[quadratures.Nodes.Length];

                    for (int k = 0; k < quadratures.Nodes.Length; k++)
                        values[k] = quadratures.Nodes[k].Weight * valuesBaseFuncs[i, k] * valuesBaseFuncs[j, k];

                    result.Add((i, j), values);
                }

            return result;
        }

        public static double[,,] CalcValuesGradientsBasicFunc(QuadratureNodes<Vector2D> quadratures, Func<Vector2D, double>[,] gradientsBasicFuncs)
        {
            double[,,] values = new double[gradientsBasicFuncs.GetLength(0), 2, quadratures.Nodes.Length];

            for (int i = 0; i < gradientsBasicFuncs.GetLength(0); i++)
                for (int j = 0; j < quadratures.Nodes.Length; j++)
                {
                    values[i, 0, j] = gradientsBasicFuncs[i, 0](quadratures.Nodes[j].Node);
                    values[i, 1, j] = gradientsBasicFuncs[i, 1](quadratures.Nodes[j].Node);
                }

            return values;
        }

        public static double[] CalcGradMultGrad(QuadratureNodes<Vector2D> quadratures, double[,,] valuesGradsFuncs, int i, int j, double[,] J)
        {
            double[] values = new double[quadratures.Nodes.Length];

            for (int k = 0; k < quadratures.Nodes.Length; k++)
                values[k] = quadratures.Nodes[k].Weight *
                    ((valuesGradsFuncs[i, 0, k] * J[0, 0] + valuesGradsFuncs[i, 1, k] * J[0, 1]) * (valuesGradsFuncs[j, 0, k] * J[0, 0] + valuesGradsFuncs[j, 1, k] * J[0, 1]) +
                     (valuesGradsFuncs[i, 0, k] * J[1, 0] + valuesGradsFuncs[i, 1, k] * J[1, 1]) * (valuesGradsFuncs[j, 0, k] * J[1, 0] + valuesGradsFuncs[j, 1, k] * J[1, 1]));

            return values;
        }
    }
}
