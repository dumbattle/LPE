using System.Collections.Generic;
using System;

namespace LPE.Graph {
    public class GraphNode<T> {
        List<NodeConnection<T>> _connections = new List<NodeConnection<T>>();

        public Graph<T> graph { get; private set; }
        public T value { get; private set; }


        public GraphNode(Graph<T> graph, T value) {
            this.graph = graph;
            this.value = value;
        }
        public GraphEdge<T> AddConnection(GraphNode<T> other, bool directed) {
            if (other.graph != graph) {
                throw new InvalidOperationException($"ERROR: connecting nodes: Nods are not a part of the same graph");
            }

            GraphEdge<T> e = new GraphEdge<T>(graph, this, other) {
                directed = directed
            };

            if (directed) {
                _connections.Add(new NodeConnection<T>(other, e, ConnectionType.away));
                other._connections.Add(new NodeConnection<T>(this, e, ConnectionType.towards));
            }
            else {
                _connections.Add(new NodeConnection<T>(other, e, ConnectionType.nonDirected));
                other._connections.Add(new NodeConnection<T>(this, e, ConnectionType.nonDirected));
            }

            return e;
        }

        public IEnumerable<GraphNode<T>> NextNodes() {
            foreach (var c in _connections) {
                if (c.type == ConnectionType.away) {
                    yield return c.otherNode;
                }
            }
        }
        public IEnumerable<GraphNode<T>> PreviousNodes() {
            foreach (var c in _connections) {
                if (c.type == ConnectionType.towards) {
                    yield return c.otherNode;
                }
            }
        }
        public IEnumerable<GraphNode<T>> NonDirectedConnectedNode() {
            foreach (var c in _connections) {
                if (c.type == ConnectionType.nonDirected) {
                    yield return c.otherNode;
                }
            }
        }
        public IEnumerable<GraphNode<T>> ConnectedNodes() {
            foreach (var c in _connections) {
                yield return c.otherNode;
            }
        }
        public IEnumerable<NodeConnection<T>> Connections() {
            foreach (var c in _connections) {
                yield return c;
            }
        }

        public float DistanceTo(GraphNode<T> other) {
            if (other.graph != graph) {
                throw new InvalidOperationException("Nodes are not part of the same graph");
            }

            LinkedList<(GraphNode<T> node, float dist)> queue = new LinkedList<(GraphNode<T>, float)>();
            queue.AddFirst((this, 0));
            List<GraphNode<T>> visited = new List<GraphNode<T>>();

            int safety = 100;
            while (queue.Count > 0 && safety-- > 0) {
                var next = queue.First;
                queue.RemoveFirst();

                if (visited.Contains(next.Value.node)) {
                    continue;
                }
                visited.Add(next.Value.node);

                if (next.Value.node == other) {
                    return next.Value.dist;
                }

                foreach (var c in next.Value.node._connections) {
                    if (c.type == ConnectionType.nonDirected || c.type == ConnectionType.away) {
                        float dist = next.Value.dist + c.edge.weight;
                        var n = queue.First;
                        while (n != null) {
                            if (n.Value.dist > dist) {
                                queue.AddBefore(n, (c.otherNode, dist));
                                break;
                            }
                            n = n.Next;
                        }

                        if (n == null) {
                            queue.AddLast((c.otherNode, dist));
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// returns list of all nodes that can be reached, including itself
        /// </summary>
        /// <returns></returns>
        public List<GraphNode<T>> GetAccessableNodes() {
            List<GraphNode<T>> result = new List<GraphNode<T>>() { this };

            for (int i = 0; i < result.Count; i++) {
                GraphNode<T> currentNode = result[i];

                foreach (var c in currentNode._connections) {
                    if (c.type == ConnectionType.nonDirected || c.type == ConnectionType.away) {
                        if (!result.Contains(c.otherNode)) {
                            result.Add(c.otherNode);
                        }
                    }
                }
            }
            return result;
        }
    }
}