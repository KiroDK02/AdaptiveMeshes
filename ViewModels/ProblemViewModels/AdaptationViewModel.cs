using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Adaptation.Adapters;
using Core.Adaptation.Adapters.Adapters2DMeshes;
using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;
using Core.Adaptation.DistributionStrategies.SplitStrategies2DMeshes;
using Core.FEM;
using Core.Problems;

namespace ViewModels.ProblemViewModels;

public partial class AdaptationViewModel : ObservableObject
{
    [ObservableProperty] private CalculationErrorStrategy calculationErrorStrategyType = 
        CalculationErrorStrategy.StrategyAverageFlowDifference;

    [ObservableProperty] private AdaptationType adaptationType = AdaptationType.HAdaptation;
    
    public async Task<IFiniteElementMesh> ExecuteAdaptation(IProblem problem)
    {
        var mesh = problem.Mesh;

        var calculatingErrorStrategy = CalculationErrorStrategyFactory
            .GetCalculationErrorStrategy(CalculationErrorStrategyType, problem.Materials);
        
        var distributionStrategy = new SplitStrategy2DMeshes(mesh?.Elements!, mesh?.Vertex!);
        
        var adapter = AdaptersFactory
            .GetAdapter(
                AdaptationType, 
                problem,
                distributionStrategy, 
                calculatingErrorStrategy);
        
        return await Task.Run(adapter.Adapt);
    }
}