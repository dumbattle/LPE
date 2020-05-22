using System.Collections.Generic;
using UnityEngine;

using LPE.Graph;


public class Main : MonoBehaviour {
    int numNodes = 10;

    Graph<Vector2> g;
    GraphNode<Vector2>[][] nodes;
    GraphNode<Vector2> start;
    GraphNode<Vector2> end;
    void Start() {
        g = new Graph<Vector2>();
        nodes = new GraphNode<Vector2>[10][];

        for (int x = 0; x < 10; x++) {
            nodes[x] = new GraphNode<Vector2>[10];
            for (int y = 0; y < 10; y++) {
                nodes[x][y] = g.CreateNode(new Vector2(x, y));
            }
        }


        bool[,] connected = new bool[10, 10];
        connected[0, 0] = true;
        bool keepGoing = true;

        while (keepGoing) {
            keepGoing = false;
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 10; y++) {

                    if (!connected[x, y]) {
                        keepGoing = true;
                        if (InRange(x + 1, y) && connected[x + 1, y] && !nodes[x][y].IsConnected(nodes[x + 1][y]) && Random.value < .5f) {
                            g.ConnectNodes(nodes[x][y], nodes[x + 1][y]);
                            connected[x, y] = true;
                        }
                        if (InRange(x - 1, y) && connected[x - 1, y] && !nodes[x][y].IsConnected(nodes[x - 1][y]) && Random.value < .5f) {
                            g.ConnectNodes(nodes[x][y], nodes[x - 1][y]);
                            connected[x, y] = true;
                        }
                        if (InRange(x, y + 1) && connected[x, y + 1] && !nodes[x][y].IsConnected(nodes[x][y + 1]) && Random.value < .5f) {
                            g.ConnectNodes(nodes[x][y], nodes[x][y + 1]);
                            connected[x, y] = true;
                        }
                        if (InRange(x, y - 1) && connected[x, y - 1] && !nodes[x][y].IsConnected(nodes[x][y - 1]) && Random.value < .5f) {
                            g.ConnectNodes(nodes[x][y], nodes[x][y - 1]);
                            connected[x, y] = true;
                        }
                    }

                    bool InRange(int _x, int _y) {
                        return _x >= 0 && _x < 10 && _y >= 0 && _y < 10;
                    }
                }
            }
        }

        start = nodes[Random.Range(0, 10)][Random.Range(0, 10)];
        end = nodes[Random.Range(0, 10)][Random.Range(0, 10)];
    }
    private void Update() {

        start = nodes[Random.Range(0, 10)][Random.Range(0, 10)];
        end = nodes[Random.Range(0, 10)][Random.Range(0, 10)];
    }

    private void OnDrawGizmos() {
        if (g != null) {
            foreach (var n in g.GetNodes()) {
                Gizmos.color = Color.blue;

                foreach (var c in n.Connections()) {
                    Gizmos.DrawLine(n.value, c.otherNode.value);
                }
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(n.value, .2f);
            }

            List<GraphNode<Vector2>> path = new List<GraphNode<Vector2>>();
            PathFinding.AStar(start, end, path, (a, b) => (a - b).sqrMagnitude);

            Gizmos.color = Color.red;
            for (int i = 1; i < path.Count; i++) {
                Gizmos.DrawLine(path[i - 1].value, path[i].value);
            }
        }
    }

}