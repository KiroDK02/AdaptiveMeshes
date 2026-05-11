using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Core.Problems;
using DataTransferObjects;
using Services.MeshLoaders;
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
    public MaterialsViewModel Materials { get; init; } = new();
    public SolutionPlotViewModel SolutionPlot { get; } = new();
    public SolutionPointsViewModel SolutionPoints { get; set; } = new();
    public MeshPlotViewModel MeshPlot { get; }
    public AdaptationViewModel Adaptation { get; set; } = new();
    public IFiniteElementMesh? ProblemMesh { get; set; }
    
    public bool CanShowError => ProblemSolved && IsRealSolutionKnown;

    [ObservableProperty] private string _problemName = "default";
    [ObservableProperty] private ProblemType _selectedProblemType;
    [ObservableProperty] private string _meshFilePath = string.Empty;
    [ObservableProperty] private IProblem? _currentProblem;
    [ObservableProperty] private bool _isRealSolutionKnown;
    [ObservableProperty] private double _errorSolution;
    [ObservableProperty] private string _realSolution = "0";
    [ObservableProperty] private bool _problemSolved = false;

    private string _pathLoadedMesh = string.Empty;
    private bool _meshChanged = false;
    
    private readonly MeshLoaderFactory _meshLoaderFactory = MeshLoaderFactory.Instance;

    private readonly IScriptCompiler _compiler;
    private readonly IProblemFactory _problemFactory;
    private readonly IWindowService _windowService;

    private readonly Action<
        MaterialsViewModel,
        ProblemType,
        string,
        string,
        IFiniteElementMesh?> _addNewProblem;

    private IDictionary<string, IMaterial>? _materials;

    public ProblemViewModel(
        MeshPlotViewModel meshPlot,
        IScriptCompiler compiler,
        IProblemFactory problemFactory,
        IWindowService windowService,
        Action<MaterialsViewModel, ProblemType, string, string, IFiniteElementMesh?> addNewProblem)
    {
        MeshPlot = meshPlot;
        _compiler = compiler;
        _problemFactory = problemFactory;
        _windowService = windowService;
        _addNewProblem = addNewProblem;
    }

    public ProblemDto ToProblemDto() => new()
    {
        ProblemName = this.ProblemName,
        SelectedProblemType = this.SelectedProblemType,
        
        MeshFilePath = this.MeshFilePath,
        Materials = [..Materials.Materials.Select(m => m.ToMaterialDto())],
        
        Points = [..SolutionPoints.Points.Select(p => p.ToPointDto())],
        
        AdaptationDto = Adaptation.ToAdaptationDto(),
        
        IsRealSolutionKnown = this.IsRealSolutionKnown,
        RealSolution = this.RealSolution
    };
    
    [RelayCommand]
    private async Task BuildProblemAsync()
    {
        if (ProblemMesh is null || _meshChanged)
        {
            if (!File.Exists(MeshFilePath))
                throw new InvalidOperationException("Mesh file path is required.");
            
            await LoadMeshAsync();
        }

        _materials = await Materials.BuildMaterialsAsync(_compiler);

        CurrentProblem = _problemFactory.CreateProblem(
            SelectedProblemType,
            _materials,
            ProblemMesh!);

        CurrentProblem.Prepare();
    }

    [RelayCommand]
    private async Task SolveProblemAsync()
    {
        if (CurrentProblem is not null && ProblemSolved && !_meshChanged)
            return;
        
        if (CurrentProblem is null || _meshChanged)
            await BuildProblemAsync();
        
        CurrentProblem?.Solve();
        SolutionPlot.SetSolution(CurrentProblem?.Solution!);

        var otherSolution = SelectedProblemType switch
        {
            ProblemType.EllipticalProblem => await _compiler.CompileStationaryFunction(RealSolution),
            
            _ => throw new ArgumentException("Invalid problem type.")
        };
        
        ErrorSolution = CurrentProblem!.Solution.CalcErrorFrom(otherSolution);
        SolutionPoints.SetSolution(CurrentProblem.Solution);
        ProblemSolved = true;
    }

    [RelayCommand]
    private async Task AdaptMeshAsync()
    {
        await SolveProblemAsync();
        
        var adaptedMesh = await Adaptation.ExecuteAdaptationAsync(CurrentProblem!);

        var directoryName = Path.GetDirectoryName(MeshFilePath);
        var fileName = Path.GetFileNameWithoutExtension(MeshFilePath);
        var extension = Path.GetExtension(MeshFilePath);

        var newMeshFile =
            $@"{directoryName}\{fileName}{Adaptation.AdaptationType}{Adaptation.CalculationErrorStrategy}{extension}";
        
        _addNewProblem(Materials, SelectedProblemType, $"{ProblemName} - Adapted", newMeshFile, adaptedMesh);
    }

    [RelayCommand]
    private void DrawMesh()
    {
        if (ProblemMesh is null)
            return;

        MeshPlot.DrawMesh(ProblemMesh, Materials.Materials);
    }

    [RelayCommand]
    private void DrawSolution()
    {
        if (CurrentProblem is null || !CurrentProblem.Solved)
            return;

        _windowService.ShowSolutionWindow(SolutionPlot);

        SolutionPlot.DrawSolution();
    }

    // Сделать, чтоб сетка грузилась при повторном нажатии
    // только если поменялся файл.
    [RelayCommand]
    private async Task LoadMeshAsync()
    {
        if (!File.Exists(MeshFilePath) || MeshFilePath == _pathLoadedMesh)
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        ProblemMesh = await meshLoader.LoadMeshAsync(MeshFilePath);
        _pathLoadedMesh = MeshFilePath;
        
        ProblemSolved = false;
        _meshChanged = false;
    }

    [RelayCommand]
    private void SelectMeshFile()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "Выберите файл сетки",
            Filter = "Текстовый файл сетки (*.txt)|*.txt",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
            MeshFilePath = openFileDialog.FileName;

        _meshChanged = true;
    }

    [RelayCommand]
    private void SaveMesh()
    {
        if (ProblemMesh is null || string.IsNullOrEmpty(MeshFilePath))
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        meshLoader.SaveMeshToFileAsync(ProblemMesh, MeshFilePath);
    }

    [RelayCommand]
    private void SelectMeshFileToSave()
    {
        if (ProblemMesh is null)
            return;

        // TODO: вынести это через трансфер тоже?
        var saveFileDialog = new SaveFileDialog()
        {
            Title = "Выберите файл для сохранения сетки",
            Filter = "Текстовый файл (*.txt)|*.txt"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;
        
        var fileName = saveFileDialog.FileName;
        var meshLoader = _meshLoaderFactory.CreateMeshLoader(fileName);

        MeshFilePath = fileName;

        meshLoader.SaveMeshToFileAsync(ProblemMesh, fileName);
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
    
    partial void OnProblemSolvedChanged(bool value) => OnPropertyChanged(nameof(CanShowError));
    partial void OnIsRealSolutionKnownChanged(bool value) => OnPropertyChanged(nameof(CanShowError));
}

public static class ProblemTypeHelper
{
    public static ProblemType[] Values { get; } = Enum.GetValues<ProblemType>();
}