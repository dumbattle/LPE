using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using LPE.Triangulation;


public class TriangulationMain : MonoBehaviour {
    public GameObject pointer;
    public GameObject start;
    public GameObject end;
    public GameObject bounds;
    public GameObject[] objects;
    public Vector2Int size;
    Delaunay d;
    List<DelaunayTriangle> path;
    List<Vector2> p;

    [Header("Display")]
    public bool drawDelaunay = false;
    void Start() {

    }

    private void Update() {
        d = new Delaunay();

        List<Vector2> points = new List<Vector2>();
        List<Vector2> con = new List<Vector2>();
        var (x1, x2, y1, y2) = GetRect(bounds);

        points.Add(new Vector2(x1, y1));
        points.Add(new Vector2(x1, y2));
        points.Add(new Vector2(x2, y1));
        points.Add(new Vector2(x2, y2));

        con.Add(new Vector2(x1, y1));
        con.Add(new Vector2(x1, y2));

        con.Add(new Vector2(x1, y2));
        con.Add(new Vector2(x2, y2));

        con.Add(new Vector2(x2, y2));
        con.Add(new Vector2(x2, y1));

        con.Add(new Vector2(x2, y1));
        con.Add(new Vector2(x1, y1));

        foreach (var o in objects) {
            if (WithinBounds(o)) {
                (x1, x2, y1, y2) = GetRect(o);

                points.Add(new Vector2(x1, y1));
                points.Add(new Vector2(x1, y2));
                points.Add(new Vector2(x2, y1));
                points.Add(new Vector2(x2, y2));

                con.Add(new Vector2(x1, y1));
                con.Add(new Vector2(x1, y2));

                con.Add(new Vector2(x1, y2));
                con.Add(new Vector2(x2, y2));

                con.Add(new Vector2(x2, y2));
                con.Add(new Vector2(x2, y1));

                con.Add(new Vector2(x2, y1));
                con.Add(new Vector2(x1, y1));
            }
        }
       
        
        d.AddPoints(points);
        d.AddConstraints(con);
        var b = GetRect(bounds);

        path = d.AStar(start.transform.position, end.transform.position);
        if (path != null) {
            p = DelaunayAlgorithms.Funnel(path, start.transform.position, end.transform.position);

        }

    }


    (float x1, float x2, float y1, float y2) GetRect(GameObject o) {
        return (
            o.transform.position.x - o.transform.lossyScale.x / 2,
            o.transform.position.x + o.transform.lossyScale.x / 2,
            o.transform.position.y - o.transform.lossyScale.y / 2,
            o.transform.position.y + o.transform.lossyScale.y / 2);
    }

    bool WithinBounds(GameObject o) {
        var a = GetRect(o);
        var b = GetRect(bounds);

        return
            a.x1 >= b.x1 &&
            a.x2 <= b.x2 &&
            a.y1 >= b.y1 &&
            a.y2 <= b.y2;
    }

    private void OnDrawGizmos() {
        DrawDelaunay();
        DrawEnvironment();
        DrawPath();

        //if (pointer == null) {
        //    return;
        //}
        //var mid = d.Point2Triangle(pointer.transform.position);

        //if (mid != null) {
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(mid.e1.v1.pos, mid.e1.v2.pos);
        //    Gizmos.DrawLine(mid.e2.v1.pos, mid.e2.v2.pos);
        //    Gizmos.DrawLine(mid.e3.v1.pos, mid.e3.v2.pos);
        //}



    }

    void DrawEnvironment() {
        if (bounds == null) {
            return;
        }
        Gizmos.color = Color.blue;
        var b = GetRect(bounds);
        DrawBox(b.x1, b.x2, b.y1, b.y2);
        foreach (var o in objects) {
            Gizmos.color = WithinBounds(o) ? Color.yellow : Color.red;
            b = GetRect(o);
            DrawBox(b.x1, b.x2, b.y1, b.y2);
        }
        void DrawBox(float x1, float x2, float y1, float y2) {
            Gizmos.DrawLine(new Vector3(x1, y1), new Vector3(x1, y2));
            Gizmos.DrawLine(new Vector3(x1, y1), new Vector3(x2, y1));
            Gizmos.DrawLine(new Vector3(x2, y2), new Vector3(x1, y2));
            Gizmos.DrawLine(new Vector3(x2, y2), new Vector3(x2, y1));
        }
    }
    private void DrawDelaunay() {
        if (!drawDelaunay) {
            return;
        }
        if (d == null) {
            return;
        }

        foreach (var t in d.triangles) {
            if (t.super) {
                continue;
            }
            Gizmos.color = t.e1.IsConstraint ? Color.red : Color.green;
            Gizmos.DrawLine(t.e1.v1.pos, t.e1.v2.pos);
            Gizmos.color = t.e2.IsConstraint ? Color.red : Color.green;
            Gizmos.DrawLine(t.e2.v1.pos, t.e2.v2.pos);
            Gizmos.color = t.e3.IsConstraint ? Color.red : Color.green;
            Gizmos.DrawLine(t.e3.v1.pos, t.e3.v2.pos);
        }
    }

    void DrawPath() {
        if (p == null) {
            return;
        }


        //if (path != null) {
        //    Gizmos.color = Color.white;
        //    foreach (var t in path) {
        //        Gizmos.color = new Color(Gizmos.color.r * .9f, Gizmos.color.g * .9f, Gizmos.color.b * .9f);
        //        Gizmos.DrawLine(t.e1.v1.pos, t.e1.v2.pos);
        //        Gizmos.DrawLine(t.e2.v1.pos, t.e2.v2.pos);
        //        Gizmos.DrawLine(t.e3.v1.pos, t.e3.v2.pos);
        //        //print($" {t.v1.pos}, {t.v2.pos}, {t.v3.pos}");
        //    }
        //}
        Gizmos.color = Color.black;

        for (int i = 0; i < p.Count - 1; i++) {
            Gizmos.DrawLine(p[i], p[i + 1]);
        }
        //Gizmos.color = Color.cyan;

        //for (int i = 0; i < path.Count - 1; i++) {
        //    var t1 = path[i];
        //    var t2 = path[i + 1];
        //    Gizmos.DrawLine(
        //        (t1.v1.pos + t1.v2.pos + t1.v3.pos) / 3f,
        //        (t2.v1.pos + t2.v2.pos + t2.v3.pos) / 3f
        //        );
        //}
    }
}
