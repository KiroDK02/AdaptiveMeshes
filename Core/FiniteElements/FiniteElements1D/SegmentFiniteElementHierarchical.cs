using System;
using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Matrices;
using Core.SLAE;
using Core.SLAESolvers;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements1D;

public class SegmentFiniteElementHierarchical : BaseSegmentFiniteElement, ICalculatingMatricesForBoundaryConditions
{
    private const int NotSet = -1;
    private const int MaxDofs = 4;

    private const int EdgeNumOffset = 2;
    
    public override IMasterElement<double> MasterElement { get; }
    public override int[] Dofs { get; }
    public override IDictionary<(int i, int j), int> EdgesDofs { get; }
    
    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType => 
        IFiniteElement.BasicFunctionsTypeEnum.Hierarchical;
    public override int Order { get; }

    public SegmentFiniteElementHierarchical(string material, int[] vertexNumbers, int order) : base(material,
        vertexNumbers)
    {
        MasterElement = MasterElementHierarchicalSegment.Instance;
        Order = order;
        
        EdgesDofs = new Dictionary<(int i, int j), int>();
        Dofs = Enumerable
            .Repeat(NotSet, MaxDofs)
            .ToArray();
    }

    public double[] BuildLocalRightPartFirstBc(Vector2D[] vertexCoords, Func<Vector2D, double> ug)
    {
        if (Order == 1)
            return CalcLocalF(vertexCoords, ug);

        var splineSlae = BuildSlae(vertexCoords, ug);

        return SolveSlae(splineSlae);
    }
    
    public double[] BuildLocalRightPartSecondBc(Vector2D[] vertexCoords, Func<Vector2D, double> theta)
    {
        var lengthBound = vertexCoords[VertexNumbers[0]].Distance(vertexCoords[VertexNumbers[1]]);
        
        var nodes = MasterElement.QuadratureNodes;
        var values = MasterElement.ValuesBasicFuncs;
        var localRightPart = new double[Dofs.Count(dof => dof != NotSet)];

        var skipRowsCount = 0;

        for (int i = 0; i < Dofs.Length; i++)
        {
            if (Dofs[i] == NotSet)
            {
                skipRowsCount++;
                continue;
            }

            var value = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                value += nodes.Nodes[k].Weight * LocalTheta(nodes.Nodes[k].Node) * values[i, k];

            localRightPart[i - skipRowsCount] = value * lengthBound;
        }
        
        return localRightPart;
        
        double LocalTheta(double t) => GetCoefAtLocalCoords(vertexCoords, theta, t);
    }
    
    public override void SetVertexDof(int vertex, int n, int dof) => Dofs[vertex] = dof;
    
    public override void SetEdgeDof(int edge, int n, int dof) => Dofs[EdgeNumOffset + n] = dof;

    public override int DofOnVertex(int vertex) => 1;

    public override string ToString() => $"SegmentHierarchical {Order} {VertexNumbers[0]} {VertexNumbers[1]} {Material}";

    protected override double[] CalcLocalF(Vector2D[] vertexCoords, Func<Vector2D, double> F)
    {
        var localF = new double[Dofs.Count(dof => dof != NotSet)];

        localF[0] = F(vertexCoords[VertexNumbers[0]]);
        localF[1] = F(vertexCoords[VertexNumbers[1]]);

        return localF;
    }

    private PardisoSLAE BuildSlae(Vector2D[] vertexCoords, Func<Vector2D, double> ug)
    {
        var lengthBound = vertexCoords[VertexNumbers[0]].Distance(vertexCoords[VertexNumbers[1]]);
        var nodes = MasterElement.QuadratureNodes;
        var valuesPsi = MasterElement.ValuesBasicFuncs;
        
        var sizeM = Dofs.Count(dof => dof != NotSet);

        var M = new double[sizeM, sizeM];
        var b = new double[sizeM];

        var skipRowsCount = 0;

        for (int i = 0; i < Dofs.Length; i++)
        {
            if (Dofs[i] == NotSet)
            {
                skipRowsCount++;
                continue;
            }

            var skipColumnsCount = 0;

            for (int j = 0; j < Dofs.Length; j++)
            {
                if (Dofs[j] == NotSet)
                {
                    skipColumnsCount++;
                    continue;
                }
                
                var values = MasterElement.PsiProduct[(i, j)];
                var valueM = 0.0;
                
                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueM += values[k];
                
                M[i - skipRowsCount, j - skipColumnsCount] = valueM * lengthBound;
            }

            var valueB = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                valueB += nodes.Nodes[k].Weight * LocalUg(nodes.Nodes[k].Node) * valuesPsi[i, k];
            
            b[i - skipRowsCount] = valueB * lengthBound;
        }

        var profile = new SortedSet<int>[sizeM];
        var temp = new int[sizeM];

        for (int i = 0; i < sizeM; i++)
        {
            temp[i] = i;
            profile[i] = [];

            for (int j = 0; j < sizeM; j++)
                profile[i].Add(j);
        }
        
        var slae = new PardisoSLAE(new PardisoMatrix(profile, Quasar.Native.PardisoMatrixType.SymmetricIndefinite));

        slae.Matrix.AddLocal(temp, M);
        slae.AddLocalRightPart(temp, b);

        return slae;
        
        double LocalUg(double t) => GetCoefAtLocalCoords(vertexCoords, ug, t);
    }

    private static double[] SolveSlae(PardisoSLAE slae)
    {
        using PardisoSLAESolver solver = new(slae);
        solver.Prepare();
        return solver.Solve();
    }
}