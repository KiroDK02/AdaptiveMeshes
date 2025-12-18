namespace Core.NumericalIntegration;

public class QuadratureNode<T>
{
    public T Node { get; }
    public double Weight { get; }
    
    public QuadratureNode(T node, double weight)
    {
        Node = node;
        Weight = weight;
    }
}

public class QuadratureNodes<T>
{
    public QuadratureNode<T>[] Nodes { get; }
    public int Order { get; }
    
    public QuadratureNodes(QuadratureNode<T>[] nodes, int order)
    {
        Nodes = nodes;
        Order = order;
    }
}