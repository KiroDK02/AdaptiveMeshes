using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.FEM;
using Core.Vectors;
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

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[Material]");
        sb.AppendLine($"[MaterialName]:{Name}");
        sb.AppendLine($"[MaterialType]:{(int)SelectedType}");
        sb.AppendLine($"[LambdaBody]:{LambdaBody}");
        sb.AppendLine($"[SigmaBody]:{SigmaBody}");
        sb.AppendLine($"[FBody]:{FBody}");
        sb.AppendLine($"[UgBody]:{UgBody}");
        sb.AppendLine($"[ThettaBody]:{ThettaBody}");

        return sb.ToString();
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

public static class MaterialTypeHelper
{
    public static MaterialType[] Values { get; } = Enum.GetValues<MaterialType>();
}