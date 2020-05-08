namespace LPE.Graph {
    public class NodeConnection<T> {
        public GraphNode<T> otherNode {get; private set; }
        public ConnectionType type { get; private set; }
        public GraphEdge<T> edge { get; private set; }

        public NodeConnection(GraphNode<T> n, GraphEdge<T> e, ConnectionType type) {
            otherNode = n;
            edge = e;
            this.type = type;
        }
    }

    public enum ConnectionType {
        nonDirected,
        towards,
        away
    }
}