using AdaptiveMeshes;
using AdaptiveMeshes.Adaptation.Adapters.Adapters2DMeshes;
using AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;
using AdaptiveMeshes.Adaptation.SplitStrategies.SplitStrategies2DMeshes;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.FiniteElements1D;
using AdaptiveMeshes.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using AdaptiveMeshes.Problems;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Vectors;

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

Console.WriteLine("Hello, World!");

/*Dictionary<string, IMaterial> materials = [];

materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0.0,                   (x, t) => 0.0, (x, t) => -4 + x.X * x.X + x.Y * x.Y));
materials.Add("1",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("2",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + 4,         (x, t) => 0.0, (x, t) => 0));
materials.Add("3",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("4",      new Material(false, false, true, x => 1, x => 1, (x, t) => 0.0,                   (x, t) =>-2.0, (x, t) => 0));

Vector2D[] vertex = [new(1, 1), new(3, 1), new(2, 2), new(4, 2)];

IEnumerable<IFiniteElement> elements = [new TriangleFEQuadraticBaseWithNI("volume", [0, 1, 3]), new TriangleFEQuadraticBaseWithNI("volume", [0, 3, 2]),
                                        new SegmentFEQuadraticBaseWithNI("1", [0, 2]), new SegmentFEQuadraticBaseWithNI("2", [2, 3]),
                                        new SegmentFEQuadraticBaseWithNI("3", [3, 1]), new SegmentFEQuadraticBaseWithNI("4", [0, 1])];

FiniteElementMesh mesh = new(elements, vertex);

EllipticalProblem ellipticalProblem = new(materials, mesh);

ellipticalProblem.Prepare();
Console.WriteLine(ellipticalProblem.Solve());

Console.WriteLine(ellipticalProblem.Solution.Value(new(1.5, 1.5)));
Console.WriteLine(ellipticalProblem.Solution.Gradient(new(3, 1.5)));
Console.WriteLine(ellipticalProblem.Solution.Flow(new(3, 1.5), materials));*/

FileManager fileManager = new();

IFiniteElementMesh startMesh = fileManager.ReadMeshFromTelma(@"квадруполь_грубая_новая.txt");

Dictionary<string, IMaterial> materials = new()
{
    ["air"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0),
    ["JMinus"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -1),
    ["JPlus"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 1),
    ["steel"] = new Material(true, false, false, x => 0.01, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0),
    ["Zero tangent"] = new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0.0, (x, t) => 0)
};

EllipticalProblem problem = new(materials, startMesh);

problem.Prepare();
Console.WriteLine(problem.Solve());

fileManager = new("verticesBeforeAddaptation.txt",
                  "trianglesBeforeAddaptation.txt",
                  "valuesBeforeAddaptation.txt");

fileManager.LoadToFile(startMesh.Vertex, startMesh.Elements, [.. problem.Solution.SolutionVector]);

double[] xFlowValues = new double[startMesh.Vertex.Length];
double[] yFlowValues = new double[startMesh.Vertex.Length];

for (int i = 0; i < startMesh.Vertex.Length; i++)
{
    var flow = problem.Solution.Flow(startMesh.Vertex[i], materials);

    xFlowValues[i] = flow.X;
    yFlowValues[i] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, "dxValuesBeforeAdaptation.txt");
fileManager.LoadValuesToFile(yFlowValues, "dyValuesBeforeAdaptation.txt");

Console.WriteLine($"""

    Base mesh:
    dofs - {startMesh.NumberOfDOFs}
    elements - {startMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

SplitStrategy2DMeshes splitStrategy = new(startMesh.Elements, startMesh.Vertex);
CESDifferenceAverageFlowOnEdge calculationErrorStrategy = new(materials);

Adapter2DMeshes adapter = new(problem, splitStrategy, calculationErrorStrategy);

IFiniteElementMesh adaptedMesh = adapter.Adapt();

EllipticalProblem newProblem = new(materials, adaptedMesh);

newProblem.Prepare();
Console.WriteLine(newProblem.Solve());

fileManager = new FileManager("verticesAfterAddaptation.txt",
                              "trianglesAfterAddaptation.txt",
                              "valuesAfterAddaptation.txt");

fileManager.LoadToFile(adaptedMesh.Vertex, adaptedMesh.Elements, [.. newProblem.Solution.SolutionVector]);

Console.WriteLine($"""
    
    Adapted mesh:
    dofs - {adaptedMesh.NumberOfDOFs}
    elements - {adaptedMesh.Elements.Where(x => x.VertexNumber.Length != 2).Count()}

    """);

xFlowValues = new double[adaptedMesh.Vertex.Length];
yFlowValues = new double[adaptedMesh.Vertex.Length];

for (int i = 0; i < adaptedMesh.Vertex.Length; i++)
{
    var flow = newProblem.Solution.Flow(adaptedMesh.Vertex[i], materials);

    xFlowValues[i] = flow.X;
    yFlowValues[i] = flow.Y;
}

fileManager.LoadValuesToFile(xFlowValues, "dxValuesAfterAdaptation.txt");
fileManager.LoadValuesToFile(yFlowValues, "dyValuesAfterAdaptation.txt");

