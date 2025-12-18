using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Problems;
using Services.MeshLoaders;
using Services.ProblemFactories.Interfaces;
using Services.ScriptCompilers.Interfaces;
using ViewModels.MaterialViewModels;
using Microsoft.Win32;

namespace ViewModels.ProblemViewModels;

public partial class ProblemViewModel : ObservableObject
{
    public MaterialsViewModel Materials { get; } = new();
    
    private readonly MeshLoaderFactory _meshLoaderFactory = MeshLoaderFactory.Instance; 
    
    private readonly IScriptCompiler _compiler;
    private readonly IProblemFactory _problemFactory;

    [ObservableProperty] private string problemName = "default";
    [ObservableProperty] private IProblemFactory.ProblemType selectedProblemType;
    [ObservableProperty] private string meshFilePath = "";
    [ObservableProperty] private IProblem? currentProblem;

    public ProblemViewModel(
        IScriptCompiler compiler,
        IProblemFactory problemFactory)
    {
        _compiler = compiler;
        _problemFactory = problemFactory;
    }

    [RelayCommand]
    private async Task BuildProblemAsync()
    {
        if (string.IsNullOrWhiteSpace(MeshFilePath))
            throw new InvalidOperationException("Mesh file path is required.");
        
        var meshLoader = _meshLoaderFactory.CreateMeshLoader(MeshFilePath);
        var mesh = await meshLoader.LoadMeshAsync(MeshFilePath);
        var materials = await Materials.BuildMaterialsAsync(_compiler);

        CurrentProblem = _problemFactory.CreateProblem(
            SelectedProblemType,
            materials,
            mesh);
        
        CurrentProblem.Prepare();
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
    private void SelectMeshFileToSave()
    {
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
}