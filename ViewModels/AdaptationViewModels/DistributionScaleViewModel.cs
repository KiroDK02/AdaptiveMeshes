using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTransferObjects;

namespace ViewModels.AdaptationViewModels;

public partial class DistributionScaleViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "Новая шкала";

    public ObservableCollection<DistributionSegmentViewModel> Segments { get; } =
        [new(0.0, 0), new(1.0, null)];

    public IEnumerable<double> DistanceFromMinParts => Segments.Select(s => s.DistanceFromMin);

    public IEnumerable<int> ScaleDistribution => Segments
        .Where(s => s.Scale.HasValue)
        .Select(s => s.Scale!.Value);

    public ScaleDto ToScaleDto() => new()
    {
        Name = this.Name,
        
        DistanceFromMinParts = [..this.DistanceFromMinParts],
        ScaleDistribution = [..this.ScaleDistribution]
    };

    public static DistributionScaleViewModel FromScaleDto(ScaleDto scaleDto)
    {
        var scale = new DistributionScaleViewModel();
        
        scale.Name = scaleDto.Name;
        scale.Segments.Clear();
        
        for (int i = 0; i < scaleDto.ScaleDistribution.Count; i++)
            scale.Segments.Add(new(scaleDto.DistanceFromMinParts[i], scaleDto.ScaleDistribution[i]));
        
        scale.Segments.Add(new(scaleDto.DistanceFromMinParts[^1], null));
        
        return scale;
    }
    
    [RelayCommand]
    private void AddNewSegment()
    {
        Segments.Last().Scale = 0;
        Segments.Add(new(1.0, null));
    }

    [RelayCommand]
    private void RemoveLastSegment()
    {
        if (Segments.Count <= 2)
            return;
        
        Segments.RemoveAt(Segments.Count - 1);
    }
}