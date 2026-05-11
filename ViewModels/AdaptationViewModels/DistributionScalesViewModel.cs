using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferObjects;

namespace ViewModels.AdaptationViewModels;

public partial class DistributionScalesViewModel : ObservableObject
{
    [ObservableProperty] private DistributionScaleViewModel? _activeScale; 
    
    public ObservableCollection<DistributionScaleViewModel> Scales { get; } = [];
    
    public void LoadFromDto(IEnumerable<ScaleDto> scaleDtos)
    {
        Scales.Clear();
        
        foreach (var scaleDto in scaleDtos)
            Scales.Add(DistributionScaleViewModel.FromScaleDto(scaleDto));
        
        ActiveScale = Scales.FirstOrDefault();
    }
    
    [RelayCommand]
    private void AddScale()
    {
        var scale = new DistributionScaleViewModel();
        Scales.Add(scale);
        
        ActiveScale ??= scale;
    }

    [RelayCommand]
    private void RemoveScale(DistributionScaleViewModel scale)
    {
        Scales.Remove(scale);
        
        if (ActiveScale == scale)
            ActiveScale = Scales.FirstOrDefault();
    }
    
    [RelayCommand]
    private void SetActiveScale(DistributionScaleViewModel scale) => ActiveScale = scale;
}

public partial class DistributionSegmentViewModel : ObservableObject
{
    [ObservableProperty] private double _distanceFromMin;
    [ObservableProperty] private int? _scale;

    public DistributionSegmentViewModel(double distanceFromMin, int? scale)
    {
        DistanceFromMin = distanceFromMin;
        Scale = scale;
    }
}