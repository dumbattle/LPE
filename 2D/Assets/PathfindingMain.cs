using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE;
using LPE.Graph;
using LPE.Hex;
using LPE2D;


public class PathfindingMain : MonoBehaviour {
    public int numWalls;
    public int mapRadius;

    LooseQuadTreePartion<IShape2D> qt;
    Shape2D[] walls;
    Graph<PathfindingNode> pfg;
    GraphNode<PathfindingNode>[,] nodes;

    void Start() {
        qt = new LooseQuadTreePartion<IShape2D>();
        qt.Initialize(new Vector2(-mapRadius, -mapRadius), new Vector2(mapRadius, mapRadius), 10, 3);

        walls = new Shape2D[numWalls];

        for (int i = 0; i < numWalls; i++) {

            walls[i] = new RectangleShape(Random.Range(1f, 5f), Random.Range(1f, 5f));
            walls[i].position = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            walls[i].UpdateShape();

            qt.AddShape(walls[i]);

        }



        pfg = new Graph<PathfindingNode>();
        nodes = new GraphNode<PathfindingNode>[mapRadius * 2, mapRadius * 2];
        CircleShape c = new CircleShape(.5f);

        for (int x = 0; x < mapRadius * 2; x++) {
            for (int y = 0; y < mapRadius * 2; y++) {
                nodes[x, y] = pfg.CreateNode(new PathfindingNode(new Vector2(x - mapRadius, y - mapRadius)));

                c.position = new Vector2(x - mapRadius, y - mapRadius);
                nodes[x, y].value.valid = !qt.IsColliding(c);
            }
        }
    }

    public Orientation orientation;
    void OnDrawGizmos() {
        if (qt == null) {
            return;
        }

        Gizmos.color = Color.blue;
        qt.OnDrawGizmos();
        Gizmos.color = Color.green;

        foreach (var s in walls) {
            s.OnDrawGizmos();
        }

        foreach (var n in pfg.GetNodes()) {
            Gizmos.color = n.value.valid ? Color.yellow : Color.red;
            Gizmos.DrawSphere(n.value.position, .1f);
        }

        //CircleShape crc = new CircleShape(1);
        //var g = new HexRectGrid(mapRadius * 2, mapRadius * 2, .5f, orientation);


        //foreach (var c in g) {
        //    Vector2 p = c.ToCartesian(radius: .5f) - new Vector2(mapRadius, mapRadius) + g.CenterOffset();
        //    crc.position = p;
        //    crc.UpdateShape();
        //    Gizmos.color = qt.IsColliding(crc) ? Color.red : Color.yellow;

        //    Gizmos.DrawSphere(p, .05f);

        //}

    }


    public class PathfindingNode {
        
        public Vector2 position;
        public bool valid = false;

        public PathfindingNode(Vector2 position) {
            this.position = position;
        }
    }

}

public class PathFindingGrid {
    Graph<Node> _graph;
    GraphNode<Node>[,] _nodes;

    Vector2 spacing;
    Vector2 offset;

    class Node {

    }
}
