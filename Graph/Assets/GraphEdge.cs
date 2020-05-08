namespace LPE.Graph {
    public class GraphEdge<T> {
        public Graph<T> graph { get; private set; }
        public GraphNode<T> node1 { get; private set; }
        public GraphNode<T> node2{ get; private set; }

        public float weight = 1;
        public bool directed = false;

        public GraphEdge(Graph<T> g, GraphNode<T> node1, GraphNode<T> node2) {
            graph = g;
            this.node1 = node1;
            this.node2 = node2;
        }
    }
}