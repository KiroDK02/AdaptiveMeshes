using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Solution.Interfaces;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.ProblemViewModels;

public partial class ErrorCalculationViewModel : ObservableObject
{
    [ObservableProperty] private double _errorValue = double.NaN;
    [ObservableProperty] private ErrorType _errorType;
    [ObservableProperty] private string _realSolution = "0";
    [ObservableProperty] private ProblemViewModel? _selectedOtherProblem;

    public bool IsErrorValueNaN => double.IsNaN(ErrorValue);
    public ObservableCollection<ProblemViewModel> AllProblems { get; }

    private IScriptCompiler _compiler;
    private ISolution? _currentSolution;

    public ErrorCalculationViewModel(
        ObservableCollection<ProblemViewModel> allProblems,
        IScriptCompiler compiler)
    {
        AllProblems = allProblems;
        _compiler = compiler;
    }

    public void SetCurrentSolution(ISolution solution) => _currentSolution = solution;

    [RelayCommand]
    private async Task CalculateErrorFromOtherSolution()
    {
        if (!IsErrorValueNaN
            || _currentSolution is null
            || SelectedOtherProblem is null
            || !SelectedOtherProblem.ProblemSolved)
            return;

        ErrorValue = await Task.Run(() =>
            _currentSolution.CalcErrorFrom(SelectedOtherProblem!.CurrentProblem!.Solution.Value));
    }

    [RelayCommand]
    private async Task CalculateErrorFromRealSolutionAsync()
    {
        if (!IsErrorValueNaN
            || _currentSolution is null)
            return;

        var realFunc = await _compiler.CompileStationaryFunction(RealSolution);
        ErrorValue = _currentSolution.CalcErrorFrom(realFunc);
    }

    [RelayCommand]
    private void ResetErrorValue() => ErrorValue = double.NaN;

    partial void OnSelectedOtherProblemChanged(ProblemViewModel? value) => ResetErrorValue();
}

public enum ErrorType
{
    [Description("От искомого решения")] FromDesiredSolution,
    [Description("От другого решения")] FromOtherSolution
}

public static class ErrorTypeHelper
{
    public static ErrorType[] Values { get; } = Enum.GetValues<ErrorType>();

    public static string GetDescription(ErrorType errorType) =>
        errorType
            .GetType()
            .GetField(errorType.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description
        ?? errorType.ToString();
}