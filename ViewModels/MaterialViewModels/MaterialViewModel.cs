using System;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using Core.Vectors;
using DataTransferObjects;
using ScottPlot;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.MaterialViewModels;

public partial class MaterialViewModel : ObservableObject
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private MaterialType selectedType;
    [ObservableProperty] private Color selectedColor = Color.Gray(100);
    
    [ObservableProperty] private string lambdaBody = "0";
    [ObservableProperty] private string sigmaBody = "0";
    [ObservableProperty] private string fBody = "0";
    [ObservableProperty] private string ugBody = "0";
    [ObservableProperty] private string thettaBody = "0";

    public bool IsVolume => SelectedType is MaterialType.Volume;
    public bool Is1 => SelectedType is MaterialType.FirstBoundary;
    public bool Is2 => SelectedType is MaterialType.SecondBoundary;

    public async Task<IMaterial> BuildMaterialAsync(IScriptCompiler compiler)
    {
        Func<Vector2D, double> lambda = _ => 0;
        Func<Vector2D, double> sigma = _ => 0;
        Func<Vector2D, double, double> f = (_, _) => 0;
        Func<Vector2D, double, double> ug = (_, _) => 0;
        Func<Vector2D, double, double> thetta = (_, _) => 0;

        if (IsVolume)
        {
            lambda = await compiler.CompileStationaryFunction(LambdaBody);
            sigma = await compiler.CompileStationaryFunction(SigmaBody);
            f = await compiler.CompileNonStationaryFunction(FBody);
        }
        else if (Is1)
            ug = await compiler.CompileNonStationaryFunction(UgBody);
        else if (Is2)
            thetta = await compiler.CompileNonStationaryFunction(ThettaBody);

        return new Material(IsVolume, Is1, Is2,
            lambda, sigma, ug, thetta, f);
    }

    public MaterialDto ToMaterialDto() => new()
    {
        Name = this.Name,
        
        MaterialType = this.SelectedType,
        
        LambdaBody = this.LambdaBody,
        SigmaBody = this.SigmaBody,
        FBody = this.FBody,
        UgBody = this.UgBody,
        ThettaBody = this.ThettaBody,
        
        ARGB = SelectedColor.ARGB
    };

    public static MaterialViewModel FromDto(MaterialDto materialDto)
    {
        return new ()
        {
            Name = materialDto.Name,
            SelectedColor = new Color(materialDto.ARGB),
            SelectedType = materialDto.MaterialType,
            
            LambdaBody = materialDto.LambdaBody,
            SigmaBody = materialDto.SigmaBody,
            FBody = materialDto.FBody,
            UgBody = materialDto.UgBody,
            ThettaBody = materialDto.ThettaBody
        };
    }

    partial void OnSelectedTypeChanged(MaterialType value)
    {
        OnPropertyChanged(nameof(IsVolume));
        OnPropertyChanged(nameof(Is1));
        OnPropertyChanged(nameof(Is2));
    }
}

public static class MaterialTypeHelper
{
    public static MaterialType[] Values { get; } = Enum.GetValues<MaterialType>();
}