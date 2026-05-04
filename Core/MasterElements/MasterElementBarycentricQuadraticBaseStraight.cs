using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.NumericalIntegration;

namespace Core.MasterElements;

public class MasterElementBarycentricQuadraticBaseStraight : IMasterElement<double>
{
    private static readonly Lazy<MasterElementBarycentricQuadraticBaseStraight> LazyInstance =
        new(() => new MasterElementBarycentricQuadraticBaseStraight(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static MasterElementBarycentricQuadraticBaseStraight Instance => LazyInstance.Value;

    public Func<double, double>[] BasesFuncs => BaseFuncs.QuadraticBaseStraight.BasesFuncs;

    public Func<double, double>[,] GradientsBasesFuncs => throw new NotSupportedException();

    public double[,] ValuesBasicFuncs { get; }

    public double[,,] ValuesBasicFuncsGradients => throw new NotSupportedException();

    public QuadratureNodes<double> QuadratureNodes { get; }

    public IDictionary<(int, int), double[]> PsiProduct { get; }

    private MasterElementBarycentricQuadraticBaseStraight()
    {
        QuadratureNodes = new(NumericalIntegrationMethods
            .GaussQuadrature1DOrder7()
            .ToArray(), 7);
        
        ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc1D(QuadratureNodes, BasesFuncs);
        PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi1D(QuadratureNodes, ValuesBasicFuncs);
    }
}