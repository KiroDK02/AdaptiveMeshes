using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.FEM;
using DataTransferObjects;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.MaterialViewModels;

public partial class MaterialsViewModel : ObservableObject
{
    public ObservableCollection<MaterialViewModel> Materials { get; } = [];
    
    [ObservableProperty] private MaterialViewModel? selectedMaterial;
    
    public async Task<IDictionary<string, IMaterial>> BuildMaterialsAsync(IScriptCompiler compiler)
    {
        var materials = new Dictionary<string, IMaterial>();
        
        var materialTasks = Materials
            .Select(mat => (Name: mat.Name, Task: mat.BuildMaterialAsync(compiler)))
            .ToList();
        
        await Task.WhenAll(materialTasks.Select(materialTask => materialTask.Task));

        return materialTasks.ToDictionary(mat => mat.Name, mat => mat.Task.Result);
    }

    public void LoadFromDto(IEnumerable<MaterialDto> materialDtos)
    {
        Materials.Clear();
        foreach (var materialDto in materialDtos)
            Materials.Add(MaterialViewModel.FromDto(materialDto));
    }
    
    [RelayCommand]
    private void AddMaterial() =>
        Materials.Add(
            new MaterialViewModel()
            {
                Name = $"Material{Materials.Count + 1}",
                SelectedType = MaterialType.Volume
            });

    [RelayCommand]
    private void RemoveMaterial()
    {
        if (SelectedMaterial is not null)
            Materials.Remove(SelectedMaterial);
    }
}