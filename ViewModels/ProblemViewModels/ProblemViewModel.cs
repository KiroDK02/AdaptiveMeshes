using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Adaptation.Adapters.Adapters2DMeshes;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;
using Core.Adaptation.SplitStrategies.SplitStrategies2DMeshes;
using Core.FEM;
using Core.Problems;
using Services.MeshLoaders;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using ViewModels.MaterialViewModels;
using Microsoft.Win32;
using Services.WindowServices;
using ViewModels.PlotViewModels;

namespace ViewModels.ProblemViewModels;

public partial class ProblemViewModel : ObservableObject
{
    public MaterialsViewModel Materials { get; init; } = new();
    public SolutionPlotViewModel SolutionPlot { get; } = new();

    public IFiniteElementMesh? ProblemMesh { get; set; }

    [ObservableProperty] private string problemName = "default";
    [ObservableProperty] private ProblemType selectedProblemType;
    [ObservableProperty] private string meshFilePath = string.Empty;
    [ObservableProperty] private IProblem? currentProblem;

    private readonly MeshLoaderFactory _meshLoaderFactory = MeshLoaderFactory.Instance;

    private readonly MeshPlotViewModel _meshPlot;
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
        _meshPlot = meshPlot;
        _compiler = compiler;
        _problemFactory = problemFactory;
        _windowService = windowService;
        _addNewProblem = addNewProblem;
    }

    [RelayCommand]
    private async Task BuildProblemAsync()
    {
        if (ProblemMesh is null)
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
        if (CurrentProblem is not null && CurrentProblem.Solved)
            return;
        
        if (CurrentProblem is null)
            await BuildProblemAsync();
        
        CurrentProblem?.Solve();
        SolutionPlot.SetSolution(CurrentProblem?.Solution!);
    }

    [RelayCommand]
    private async Task AdaptMeshAsync()
    {
        await SolveProblemAsync();
        var mesh = CurrentProblem?.Mesh;

        var splitStrategy = new SplitStrategy2DMeshes(mesh?.Elements!, mesh?.Vertex!);
        var calculatingErrorStrategy = new CesDifferenceAverageFlowOnEdge(CurrentProblem?.Materials!);
        var adapter = new HAdapter2DMeshes(CurrentProblem!, splitStrategy, calculatingErrorStrategy);
        
        var adaptedMesh = await Task.Run(adapter.Adapt);

        var newMeshFile =
            $@"{Path.GetDirectoryName(MeshFilePath)}\{Path.GetFileNameWithoutExtension(MeshFilePath)}Adapted{Path
                .GetExtension(MeshFilePath)}";
        _addNewProblem(Materials, SelectedProblemType, $"{ProblemName} - Adapted", newMeshFile, adaptedMesh);
    }

    [RelayCommand]
    private void DrawMesh()
    {
        if (ProblemMesh is null)
            return;

        _meshPlot.DrawMesh(ProblemMesh, Materials.Materials);
    }

    [RelayCommand]
    private void DrawSolution()
    {
        if (CurrentProblem is null || !CurrentProblem.Solved)
            return;

        _windowService.ShowSolutionWindow(SolutionPlot);

        SolutionPlot.DrawSolution();
    }

    [RelayCommand]
    private async Task LoadMeshAsync()
    {
        if (!File.Exists(MeshFilePath))
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        ProblemMesh = await meshLoader.LoadMeshAsync(MeshFilePath);
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
    private void SaveProblemToFile()
    {
        // TODO: реализовать через DataTransfers
    }
}

public static class ProblemTypeHelper
{
    public static ProblemType[] Values { get; } = Enum.GetValues<ProblemType>();
}