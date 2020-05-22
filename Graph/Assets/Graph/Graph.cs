using System.Collections.Generic;
using System;

namespace LPE.Graph {
    public class Graph<T> {
        public int numNodes => _nodes.Count;

        List<GraphNode<T>> _nodes = new List<GraphNode<T>>();
        List<GraphEdge<T>> _edges = new List<GraphEdge<T>>();        

        public IEnumerable<GraphNode<T>> GetNodes(Func<GraphNode<T>, bool> condition) {
            foreach (var n in _nodes) {
                if (condition(n)) {
                    yield return n;
                }
            }
        }
        public IEnumerable<GraphNode<T>> GetNodes() {
            return _nodes;
        }
        public IEnumerable<GraphNode<T>> Roots() {
            foreach (var n in _nodes) {
                bool valid = true;
                foreach (var cn in n.Connections()) {
                    if (cn.type != ConnectionType.away) {
                        valid = false;
                        break;
                    }
                }
                if (!valid) {
                    continue;
                }
                yield return n;
            }
        }
        public IEnumerable<GraphNode<T>> Leaves() {
            foreach (var n in _nodes) {
                bool valid = true;
                foreach (var cn in n.Connections()) {
                    if (cn.type != ConnectionType.towards) {
                        valid = false;
                        break;
                    }
                }
                if (!valid) {
                    continue;
                }
                yield return n;
            }
        }

        // node creation
        public GraphNode<T> CreateNode(T value) {
            var n = new GraphNode<T>(value);
            _nodes.Add(n);
            return n;
        }
        public GraphNode<T> GetOrCreateNode(T value) {
            foreach (var n in GetNodes((n) => Comparer<T>.Default.Compare(n.value,value) == 0)) {
                return n;
            }

            return CreateNode(value);
        }

        // node connections
        public GraphEdge<T> ConnectNodes(GraphNode<T> first, GraphNode<T> second, bool directed = false) {
            var e = (first.AddConnection(second, directed));
            _edges.Add(e);

            return e;
        }


    }

}