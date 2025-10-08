using AdaptiveMeshes;
using AdaptiveMeshes.Adaptation.Adapters;
using AdaptiveMeshes.Adaptation.Adapters.Adapters2DMeshes;
using AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;
using AdaptiveMeshes.Adaptation.SplitStrategies.SplitStrategies2DMeshes;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FEM.Materials;
using AdaptiveMeshes.FEM.Materials.GeometryMaterials;
using AdaptiveMeshes.FEM.Mesh;
using AdaptiveMeshes.FEM.Mesh.MeshGenerator;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.FiniteElements1D;
using AdaptiveMeshes.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using AdaptiveMeshes.Problems;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Splines;
using AdaptiveMeshes.Splines.Splines1D;
using AdaptiveMeshes.Vectors;
using System.Net.Http.Headers;

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

Console.WriteLine("Hello, World!");

/*FileManager fileManager = new();

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
fileManager.LoadValuesToFile(yFlowValues, "dyValuesAfterAdaptation.txt");*/


// ****************************************************************************
//                            0                     1                  2                   3                  4
/*Vector2D[] points = [new Vector2D(1, 1), new Vector2D(5, 1), new Vector2D(1, 6), new Vector2D(5, 6), new Vector2D(3, 3.5)];

IFiniteElement[] elems = [new TriangleFEQuadraticBaseWithNI("scalarVolume", [0, 1, 4]),
            new TriangleFEQuadraticBaseWithNI("scalarVolume", [1, 3, 4]),
new TriangleFEQuadraticBaseWithNI("scalarVolume", [3, 2, 4]),
new TriangleFEQuadraticBaseWithNI("scalarVolume", [2, 0, 4]),
new SegmentFEQuadraticBaseWithNI("firstBoundary", [0, 2]),
new SegmentFEQuadraticBaseWithNI("secondBoundary", [0, 1]),
new SegmentFEQuadraticBaseWithNI("thirdBoundary1", [1, 3]),
new SegmentFEQuadraticBaseWithNI("thirdBoundary2", [2, 3])];

Dictionary<string, IMaterial> materials = new()
{
    ["scalarVolume"] = new Material(true, false, false, x => 10.0, x => 5.0, (x, t) => 0, (x, t) => 0, (x, t) => 5.0 * (x.X + x.Y + x.X * x.Y)),
    ["firstBoundary"] = new Material(false, true, false, x => 10.0, x => 5.0, (x, t) => 1.0 + 2.0 * x.Y, (x, t) => 0, (x, t) => 0.0),
    ["secondBoundary"] = new Material(false, true, false, x => 10.0, x => 5.0, (x, t) => 2.0 * x.X + 1.0, (x, t) => 0, (x, t) => 0.0),
    ["thirdBoundary1"] = new Material(false, true, false, x => 10.0, x => 5.0, (x, t) => 5.0 + 6.0 * x.Y, (x, t) => 0, (x, t) => 0),
    ["thirdBoundary2"] = new Material(false, false, true, x => 10.0, x => 5.0, (x, t) => 0.0, (x, t) => 10.0 * (1.0 + x.X), (x, t) => 0)
};

// (0, 1) * (1 + y, 1 + x) = 1 + x

FiniteElementMesh mesh = new(elems, points);

EllipticalProblem ellipticalProblem = new(materials, mesh);

ellipticalProblem.Prepare();
Console.WriteLine(ellipticalProblem.Solve());

Console.WriteLine();

Console.WriteLine(ellipticalProblem.Solution.Value(new(3, 5)));
Console.WriteLine(ellipticalProblem.Solution.Gradient(new(3, 5)));*/


/*double[] points = [1, 2, 3, 4, 5, 6, 7, 8];
double[] valuesFuncAtPoints = [1, 8, 27, 64, 125, 216, 343, 512];*/

/*double[] points = [1, 2, 3, 4];
double[] valuesFuncAtPoints = [1, 8, 27, 64];

double alpha = 1e-5;
double betta = 1e-3;

Vector2D[] vertices = [new(1, 0), new(3, 0), new(7, 0), new(9, 0)];

IFiniteElement[] elements = [new SegmentFECubicHermitianBase("", [0, 1]), new SegmentFECubicHermitianBase("", [1, 2]),
                             new SegmentFECubicHermitianBase("", [2, 3])];

IFiniteElementMesh mesh = new FiniteElementMesh(elements, vertices);

ISmoothingSpline<double> spline = new CubicHermitianSmoothingSpline1D(points, mesh, alpha, betta);

spline.BuildSpline(valuesFuncAtPoints);

Console.WriteLine(spline.Value(2.5));*/

