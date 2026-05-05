using System;
using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;

public sealed class TriangleFiniteElementHierarchical : BaseTriangularFiniteElement, ICalculatingMatrices
{
    private const int MaxDofs = 10;
    private const int NotSet = -1;

    private const int EdgeLocalNumOffset = 3;
    private const int ElementLocalNumOffset = 9;

    public override IMasterElement<Vector2D> MasterElement { get; }
    public override int[] Dofs { get; }
    public override IDictionary<(int i, int j), int> EdgesDofs { get; }

    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType => IFiniteElement.BasicFunctionsTypeEnum
        .Hierarchical;

    public override int Order { get; }

    public TriangleFiniteElementHierarchical(string material, int[] vertexNumbers, int order) : base(material,
        vertexNumbers)
    {
        MasterElement = MasterElementTriangleHierarchicalBase.Instance;
        Order = order;

        EdgesDofs = new Dictionary<(int i, int j), int>();
        Dofs = Enumerable
            .Repeat(NotSet, MaxDofs)
            .ToArray();
    }

    public override void SetVertexDof(int vertex, int n, int dof) => Dofs[vertex] = dof;

    public override void SetEdgeDof(int edge, int n, int dof) => 
        Dofs[EdgeLocalNumOffset + edge + 3 * n] = dof;

    public override void SetElementDof(int n, int dof) => Dofs[ElementLocalNumOffset] = dof;

    public override int DofOnVertex(int vertex) => 1;

    public override int DofOnElement() => Order == 3 ? 1 : 0;

    public override IEnumerable<(Vector2D position, int dof)> GetDofsWithPositions(Vector2D[] vertexCoords)
    {
        var vertices = VertexNumbers
            .Select(n => vertexCoords[n])
            .ToArray();

        for (int i = 0; i < EdgeLocalNumOffset; i++)
            if (Dofs[i] != NotSet)
                yield return (vertices[i], Dofs[i]);

        for (int edgei = 0; edgei < NumberOfEdges; edgei++)
        {
            var edge = Edge(edgei);
            var mid = new Vector2D(
                (vertices[edge.i].X + vertices[edge.j].X) / 2.0,
                (vertices[edge.i].Y + vertices[edge.j].Y) / 2.0);

            for (int n = 0; n < Order - 1; n++)
            {
                var dof = Dofs[3 + edgei + 3 * n];
                
                if (dof != NotSet)
                    yield return (mid, dof);
            }
        }

        if (Dofs[ElementLocalNumOffset] != NotSet)
        {
            var center = new Vector2D(
                vertices.Average(v => v.X),
                vertices.Average(v => v.Y));
            
            yield return (center, Dofs[ElementLocalNumOffset]);
        }
    }

    public double[,] BuildLocalMatrix(
        Vector2D[] vertexCoords, 
        IFiniteElement.MatrixTypeEnum type, 
        Func<Vector2D, double> coefficient)
    {
        return type switch
        {
            IFiniteElement.MatrixTypeEnum.Stiffness => BuildStiffnessMatrix(vertexCoords, coefficient),
            IFiniteElement.MatrixTypeEnum.Mass => BuildMassMatrix(vertexCoords, coefficient),
            _ => throw new ArgumentException("Invalid type of matrix.")
        };
    }
    
    public double[] BuildLocalRightPart(Vector2D[] vertexCoords, Func<Vector2D, double> f)
    {
        var nodes = MasterElement.QuadratureNodes;
        var values = MasterElement.ValuesBasicFuncs;

        var detD = DetD(vertexCoords);
        var localRightPart = new double[Dofs.Count(dof => dof != NotSet)];

        var skipRowsCount = 0;
        
        for (int i = 0; i < Dofs.Length; i++)
        {
            if (Dofs[i] == NotSet)
            {
                skipRowsCount++;
                continue;
            }

            var valueIntegral = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                valueIntegral += nodes.Nodes[k].Weight * LocalF(nodes.Nodes[k].Node) * values[i, k];

            localRightPart[i - skipRowsCount] = Math.Abs(detD) * valueIntegral;
        }

        return localRightPart;
        
        double LocalF(Vector2D vert) => GetCoefAtLocalCoords(vertexCoords, f, vert);
    }
    
    public override string ToString() =>
        $"TriangleHierarchical {Order} {VertexNumbers[0]} {VertexNumbers[1]} {VertexNumbers[2]} {Material}";
    
    protected override Vector2D GetGradientAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
    {
        var gradBasesFuncs = MasterElement.GradientsBasesFuncs;

        var valueXComp = 0.0;
        var valueYComp = 0.0;

        for (int i = 0; i < Dofs.Length; i++)
        {
            if (Dofs[i] == NotSet)
                continue;

            valueXComp += weights[Dofs[i]] * gradBasesFuncs[i, 0](localPoint);
            valueYComp += weights[Dofs[i]] * gradBasesFuncs[i, 1](localPoint);
        }

        return new(valueXComp, valueYComp);
    }

    protected override double GetValueAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
    {
        var basesFuncs = MasterElement.BasesFuncs;

        var value = 0.0;

        for (int i = 0; i < Dofs.Length; i++)
        {
            if (Dofs[i] == NotSet)
                continue;
            
            value += weights[Dofs[i]] * basesFuncs[i](localPoint);
        }

        return value;
    }
    
    private double[,] BuildStiffnessMatrix(Vector2D[] vertexCoords, Func<Vector2D, double> lambda)
    {
        var detD = DetD(vertexCoords);
        var J = GetMatrixJacobi(vertexCoords);
        var nodes = MasterElement.QuadratureNodes;

        var matrixSize = Dofs.Count(dof => dof != NotSet);
        
        var localMatrix = new double[matrixSize, matrixSize];

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
                
                var values = MasterELementsAlgorithms.CalcGradMultGrad(nodes,
                    MasterElement.ValuesBasicFuncsGradients, i, j, J);

                var valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += LocalLambda(nodes.Nodes[k].Node) * values[k];

                localMatrix[i - skipRowsCount, j - skipColumnsCount] = Math.Abs(detD) * valueIntegral;
            }
        }

        return localMatrix;

        double LocalLambda(Vector2D point) => GetCoefAtLocalCoords(vertexCoords, lambda, point);
    }

    private double[,] BuildMassMatrix(Vector2D[] vertexCoords, Func<Vector2D, double> sigma)
    {
        var nodes = MasterElement.QuadratureNodes;
        var detD = DetD(vertexCoords);

        var matrixSize = Dofs.Count(dof => dof != NotSet);
        
        var localMatrix = new double[matrixSize, matrixSize];

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
                var valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += LocalSigma(nodes.Nodes[k].Node) * values[k];

                localMatrix[i - skipRowsCount, j - skipColumnsCount] = Math.Abs(detD) * valueIntegral;
            }
        }

        return localMatrix;

        double LocalSigma(Vector2D point) => GetCoefAtLocalCoords(vertexCoords, sigma, point);
    }
}