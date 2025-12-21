using System.IO;
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
    
    public IFiniteElementMesh? CurrentMesh { get; set; }

    [ObservableProperty] private string problemName = "default";
    [ObservableProperty] private ProblemType selectedProblemType;
    [ObservableProperty] private string meshFilePath = "";
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
            IFiniteElementMesh?> _addNewProblem;
    private IDictionary<string, IMaterial>? _materials;
    
    public ProblemViewModel(
        MeshPlotViewModel meshPlot,
        IScriptCompiler compiler,
        IProblemFactory problemFactory,
        IWindowService windowService,
        Action<MaterialsViewModel, ProblemType, string, IFiniteElementMesh?> addNewProblem)
    {
        _meshPlot = meshPlot;
        _compiler = compiler;
        _problemFactory = problemFactory;
        _windowService = windowService;
        _addNewProblem  = addNewProblem;
    }

    [RelayCommand]
    private async Task BuildProblemAsync()
    {
        if (!File.Exists(MeshFilePath))
            throw new InvalidOperationException("Mesh file path is required.");

        await LoadMesh();
        _materials = await Materials.BuildMaterialsAsync(_compiler);

        CurrentProblem = _problemFactory.CreateProblem(
            SelectedProblemType,
            _materials,
            CurrentMesh!);

        CurrentProblem.Prepare();
    }

    [RelayCommand]
    private void SolveProblem()
    {
        if (CurrentProblem is null || CurrentProblem.Solved)
            return;

        CurrentProblem.Solve();
        SolutionPlot.SetSolution(CurrentProblem.Solution);
    }

    [RelayCommand]
    private void AdaptMesh()
    {
        if (CurrentProblem is null)
            return;

        var mesh = CurrentProblem.Mesh;
        SolveProblem();

        var splitStrategy = new SplitStrategy2DMeshes(mesh.Elements, mesh.Vertex);
        var calculatingErrorStrategy = new CesDifferenceAverageFlowOnEdge(CurrentProblem.Materials);
        var adapter = new Adapter2DMeshes(CurrentProblem, splitStrategy,  calculatingErrorStrategy);
        var adaptedMesh = adapter.Adapt();
        
        _addNewProblem(Materials, SelectedProblemType, $"{ProblemName} - Adapted", adaptedMesh);
    }

    [RelayCommand]
    private void DrawMesh()
    {
        if (CurrentMesh is null)
            return;

        _meshPlot.DrawMesh(CurrentMesh, Materials.Materials);
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
    private async Task LoadMesh()
    {
        if (!File.Exists(MeshFilePath))
            return;
        
        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        CurrentMesh = await meshLoader.LoadMeshAsync(MeshFilePath);
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
        if (string.IsNullOrEmpty(MeshFilePath))
            return;
        
        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        meshLoader.SaveMeshToFileAsync(CurrentProblem?.Mesh!, MeshFilePath);
    }
    
    [RelayCommand]
    private void SelectMeshFileToSave()
    {
        // TODO: вынести это через трансфер тоже?
        var saveFileDialog = new SaveFileDialog()
        {
            Title = "Выберите файл для сохранения сетки",
            Filter = "Текстовый файл (*.txt)|*.txt"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var fileName = saveFileDialog.FileName;
            var meshLoader = _meshLoaderFactory.CreateMeshLoader(fileName);

            meshLoader.SaveMeshToFileAsync(CurrentProblem?.Mesh!, fileName);
        }
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