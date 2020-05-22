using System.Collections.Generic;
using System;


namespace LPE {

    namespace Graph {
        public static class PathFinding {
            public static void AStar<T>(GraphNode<T> start, GraphNode<T> end, List<GraphNode<T>> results, Func<T, T, float> heuristic) {
            
                results.Clear();

                PriorityQueue<GraphNode<T>> q = new PriorityQueue<GraphNode<T>>();

                Dictionary<GraphNode<T>, AStartNode<T>> nodes = new Dictionary<GraphNode<T>, AStartNode<T>>();

                nodes.Add(start, new AStartNode<T> { previous = null, gScore = 0, fScore = heuristic(start.value, end.value) });

                q.Add(start, -nodes[start].fScore);
                while (!q.isEmpty) {
                    var current = q.Get();

                    if (current == end) {

                        while (current != null && nodes.ContainsKey(current)) {
                            results.Add(current);
                            current = nodes[current].previous;
                        }
                        results.Reverse();
                        return;
                    }

                    foreach (var c in current.Connections()) {
                        if (c.type == ConnectionType.towards) {
                            continue;
                        }

                        float s = nodes[current].gScore + c.edge.weight;

                        GraphNode<T> next = c.otherNode;

                        if (!nodes.ContainsKey(next)) {
                            nodes.Add(next, new AStartNode<T> {
                                previous = current,
                                gScore = s,
                                fScore = s + heuristic(next.value, end.value)
                            });
                            q.Add(next, -nodes[next].fScore);
                        }
                        else if (s < nodes[next].gScore) {
                            nodes[next] = new AStartNode<T> {
                                previous = current,
                                gScore = s,
                                fScore = s + heuristic(next.value, end.value)
                            };

                            q.Add(next, -nodes[next].fScore);
                        }
                    }
                }

            }

            struct AStartNode<T> {
                public GraphNode<T> previous;
                public float gScore;
                public float fScore;
            }

        }
    }
}