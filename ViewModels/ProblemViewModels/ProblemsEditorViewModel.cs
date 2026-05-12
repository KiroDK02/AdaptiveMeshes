using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Core.Problems;
using DataTransferObjects;
using Microsoft.Win32;
using Newtonsoft.Json;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using Services.WindowServices;
using ViewModels.AdaptationViewModels;
using ViewModels.MaterialViewModels;
using ViewModels.PlotViewModels;

namespace ViewModels.ProblemViewModels;

public partial class ProblemsEditorViewModel : ObservableObject
{
    public ObservableCollection<ProblemViewModel> Problems { get; } = [];

    [ObservableProperty] private ProblemViewModel? _selectedProblem;

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
            null);
    
    [RelayCommand]
    private void RemoveProblem(ProblemViewModel problemVm)
    {
        Problems.Remove(problemVm);

        if (SelectedProblem == problemVm)
            SelectedProblem = Problems.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadProblemFromFileAsync()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "Загрузить проблему",
            Filter = "JSON файл (*.json)|*.json",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        var json = await File.ReadAllTextAsync(openFileDialog.FileName);
        var problemDto = JsonConvert.DeserializeObject<ProblemDto>(json);

        if (problemDto is null)
            return;

        var materials = new MaterialsViewModel();
        materials.LoadFromDto(problemDto.Materials);

        var solutionPoints = new SolutionPointsViewModel();
        solutionPoints.LoadFromDto(problemDto.Points);

        var adaptation = new AdaptationViewModel();
        adaptation.LoadFromDto(problemDto.AdaptationDto);

        var meshViewModel = new MeshViewModel(_meshPlot, materials);
        meshViewModel.LoadFromDto(problemDto.MeshDto);
        
        AddNewProblem(
            materials,
            problemDto.SelectedProblemType,
            problemDto.ProblemName,
            meshViewModel);

        var addedProblem = Problems.LastOrDefault();

        addedProblem?.Adaptation = adaptation;
        addedProblem?.SolutionPoints = solutionPoints;
    }

    private void AddNewProblem(
        MaterialsViewModel materials,
        ProblemType problemType,
        string problemName,
        MeshViewModel? meshViewModel)
    {
        var newProblemVm = new ProblemViewModel(
            _meshPlot,
            _scriptCompiler,
            _problemFactory,
            _windowService,
            Problems,
            materials,
            meshViewModel)
        {
            SelectedProblemType = problemType,
            ProblemName = problemName
        };

        Problems.Add(newProblemVm);
        SelectedProblem = newProblemVm;
    }
}