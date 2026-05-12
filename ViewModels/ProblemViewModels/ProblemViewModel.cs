using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Core.Problems;
using DataTransferObjects;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using ViewModels.MaterialViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using Services.WindowServices;
using ViewModels.AdaptationViewModels;
using ViewModels.PlotViewModels;

namespace ViewModels.ProblemViewModels;

public partial class ProblemViewModel : ObservableObject
{
    public MaterialsViewModel Materials { get; }
    public SolutionPlotViewModel SolutionPlot { get; } = new();
    public SolutionPointsViewModel SolutionPoints { get; set; } = new();
    public MeshViewModel MeshViewModel { get; set; }
    public MeshPlotViewModel MeshPlot { get; }
    public AdaptationViewModel Adaptation { get; set; } = new();
    public ErrorCalculationViewModel ErrorCalculation { get; }
    public ObservableCollection<ProblemViewModel> AllProblems { get; }

    public bool ProblemSolved => CurrentProblem?.Solution is not null;
    
    [ObservableProperty] private string _problemName = "default";
    [ObservableProperty] private ProblemType _selectedProblemType;
    [ObservableProperty] private IProblem? _currentProblem;

    private readonly IScriptCompiler _compiler;
    private readonly IProblemFactory _problemFactory;
    private readonly IWindowService _windowService;

    private IDictionary<string, IMaterial>? _materials;

    public ProblemViewModel(
        MeshPlotViewModel meshPlot,
        IScriptCompiler compiler,
        IProblemFactory problemFactory,
        IWindowService windowService,
        ObservableCollection<ProblemViewModel> allProblems,
        MaterialsViewModel materials,
        MeshViewModel? meshViewModel = null)
    {
        MeshPlot = meshPlot;
        Materials = materials;
        AllProblems = allProblems;
        _compiler = compiler;
        _problemFactory = problemFactory;
        _windowService = windowService;

        MeshViewModel = meshViewModel ?? new(MeshPlot, Materials);
        ErrorCalculation = new(AllProblems, compiler);
    }

    public ProblemDto ToProblemDto() => new()
    {
        ProblemName = this.ProblemName,
        SelectedProblemType = this.SelectedProblemType,
        
        Materials = [..Materials.Materials.Select(m => m.ToMaterialDto())],
        
        Points = [..SolutionPoints.Points.Select(p => p.ToPointDto())],
        
        AdaptationDto = Adaptation.ToAdaptationDto(),
        MeshDto = MeshViewModel.ToMeshDto()
    };
    
    [RelayCommand]
    private async Task BuildProblemAsync()
    {
        if (MeshViewModel.Mesh is null)
            return;
        
        _materials = await Materials.BuildMaterialsAsync(_compiler);

        CurrentProblem = _problemFactory.CreateProblem(
            SelectedProblemType,
            _materials,
            MeshViewModel.Mesh);

        CurrentProblem.Prepare();
    }

    [RelayCommand]
    private async Task SolveProblemAsync()
    {
        if (CurrentProblem is not null && ProblemSolved)
            return;
        
        if (CurrentProblem is null)
            await BuildProblemAsync();
        
        CurrentProblem?.Solve();
        OnPropertyChanged(nameof(ProblemSolved));
        
        SolutionPlot.SetSolution(CurrentProblem?.Solution!);
        SolutionPoints.SetSolution(CurrentProblem?.Solution!);
        ErrorCalculation.SetCurrentSolution(CurrentProblem?.Solution!);
    }

    [RelayCommand]
    private void ResetSolution()
    {
        if (CurrentProblem is null)
            return;
        
        CurrentProblem.Solution = null;
        OnPropertyChanged(nameof(ProblemSolved));
    }

    [RelayCommand]
    private async Task AdaptMeshAsync()
    {
        await SolveProblemAsync();
        
        var adaptedMesh = await Adaptation.ExecuteAdaptationAsync(CurrentProblem!);

        var directoryName = Path.GetDirectoryName(MeshViewModel.MeshFilePath);
        var fileName = Path.GetFileNameWithoutExtension(MeshViewModel.MeshFilePath);
        var extension = Path.GetExtension(MeshViewModel.MeshFilePath);

        var newMeshFile =
            $@"{directoryName}\{fileName}{Adaptation.AdaptationType}{Adaptation.CalculationErrorStrategy}{extension}";
        
        var newProblemVm = new ProblemViewModel(
            MeshPlot,
            _compiler,
            _problemFactory,
            _windowService,
            AllProblems,
            Materials)
        {
            SelectedProblemType = this.SelectedProblemType,
            ProblemName = $"{ProblemName} - Adapted",
            MeshViewModel = new(MeshPlot, Materials)
            {
                MeshFilePath = newMeshFile,
                LoaderType = this.MeshViewModel.LoaderType,
                Mesh = adaptedMesh
            },
        };
        
        AllProblems.Add(newProblemVm);
    }

    [RelayCommand]
    private async Task DrawSolutionAsync()
    {
        if (CurrentProblem is null || !CurrentProblem.Solved)
            return;

        _windowService.ShowSolutionWindow(SolutionPlot);

        await Task.Run(SolutionPlot.DrawSolution);
    }

    [RelayCommand]
    private async Task SaveProblemToFileAsync()
    {
        var saveFileDialog = new SaveFileDialog()
        {
            Title = "Сохранить проблему",
            Filter = "JSON файл (*.json)|*.json"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;

        var problemDto = ToProblemDto();
        var json = JsonConvert.SerializeObject(problemDto, Formatting.Indented);
        await File.WriteAllTextAsync(saveFileDialog.FileName, json);
    }
}

public static class ProblemTypeHelper
{
    public static ProblemType[] Values { get; } = Enum.GetValues<ProblemType>();
}