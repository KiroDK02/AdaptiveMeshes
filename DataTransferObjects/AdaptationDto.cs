using System.Collections.Generic;
using Core.Adaptation.Adapters;
using Core.Adaptation.CalculationErrorStrategies;

namespace DataTransferObjects;

public class AdaptationDto
{
    public CalculationErrorStrategyType CesType { get; init; }
    public AdaptationType AdaptationType { get; init; }

    public List<ScaleDto> Scales { get; init; } = [];
}