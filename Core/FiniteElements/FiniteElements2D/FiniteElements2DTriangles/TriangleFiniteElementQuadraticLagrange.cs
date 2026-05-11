using System;
using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;

public sealed class TriangleFiniteElementQuadraticLagrange : BaseTriangularFiniteElement, ICalculatingMatrices
{
    private const int EdgeLocalNumOffset = 3;
    
    public override IMasterElement<Vector2D> MasterElement { get; }
    public override int[] Dofs { get; } = new int[6];
    public override IDictionary<(int i, int j), int> EdgesDofs { get; }

    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType =>
        IFiniteElement.BasicFunctionsTypeEnum.Lagrange;

    public override int Order => 2;

    public TriangleFiniteElementQuadraticLagrange(string material, int[] vertexNumbers)
        : base(material, vertexNumbers)
    {
        MasterElement = MasterElementTriangleBarycentricQuadraticBase.Instance;

        EdgesDofs = new Dictionary<(int i, int j), int>();
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
        var localRightPart = new double[Dofs.Length];

        for (int i = 0; i < Dofs.Length; i++)
        {
            var valueIntegral = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                valueIntegral += nodes.Nodes[k].Weight * LocalF(nodes.Nodes[k].Node) * values[i, k];

            localRightPart[i] = Math.Abs(detD) * valueIntegral;
        }

        return localRightPart;

        double LocalF(Vector2D vert) => GetCoefAtLocalCoords(vertexCoords, f, vert);
    }

    public override int DofOnVertex(int vertex) => 1;

    public override int DofOnElement() => 0;

    public override void SetEdgeDof(int edge, int n, int dof) => Dofs[EdgeLocalNumOffset + edge] = dof;

    public override void SetElementDof(int n, int dof) => throw new NotSupportedException();

    public override void SetVertexDof(int vertex, int n, int dof) => Dofs[vertex] = dof;

    public override IEnumerable<(Vector2D position, int dof)> GetDofsWithPositions(Vector2D[] vertexCoords)
    {
        var vertices = VertexNumbers
            .Select(n => vertexCoords[n])
            .ToArray();
        
        for (int i = 0; i < EdgeLocalNumOffset; i++)
            yield return (vertices[i], Dofs[i]);

        for (int edgei = 0; edgei < NumberOfEdges; edgei++)
        {
            var edge = Edge(edgei);
            var mid = new Vector2D(
                (vertices[edge.i].X + vertices[edge.j].X) / 2.0,
                (vertices[edge.i].Y + vertices[edge.j].Y) / 2.0);

            yield return (mid, Dofs[EdgeLocalNumOffset + edgei]);
        }
    }

    public override string ToString() =>
        $"TriangleLagrange {Order} {VertexNumbers[0]} {VertexNumbers[1]} {VertexNumbers[2]} {Material}";

    protected override Vector2D GetGradientAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
    {
        var gradBasesFuncs = BaseFuncs.TriangleBarycentricQuadraticBase.GradientBasesFuncs;

        var valueXComp = 0.0;
        var valueYComp = 0.0;

        for (int i = 0; i < Dofs.Length; i++)
        {
            valueXComp += weights[Dofs[i]] * gradBasesFuncs[i, 0](localPoint);
            valueYComp += weights[Dofs[i]] * gradBasesFuncs[i, 1](localPoint);
        }

        return new(valueXComp, valueYComp);
    }

    protected override double GetValueAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
    {
        var basicFuncs = BaseFuncs.TriangleBarycentricQuadraticBase.BasesFuncs;

        var value = 0.0;

        for (int i = 0; i < Dofs.Length; i++)
            value += weights[Dofs[i]] * basicFuncs[i](localPoint);

        return value;
    }

    private double[,] BuildStiffnessMatrix(Vector2D[] vertexCoords, Func<Vector2D, double> lambda)
    {
        var detD = DetD(vertexCoords);
        var J = GetMatrixJacobi(vertexCoords);
        var nodes = MasterElement.QuadratureNodes;

        var localMatrix = new double[Dofs.Length, Dofs.Length];

        for (int i = 0; i < Dofs.Length; i++)
        {
            for (int j = 0; j < Dofs.Length; j++)
            {
                var values = MasterELementsAlgorithms.CalcGradMultGrad(nodes,
                    MasterElement.ValuesBasicFuncsGradients, i, j, J);

                var valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += LocalLambda(nodes.Nodes[k].Node) * values[k];

                localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
            }
        }

        return localMatrix;

        double LocalLambda(Vector2D point) => GetCoefAtLocalCoords(vertexCoords, lambda, point);
    }

    private double[,] BuildMassMatrix(Vector2D[] vertexCoords, Func<Vector2D, double> sigma)
    {
        var nodes = MasterElement.QuadratureNodes;
        var detD = DetD(vertexCoords);

        var localMatrix = new double[Dofs.Length, Dofs.Length];

        for (int i = 0; i < Dofs.Length; i++)
        {
            for (int j = 0; j < Dofs.Length; j++)
            {
                var values = MasterElement.PsiProduct[(i, j)];
                var valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += LocalSigma(nodes.Nodes[k].Node) * values[k];

                localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
            }
        }

        return localMatrix;

        double LocalSigma(Vector2D point) => GetCoefAtLocalCoords(vertexCoords, sigma, point);
    }
}