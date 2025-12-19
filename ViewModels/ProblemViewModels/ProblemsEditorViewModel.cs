using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using Services.WindowServices;

namespace ViewModels.ProblemViewModels;

public partial class ProblemsEditorViewModel : ObservableObject
{
    public ObservableCollection<ProblemViewModel> Problems { get; } = [];

    [ObservableProperty] private ProblemViewModel? selectedProblem;
    
    private readonly IScriptCompiler _scriptCompiler;
    private readonly IProblemFactory _problemFactory;

    private readonly IWindowService _windowService;

    public ProblemsEditorViewModel(
        IScriptCompiler scriptCompiler,
        IProblemFactory problemFactory,
        IWindowService windowService)
    {
        _scriptCompiler = scriptCompiler;
        _problemFactory = problemFactory;
        _windowService = windowService;
    }
    
    [RelayCommand]
    private void AddNewProblem()
    {
        var newProblemVm = new ProblemViewModel(_scriptCompiler, _problemFactory, _windowService)
        {
            ProblemName = $"Problem{Problems.Count + 1}"
        };
        
        Problems.Add(newProblemVm);
        SelectedProblem = newProblemVm;
    }

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
}