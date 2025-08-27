using AdaptiveMeshes.Adaptation.StrategiesOfCalculationError;
using AdaptiveMeshes.Adaptation.StrategiesOfSplit;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Adaptation.Adapters
{
    /// <summary>
    /// В этом классе по сути надо будет реализовать один метод - Adapt
    /// Может быть сделать сразу этот класс с заделкой на циклическую адаптацию,
    /// просто добавив количество повторов, адаптация будет происходить через
    /// стратегии расчета ошибок и разбиения, в которых будет реализовано все нужное.
    /// </summary>
    public class Adapter : IAdapter
    {
        public Adapter(IFiniteElementMesh mesh, ISolution solution)
        {
            Mesh = mesh;
            Solution = solution;
        }
        public IFiniteElementMesh Mesh { get; }
        public ISolution Solution { get; }

        /// <value>
        /// Свойство <c>StrategyOfSplit</c> является стратегией разбиения - шкала + методы для расчета разбиений
        /// </value>
        public ISplitStrategy SplitStrategy { get; }

        /// <value>
        /// Свойство <c>StrategyOfCalculationError</c> является стратегией расчета на ребрах локальных ошибок решения - скачков потока + метод(ы) для их расчета
        /// </value>
        public IStrategyOfCalculationError CalculationErrorStrategy { get; }

        public IFiniteElementMesh Adapt()
        {
            IDictionary<(int i, int j), double> errors = CalculationErrorStrategy.ComputeError(Solution);
            IDictionary<(int i, int j), int> splits = SplitStrategy.GetSplits(errors);

            int countVertices = Mesh.Vertex.Length;
            var verticesSplitedEdges = SplitStrategy.CalcVerticesEdges(splits, ref countVertices);

            List<IFiniteElement> newElements = [];
            List<(Vector2D vert, int num)> newVertices = [];

            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length != 2)
                {
                    var datas = element.SplitToElements2D(splits, verticesSplitedEdges, ref countVertices);

                    newElements.AddRange(datas.NewElements);
                    newVertices.AddRange(datas.NewVertices);
                }
                else
                {
                    var elements = element.SplitToElements1D([.. verticesSplitedEdges[element.GlobalEdge(0)].Select(vertex => vertex.num)]);

                    newElements.AddRange(elements);
                }    
            }

            Vector2D[] vertices = new Vector2D[countVertices];

            foreach ((Vector2D vert, int number) in newVertices)
                vertices[number] = vert;

            return new FiniteElementMesh(newElements, vertices);
        }
    }
}
