using AdaptiveMeshes.MasterElements;

namespace AdaptiveMeshes.FiniteElements.Interfaces;

public interface IFiniteElementWithNumericalIntegration<T> : IFiniteElement
{
    IMasterElement<T> MasterElement { get; }
}