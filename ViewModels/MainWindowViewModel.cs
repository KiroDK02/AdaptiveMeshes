using CommunityToolkit.Mvvm.ComponentModel;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using Services.WindowServices;
using ViewModels.PlotViewModels;
using ViewModels.ProblemViewModels;

namespace ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ProblemsEditorViewModel ProblemsEditorViewModel { get; }
    public MeshPlotViewModel MeshPlotViewModel { get; }

    public MainWindowViewModel(
        IWindowService windowService,
        IScriptCompiler scriptCompiler,
        IProblemFactory problemFactory)
    {
        MeshPlotViewModel = new();
        ProblemsEditorViewModel = new(MeshPlotViewModel, scriptCompiler, problemFactory, windowService);
    }
}