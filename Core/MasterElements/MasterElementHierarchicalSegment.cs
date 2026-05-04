using System;
using System.Collections.Generic;
using System.Threading;
using Core.NumericalIntegration;

namespace Core.MasterElements;

public class MasterElementHierarchicalSegment : IMasterElement<double>
{
    private static readonly Lazy<MasterElementHierarchicalSegment> LazyInstance = new(() => new 
        MasterElementHierarchicalSegment(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static MasterElementHierarchicalSegment Instance => LazyInstance.Value;

    public Func<double, double>[] BasesFuncs => BaseFuncs.SegmentHierarchicalBase.BasesFuncs;
    public Func<double, double>[,] GradientsBasesFuncs => throw new NotSupportedException();
    public double[,] ValuesBasicFuncs { get; }
    public double[,,] ValuesBasicFuncsGradients => throw new NotSupportedException();
    public QuadratureNodes<double> QuadratureNodes { get; }
    public IDictionary<(int, int), double[]> PsiProduct { get; }

    private MasterElementHierarchicalSegment()
    {
        QuadratureNodes = new([..NumericalIntegrationMethods.GaussQuadrature1DOrder9()], 9);
        
        ValuesBasicFuncs = MasterELementsAlgorithms.CalcValuesBasicFunc1D(QuadratureNodes, BasesFuncs);
        PsiProduct = MasterELementsAlgorithms.CalcPsiMultPsi1D(QuadratureNodes, ValuesBasicFuncs);
    }
}