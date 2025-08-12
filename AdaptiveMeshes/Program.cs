using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.Problems;
using AdaptiveMeshes.Vectors;

Console.WriteLine("Hello, World!");

Dictionary<string, IMaterial> materials = [];

materials.Add("volume", new Material(true, false, false, x => 1, x => 1, (x, t) => 0.0,                   (x, t) => 0.0, (x, t) => -4 + x.X * x.X + x.Y * x.Y));
materials.Add("1",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("2",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + 4,         (x, t) => 0.0, (x, t) => 0));
materials.Add("3",      new Material(false, true, false, x => 1, x => 1, (x, t) => x.X * x.X + x.Y * x.Y, (x, t) => 0.0, (x, t) => 0));
materials.Add("4",      new Material(false, false, true, x => 1, x => 1, (x, t) => 0.0,                   (x, t) =>-2.0, (x, t) => 0));

Vector2D[] vertex = [new(1, 1), new(3, 1), new(2, 2), new(4, 2)];

IEnumerable<IFiniteElement> elements = [new TriangleFEQuadraticBaseWithNI("volume", [0, 1, 3]), new TriangleFEQuadraticBaseWithNI("volume", [0, 3, 2]),
                                        new TriangleFEStraightQuadraticBaseWithNI("1", [0, 2]), new TriangleFEStraightQuadraticBaseWithNI("2", [2, 3]),
                                        new TriangleFEStraightQuadraticBaseWithNI("3", [3, 1]), new TriangleFEStraightQuadraticBaseWithNI("4", [0, 1])];

FiniteElementMesh mesh = new(elements, vertex);

EllipticalProblem ellipticalProblem = new(materials, mesh);

ellipticalProblem.Prepare();
Console.WriteLine(ellipticalProblem.Solve());

Console.WriteLine(ellipticalProblem.Solution.Value(new(1.5, 1.5)));
Console.WriteLine(ellipticalProblem.Solution.Gradient(new(3, 1.5)));
Console.WriteLine(ellipticalProblem.Solution.Flow(new(3, 1.5), materials));