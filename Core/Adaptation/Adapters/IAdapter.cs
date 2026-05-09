using Core.FEM;

namespace Core.Adaptation.Adapters;

public interface IAdapter
{
    IFiniteElementMesh Adapt();
}

public enum AdaptationType
{
    HAdaptation,
    PAdaptation
}