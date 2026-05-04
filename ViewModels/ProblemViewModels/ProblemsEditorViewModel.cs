using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Core.Problems;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using Services.WindowServices;
using ViewModels.MaterialViewModels;
using ViewModels.PlotViewModels;

using static Services.ProblemFactories.Interfaces.IProblemFactory;

namespace ViewModels.ProblemViewModels;

public partial class ProblemsEditorViewModel : ObservableObject
{
    public ObservableCollection<ProblemViewModel> Problems { get; } = [];

    [ObservableProperty] private ProblemViewModel? selectedProblem;

    private readonly MeshPlotViewModel _meshPlot;
    private readonly IScriptCompiler _scriptCompiler;
    private readonly IProblemFactory _problemFactory;

    private readonly IWindowService _windowService;

    public ProblemsEditorViewModel(
        MeshPlotViewModel meshPlot,
        IScriptCompiler scriptCompiler,
        IProblemFactory problemFactory,
        IWindowService windowService)
    {
        _meshPlot = meshPlot;
        _scriptCompiler = scriptCompiler;
        _problemFactory = problemFactory;
        _windowService = windowService;
    }

    [RelayCommand]
    private void AddNewProblem() => 
        AddNewProblem(
            new(),
            ProblemType.EllipticalProblem,
            $"Problem{Problems.Count + 1}",
            string.Empty,
            null);

    [RelayCommand]
    private void RemoveProblem(ProblemViewModel problemVm)
    {
        Problems.Remove(problemVm);
        
        if (SelectedProblem == problemVm)
            SelectedProblem = Problems.FirstOrDefault();
    }

    [RelayCommand]
    private void LoadProblemFromFile()
    {
        // TODO: реализовать через DataTransfers
    }

    private void AddNewProblem(
        MaterialsViewModel materials, 
        ProblemType problemType, 
        string problemName,
        string meshFilePath,
        IFiniteElementMesh? mesh)
    {
        var newProblemVm = new ProblemViewModel(
            _meshPlot,
            _scriptCompiler,
            _problemFactory,
            _windowService,
            AddNewProblem)
        {
            Materials = materials,
            SelectedProblemType = problemType,
            ProblemName = problemName,
            MeshFilePath = meshFilePath,
            ProblemMesh = mesh
        };
        
        Problems.Add(newProblemVm);
        SelectedProblem = newProblemVm;
    }
}