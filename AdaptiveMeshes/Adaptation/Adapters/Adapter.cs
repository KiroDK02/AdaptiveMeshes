using AdaptiveMeshes.Adaptation.StrategiesOfCalculationError;
using AdaptiveMeshes.Adaptation.StrategiesOfSplit;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution;

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

            _numberOccurencesOfEdges = AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(Mesh.Elements);
        }

        private IDictionary<(int i, int j), int> _numberOccurencesOfEdges { get; }

        public IFiniteElementMesh Mesh { get; }
        public ISolution Solution { get; }

        /// <value>
        /// Свойство <c>StrategyOfSplit</c> является стратегией разбиения - шкала + методы для расчета разбиений
        /// </value>
        public IStrategyOfSplit StrategyOfSplit { get; }

        /// <value>
        /// Свойство <c>StrategyOfCalculationError</c> является стратегией расчета на ребрах локальных ошибок решения - скачков потока + метод(ы) для их расчета
        /// </value>
        public IStrategyOfCalculationError StrategyOfCalculationError { get; }

        public IFiniteElementMesh Adapt()
        {
            throw new NotImplementedException();
        }
    }
}