/*Dictionary<string, IMaterial> materials = [];

materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0.0, (x, t) => 0.0, (x, t) => -4 + x.X * x.X + x.Y * x.Y));
materials.Add("1", new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("2", new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + 4, (x, t) => 0.0, (x, t) => 0));
materials.Add("3", new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("4", new Material(false, false, true, x => 1, x => 1, (x, t) => 0.0, (x, t) => -2.0, (x, t) => 0));

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

// *************************************************************************************************************
// FOR SPLINES

/*FileManager fileManager = new();
IFiniteElementMesh startMesh = fileManager.ReadMeshFromTelma(@"квадруполь_грубая_новая.txt");

Dictionary<string, IMaterial> materials = new()
{
    ["air"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0),
    ["JMinus"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => -1),
    ["JPlus"] = new Material(true, false, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 1),
    ["steel"] = new Material(true, false, false, x => 0.01, x => 0, (x, t) => 0, (x, t) => 0, (x, t) => 0),
    ["Zero tangent"] = new Material(false, true, false, x => 1, x => 0, (x, t) => 0, (x, t) => 0.0, (x, t) => 0)
};

EllipticalProblem ellipticalProblem = new(materials, startMesh);

ellipticalProblem.Prepare();
Console.WriteLine(ellipticalProblem.Solve());

double y = 0.1;

double[] x = AlgorithmsForSplines.GetPointsForCalculating(0.01, 0.4, 10001);
double[] values = [.. AlgorithmsForSplines.GradientsAtPoints(ellipticalProblem.Solution, x, y).Select(point => point.X)];
var splineMesh = AlgorithmsForSplines.GetMesh(0.01, 0.4, 33);

*//*Func<double, double> func = x => x * x * x;

double[] x = AlgorithmsForSplines.GetPointsForCalculating(-3, -1, 10);
double[] values = new double[x.Length];
for (int i = 0; i < x.Length; i++)
    values[i] = func(x[i]);
var splineMesh = AlgorithmsForSplines.GetMesh(-3, 3, 3);*//*

double alpha = 0;
double betta = 0;

ISmoothingSpline<double> spline = new CubicHermitianSmoothingSpline1D(x, splineMesh, alpha, betta);

spline.BuildSpline(values);

double x0 = 0.01;
double x1 = 0.4;
int countX = 1001;
double hx = (x1 - x0) / (countX - 1);

double[] points = new double[countX];

points[0] = x0;
for (int i = 1; i < countX - 1; i++)
    points[i] = x0 + i * hx;
points[^1] = x1;

//Console.WriteLine($"||u - u*||/||u*|| = {AlgorithmsForSplines.CalcApproximateL2NormDifference(func, spline.Value, points)}");

double[] dxBeforeSmoothing = new double[countX];
double[] dxAfterSmoothing = new double[countX];

for (int i = 0; i < countX; i++)
{
        dxBeforeSmoothing[i] = ellipticalProblem.Solution.Gradient(new(points[i], y)).X;
  //  dxBeforeSmoothing[i] = func(points[i]);
    dxAfterSmoothing[i] = spline.Value(points[i]);
//    dxAfterSmoothing[i] = spline.Value(points[i]);
}

using (StreamWriter writer = new(@"C:\Учёба\НММ\data.txt"))
{
    for (int i = 0; i < points.Length; i++)
        writer.WriteLine($"{points[i]} {dxBeforeSmoothing[i]} {dxAfterSmoothing[i]}");
}*/

// SPLINES END
// *************************************************************************************************************

// *************************************************************************************************************
// потестить отсечение ушей
//                        0           1         2           3           4           5
Vector2D[] vertices = [new(1, 1), new(3, 2), new(2, 4), new(1, 3), new(2.5, 4), new(3, 5)];

IGeometryMaterial[] gMS = [new PolygonGeometryMaterial(vertices, "material", [0, 1, 2, 3]),
                           new PolygonGeometryMaterial(vertices, "material", [1, 4, 5, 2]),
                           new StraightGeometryMaterial(vertices, "boundary", [0, 1]),
                           new StraightGeometryMaterial(vertices, "boundary", [1, 4]),
                           new StraightGeometryMaterial(vertices, "boundary", [4, 5]),
                           new StraightGeometryMaterial(vertices, "boundary", [5, 2]),
                           new StraightGeometryMaterial(vertices, "boundary", [2, 3]),
                           new StraightGeometryMaterial(vertices, "boundary", [3, 0]),
];

Dictionary<(int i, int j), EdgesData> _edgesData = new()
{

};

TriangleMeshGenerator meshGenerator = new(gMS, vertices, 1, 2);

var mesh = meshGenerator.GenerateMesh();

foreach (var element in mesh.Elements)
{
    Console.Write(element.Material);
    for (int i = 0; i < element.VertexNumber.Length; i++)
        Console.Write($" {element.VertexNumber[i]} ");
    Console.WriteLine();
}

using (StreamWriter sw = new("nodes.txt"))
{
    foreach (var vertex in mesh.Vertex)
        sw.WriteLine($"{vertex.X} {vertex.Y}");
}

using (StreamWriter sw = new("triangles.txt"))
{
    foreach (var element in mesh.Elements)
        if (element.VertexNumber.Length != 2)
            sw.WriteLine($"{element.VertexNumber[0]} {element.VertexNumber[1]} {element.VertexNumber[2]}");
}

// конец тестов
// *************************************************************************************************************