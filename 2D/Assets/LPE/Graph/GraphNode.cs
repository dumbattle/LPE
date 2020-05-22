using System.Collections.Generic;
using System;

namespace LPE.Graph {
    public partial class GraphNode<T> {
        Dictionary<GraphNode<T>, NodeConnection<T>> _connections = new Dictionary<GraphNode<T>, NodeConnection<T>>();

        public T value { get; private set; }


        public GraphNode( T value) {
            this.value = value;
        }


        public GraphEdge<T> AddConnection(GraphNode<T> other, bool directed) {
            GraphEdge<T> e = new GraphEdge<T>(this, other) {
                directed = directed
            };

            if (directed) {
                _connections.Add(other, new NodeConnection<T>(other, e, ConnectionType.away));
                other._connections.Add(this, new NodeConnection<T>(this, e, ConnectionType.towards));
            }
            else {
                _connections.Add(other, new NodeConnection<T>(other, e, ConnectionType.nonDirected));
                other._connections.Add(this, new NodeConnection<T>(this, e, ConnectionType.nonDirected));
            }

            return e;
        }

        public bool IsConnected(GraphNode<T> n) {
            if (_connections.ContainsKey(n)) {
                var c = _connections[n];

                return c.type == ConnectionType.away || c.type == ConnectionType.nonDirected;
            }

            return false;
        }

        public IEnumerable<GraphNode<T>> NextNodes() {
            foreach (var c in _connections) {
                if (c.Value.type == ConnectionType.away) {
                    yield return c.Value.otherNode;
                }
            }
        }
        public IEnumerable<GraphNode<T>> PreviousNodes() {
            foreach (var c in _connections) {
                if (c.Value.type == ConnectionType.towards) {
                    yield return c.Value.otherNode;
                }
            }
        }
        public IEnumerable<GraphNode<T>> NonDirectedConnectedNode() {
            foreach (var c in _connections) {
                if (c.Value.type == ConnectionType.nonDirected) {
                    yield return c.Value.otherNode;
                }
            }
        }

        public IEnumerable<GraphNode<T>> ConnectedNodes() {
            foreach (var c in _connections) {
                yield return c.Value.otherNode;
            }
        }
        public IEnumerable<NodeConnection<T>> Connections() {
            foreach (var c in _connections) {
                yield return c.Value;
            }
        }

        public float DistanceTo(GraphNode<T> other) {
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

                foreach (var kv in next.Value.node._connections) {
                    var c = kv.Value;
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

                foreach (var kv in currentNode._connections) {
                    var c = kv.Value;
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