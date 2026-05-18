using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using Microsoft.Win32;
using Services.MeshLoaders;
using System;
using System.IO;
using System.Threading.Tasks;
using DataTransferObjects;
using ViewModels.MaterialViewModels;
using ViewModels.PlotViewModels;

namespace ViewModels.ProblemViewModels;

public partial class MeshViewModel : ObservableObject
{
    [ObservableProperty] private string _meshFilePath = string.Empty;
    [ObservableProperty] private MeshLoaderType _loaderType = MeshLoaderType.Kiro2D;
    
    public int? NumberDofs => Mesh?.NumberOfDOFs;
    
    public MeshPlotViewModel MeshPlot { get; }
    public IFiniteElementMesh? Mesh { get; set; }
    public bool MeshChanged { get; set; } = false;

    private string _pathLoadedMesh = string.Empty;

    private readonly MeshLoaderFactory _meshLoaderFactory = MeshLoaderFactory.Instance;

    private readonly MaterialsViewModel _materials;

    public MeshViewModel(MeshPlotViewModel meshPlot, MaterialsViewModel materials)
    {
        MeshPlot = meshPlot;
        _materials = materials;
    }

    public MeshDto ToMeshDto() => new()
    {
        MeshFilePath = this.MeshFilePath,
        LoaderType = this.LoaderType
    };
    
    public void NotifyNumberDofsChanged() => OnPropertyChanged(nameof(NumberDofs));
    
    [RelayCommand]
    private async Task LoadMeshAsync()
    {
        if (!File.Exists(MeshFilePath))
            throw new InvalidOperationException("Mesh file path is required.");

        if (Mesh is not null 
            && MeshFilePath == _pathLoadedMesh 
            && !MeshChanged)
            return;

        var meshLoader = _meshLoaderFactory.CreateMeshLoader(LoaderType);
        Mesh = await meshLoader.LoadMeshAsync(MeshFilePath);
        _pathLoadedMesh = MeshFilePath;

        MeshChanged = false;
    }

    public void LoadFromDto(MeshDto meshDto)
    {
        MeshFilePath = meshDto.MeshFilePath;
        LoaderType = meshDto.LoaderType;
    }

    [RelayCommand]
    private async Task DrawMeshAsync()
    {
        if (Mesh is null)
            return;

        await MeshPlot.DrawMeshAsync(Mesh, _materials.Materials);
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
    private async Task SaveMeshAsync()
    {
        if (Mesh is null || string.IsNullOrEmpty(MeshFilePath))
            return;

        var meshLoader = _meshLoaderFactory.MeshLoaderKiro2D;
        await meshLoader.SaveMeshToFileAsync(Mesh, MeshFilePath);
    }

    [RelayCommand]
    private async Task SelectMeshFileToSave()
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
        var meshLoader = _meshLoaderFactory.MeshLoaderKiro2D;

        MeshFilePath = fileName;

        await meshLoader.SaveMeshToFileAsync(Mesh, fileName);
    }
}

public static class MeshLoaderTypeHelper
{
    public static MeshLoaderType[] Values { get; } = Enum.GetValues<MeshLoaderType>();
}