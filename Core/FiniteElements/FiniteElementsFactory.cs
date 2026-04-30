using Core.FiniteElements.FiniteElements1D;
using Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using Core.FiniteElements.Interfaces;

namespace Core.FiniteElements;

public static class FiniteElementsFactory
{
    public static IFiniteElement CreateElement(IFiniteElement element, params object[] args)
    {
        return element.FunctionsType switch
        {
            IFiniteElement.BasicFunctionsTypeEnum.Lagrange =>
                CreateLagrangeElement(element, (int[])args[0]),
            
            // TODO: реализовать
            IFiniteElement.BasicFunctionsTypeEnum.Hierarchical => throw new NotImplementedException(),
            
            _ => throw new ArgumentException("Unknown functions type.")
        };
    }

    private static IFiniteElement CreateLagrangeElement(IFiniteElement element, int[] vertexNumbers)
    {
        return element switch
        {
            TriangleFiniteElementQuadraticLagrange =>
                new TriangleFiniteElementQuadraticLagrange(element.Material, vertexNumbers),
            
            SegmentFiniteElementQuadraticLagrange => 
                new SegmentFiniteElementQuadraticLagrange(element.Material, vertexNumbers),
            
            _ => throw new ArgumentException("Unknown element type.")
        };
    }
}