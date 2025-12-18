using Core.MasterElements;

namespace Core.FiniteElements.Interfaces;

public interface IFiniteElementWithNumericalIntegration<T> : IFiniteElement
{
    IMasterElement<T> MasterElement { get; }
}