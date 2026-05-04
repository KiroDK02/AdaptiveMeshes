using System;
using System.Collections.Generic;
using System.Threading;
using Core.BaseFuncs;
using Core.NumericalIntegration;
using Core.Vectors;

namespace Core.MasterElements;

public class MasterElementTriangleHierarchicalBase : IMasterElement<Vector2D>
{
    private static readonly Lazy<MasterElementTriangleHierarchicalBase> LazyInstance =
        new(() => new MasterElementTriangleHierarchicalBase(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static MasterElementTriangleHierarchicalBase Instance => LazyInstance.Value;
    
    public Func<Vector2D, double>[] BasesFuncs => TriangleHierarchicalBase.BasesFuncs;
    public Func<Vector2D, double>[,] GradientsBasesFuncs => TriangleHierarchicalBase.GradientBasesFuncs;
    public double[,] ValuesBasicFuncs { get; }
    public double[,,] ValuesBasicFuncsGradients { get; }
    public QuadratureNodes<Vector2D> QuadratureNodes { get; }
    public IDictionary<(int, int), double[]> PsiProduct { get; }

    private MasterElementTriangleHierarchicalBase()
    {
        QuadratureNodes = new([..NumericalIntegrationMethods.GaussQuadratureTriangleOrder9()], 9);
        
        ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc(QuadratureNodes, BasesFuncs);
        ValuesBasicFuncsGradients = MasterELementsAlgorithms.CalcValuesGradientsBasicFunc(QuadratureNodes, GradientsBasesFuncs);
        PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi(QuadratureNodes, ValuesBasicFuncs);
    }
}