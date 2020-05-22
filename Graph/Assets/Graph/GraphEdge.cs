namespace LPE.Graph {
    public class GraphEdge<T> {
        public GraphNode<T> node1 { get; private set; }
        public GraphNode<T> node2{ get; private set; }

        public float weight = 1;
        public bool directed = false;

        public GraphEdge(GraphNode<T> node1, GraphNode<T> node2) {
            this.node1 = node1;
            this.node2 = node2;
        }
    }
}