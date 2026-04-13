using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE.SpacePartition;
using LPE.Shape;


public class CollisionTestMain : MonoBehaviour {
    public GameObject square1;
    public GameObject square2;
    public Grid2D<Shape2D> grid;

    Shape2D s1;
    Shape2D s2;
    List<Shape2D> intersectionResults = new List<Shape2D>();


    private void Start() {
        grid = new Grid2D<Shape2D>(new Vector2(-100, -100), new Vector2(100, 100), new Vector2Int(150, 150));
        s1 = ConvexPolygonShape.Regular(5);
        s2 = ConvexPolygonShape.Regular(3);
        s2 = new CircleShape(1);
        grid.Add(s1, s1.AABB());
        grid.Add(s2, s2.AABB());
    }

    private void Update() {
        s1.SetPosition(square1.transform.position, false);
        s1.SetRotation(square1.transform.rotation.eulerAngles.z, false);
        s1.SetScale(square1.transform.localScale.z, false);
        s1.Update();

        s2.SetPosition(square2.transform.position, false);
        s2.SetRotation(square2.transform.rotation.eulerAngles.z, false);
        s2.SetScale(square2.transform.localScale.z, false);
        s2.Update();

        grid.UpdateItem(s1, s1.AABB());
        grid.UpdateItem(s2, s2.AABB());
    }

    private void OnDrawGizmos() {
        if (s1 != null) {
            intersectionResults.Clear();
            grid.QueryItems(s1.AABB(), intersectionResults);
            Gizmos.color = Color.green;

            foreach (var s in intersectionResults) {
                if (s == s1) {
                    continue;
                }

                if (s.CheckCollision(s1)) {
                    Gizmos.color = Color.red;
                    break;
                }
            }
            DrawPoly(s1.Vertices());
        }
        if (s1 != null) {
            intersectionResults.Clear();
            grid.QueryItems(s2.AABB(), intersectionResults);
            Gizmos.color = Color.green;
            foreach (var s in intersectionResults) {
                if (s == s2) {
                    continue;
                }

                if (s.CheckCollision(s2)) {
                    Gizmos.color = Color.red;
                    break;
                }
            }
            DrawCircle((CircleShape)s2);
        }

    }
    void DrawPoly(Vector2[] vs) {
        for (int i = 0; i < vs.Length - 1; i++) {
            Gizmos.DrawLine(vs[i], vs[i + 1]);
        }
        Gizmos.DrawLine(vs[0], vs[vs.Length - 1]);
    }

    void DrawCircle(CircleShape c) {
        Gizmos.DrawWireSphere(c.position, c.radius * c.scale);
    }
}

