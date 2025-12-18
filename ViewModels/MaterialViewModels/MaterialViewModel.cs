using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using Core.Vectors;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.MaterialViewModels;

public partial class MaterialViewModel : ObservableObject
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private MaterialType selectedType;

    [ObservableProperty] private string lambdaBody;
    [ObservableProperty] private string sigmaBody;
    [ObservableProperty] private string fBody;
    [ObservableProperty] private string ugBody;
    [ObservableProperty] private string thettaBody;
    
    public bool IsVolume => SelectedType is MaterialType.Volume;
    public bool Is1 => SelectedType is MaterialType.FirstBoundary;
    public bool Is2 => SelectedType is MaterialType.SecondBoundary;

    public async Task<IMaterial> BuildMaterialAsync(IScriptCompiler compiler)
    {
        Func<Vector2D, double> lambda = point => 0;
        Func<Vector2D, double> sigma  = point => 0;
        Func<Vector2D, double, double> f = (point, t) => 0;
        Func<Vector2D, double, double> ug = (point, t) => 0;
        Func<Vector2D, double, double> thetta = (point, t) => 0;

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

    partial void OnSelectedTypeChanged(MaterialType value)
    {
        OnPropertyChanged(nameof(IsVolume));
        OnPropertyChanged(nameof(Is1));
        OnPropertyChanged(nameof(Is2));
    }
}

public enum MaterialType
{
    Volume,
    FirstBoundary,
    SecondBoundary
}