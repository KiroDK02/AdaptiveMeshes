namespace AdaptiveMeshes.NumericalIntegration
{
    public class QuadratureNode<T>
    {
        public QuadratureNode(T node, double weight)
        {
            Node = node;
            Weight = weight;
        }

        public T Node { get; }
        public double Weight { get; }
    }

    public class QuadratureNodes<T>
    {
        public QuadratureNodes(QuadratureNode<T>[] nodes, int order)
        {
            Nodes = nodes;
            Order = order;
        }

        public QuadratureNode<T>[] Nodes { get; }
        public int Order { get; }
    }
}
