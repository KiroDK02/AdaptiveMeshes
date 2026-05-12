using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Microsoft.Win32;
using Services.MeshLoaders;
using System;
using System.IO;
using System.Threading.Tasks;
using ViewModels.MaterialViewModels;
using ViewModels.PlotViewModels;

namespace ViewModels.ProblemViewModels;

public partial class MeshViewModel : ObservableObject
{
    [ObservableProperty] private string _meshFilePath = string.Empty;
    [ObservableProperty] private MeshLoaderType _loaderType;

    public IFiniteElementMesh? Mesh { get; set; }
    public bool MeshChanged { get; set; } = false;

    private string _pathLoadedMesh = string.Empty;

    private readonly MeshLoaderFactory _meshLoaderFactory = MeshLoaderFactory.Instance;

    public readonly MeshPlotViewModel _meshPlot;
    private readonly MaterialsViewModel _materials;

    public MeshViewModel(MeshPlotViewModel meshPlot, MaterialsViewModel materials)
    {
        _meshPlot = meshPlot;
        _materials = materials;
    }

    [RelayCommand]
    private async Task LoadMeshAsync()
    {
        if (!File.Exists(MeshFilePath) || MeshFilePath == _pathLoadedMesh)
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(LoaderType);
        Mesh = await meshLoader.LoadMeshAsync(MeshFilePath);
        _pathLoadedMesh = MeshFilePath;

        MeshChanged = false;
    }

    [RelayCommand]
    private async Task DrawMeshAsync()
    {
        if (Mesh is null)
            return;

        await Task.Run(() => _meshPlot.DrawMesh(Mesh, _materials.Materials));
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

        MeshChanged = true;
    }

    [RelayCommand]
    private void SaveMesh()
    {
        if (Mesh is null || string.IsNullOrEmpty(MeshFilePath))
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(LoaderType);
        meshLoader.SaveMeshToFileAsync(Mesh, MeshFilePath);
    }

    [RelayCommand]
    private void SelectMeshFileToSave()
    {
        if (Mesh is null)
            return;

        var saveFileDialog = new SaveFileDialog()
        {
            Title = "Выберите файл для сохранения сетки",
            Filter = "Текстовый файл (*.txt)|*.txt"
        };

        if (saveFileDialog.ShowDialog() != true)
            return;

        var fileName = saveFileDialog.FileName;
        var meshLoader = _meshLoaderFactory.CreateMeshLoader(LoaderType);

        MeshFilePath = fileName;

        meshLoader.SaveMeshToFileAsync(Mesh, fileName);
    }
}

public static class MeshLoaderTypeHelper
{
    public static MeshLoaderType[] Values { get; } = Enum.GetValues<MeshLoaderType>();
}