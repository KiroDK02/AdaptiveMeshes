using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;

public class TriangleFiniteElementQuadraticLagrange : BaseTriangularFiniteElement, ICalculatingMatrices
{
    private readonly string _toStringObject;

    public override IMasterElement<Vector2D> MasterElement { get; }
    public override int[] Dofs { get; } = new int[6];
    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType => 
        IFiniteElement.BasicFunctionsTypeEnum.Lagrange;

    public override int Order => 2;

    public TriangleFiniteElementQuadraticLagrange(string material, int[] vertexNumbers)
        : base(material, vertexNumbers)
    {
        MasterElement = MasterElementTriangleBarycentricQuadraticBase.Instance;
        _toStringObject = $"TriangleLagrange2 {VertexNumbers[0]} {VertexNumbers[1]} {VertexNumbers[2]} {Material}";
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

    public override int DofOnEdge(int edge) => 1;

    public override int DofOnElement() => 0;

    public override void SetEdgeDof(int edge, int n, int dof)
    {
        switch (edge)
        {
            case 0:
                Dofs[3] = dof;
                break;

            case 1:
                Dofs[4] = dof;
                break;

            case 2:
                Dofs[5] = dof;
                break;

            default:
                throw new Exception("Invalid number of edge");
        }
    }

    public override void SetElementDof(int n, int dof) => throw new NotSupportedException();

    public override void SetVertexDof(int vertex, int n, int dof)
    {
        switch (vertex)
        {
            case 0:
                Dofs[0] = dof;
                break;

            case 1:
                Dofs[1] = dof;
                break;

            case 2:
                Dofs[2] = dof;
                break;

            default:
                throw new Exception("Invalid number of vertex");
        }
    }

    public override string ToString() => _toStringObject;

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