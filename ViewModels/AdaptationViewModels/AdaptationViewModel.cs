using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Adaptation.Adapters;
using Core.Adaptation.Adapters.Adapters2DMeshes;
using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;
using Core.Adaptation.DistributionStrategies;
using Core.FEM;
using Core.Problems;
using DataTransferObjects;

namespace ViewModels.AdaptationViewModels;

public partial class AdaptationViewModel : ObservableObject
{
    public DistributionScalesViewModel DistributionScalesViewModel { get; } = new();

    public DistributionScaleViewModel? ActiveScale => DistributionScalesViewModel.ActiveScale;
    
    [ObservableProperty] private CalculationErrorStrategyType _calculationErrorStrategy = 
        CalculationErrorStrategyType.AverageFlowDifference;

    [ObservableProperty] private AdaptationType _adaptationType = AdaptationType.HAdaptation;
    
    public async Task<IFiniteElementMesh> ExecuteAdaptationAsync(IProblem problem)
    {
        var mesh = problem.Mesh;

        var calculatingErrorStrategy = CalculationErrorStrategyFactory
            .GetCalculationErrorStrategy(CalculationErrorStrategy, problem.Materials);
        
        var distributionStrategy = ActiveScale is null
            ? DistributionStrategyFactory.GetDistributionStrategy(
                AdaptationType,
                problem)
            : DistributionStrategyFactory.GetDistributionStrategy(
                AdaptationType,
                problem,
                [..ActiveScale.DistanceFromMinParts],
                [..ActiveScale.ScaleDistribution]);
        
        var adapter = AdaptersFactory
            .GetAdapter(
                AdaptationType, 
                problem,
                distributionStrategy, 
                calculatingErrorStrategy);
        
        return await Task.Run(adapter.Adapt);
    }

    public AdaptationDto ToAdaptationDto() => new()
    {
        CesType = CalculationErrorStrategy,
        AdaptationType = this.AdaptationType,
        
        Scales = [..DistributionScalesViewModel.Scales.Select(s => s.ToScaleDto())]
    };

    public void LoadFromDto(AdaptationDto adaptationDto)
    {
        CalculationErrorStrategy = adaptationDto.CesType;
        AdaptationType = adaptationDto.AdaptationType;

        DistributionScalesViewModel.LoadFromDto(adaptationDto.Scales);
    }
}

public static class AdaptationTypeHelper
{
    public static AdaptationType[] Values { get; } = Enum.GetValues<AdaptationType>();
}

public static class CalculationErrorStrategyTypeHelper
{
    public static CalculationErrorStrategyType[] Values { get; } = Enum.GetValues<CalculationErrorStrategyType>();

    public static string GetDescription(CalculationErrorStrategyType type) =>
        type
            .GetType()
            .GetField(type.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description ?? type.ToString();
}