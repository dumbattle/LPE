using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPE2D;
using LPE;
using Unity.Profiling;

public class Main : MonoBehaviour {
    public static Main main;
    ProfilerMarker qtUpdateMarker = new ProfilerMarker("Quadtree update");
    ShapeManager sm;

    public static int size = 200;
    public int numAstroids = 500;

    public float seperation = 1;
    public float alignment = 1;
    public float cohesion = 1;

    public static float Seperation => main.seperation;
    public static float Alignment => main.alignment;
    public static float Cohesion => main.cohesion;

    public GameObject target;
    public static GameObject Target => main.target;
    Astroid[] astroids;

    void Start() {
        //qt = new QuadTree<IShape2D>(new AABB(new Vector2(-10, -10), new Vector2(10, 10)), 7, 1);
        //shapes = new List<IShape2D>();

        //for (int i = 0; i < numAstroids; i++) {
        //    var c = new CircleShape(.2f);
        //    c.position = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));

        //    qt.Add(c, new AABB(c.AABB().min, c.AABB().max));
        //    shapes.Add(c);
        //}


        main = this;
        //return;
        sm = new ShapeManager(new Vector2(size * 2 + 1, size * 2 + 1));

        astroids = new Astroid[numAstroids];
        for (int i = 0; i < numAstroids; i++) {
            astroids[i] = new Astroid();
            //sm.AddShape(astroids[i]);
        }
    }

    private void Update() {
        //if (shapes.Count > 0) {
        //    qt.Remove(shapes[0]);
        //    shapes.RemoveAt(0);
        //}
        //var c = new CircleShape(.2f);
        //c.position = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));

        //qt.Add(c, new AABB(c.AABB().min, c.AABB().max));
        //shapes.Add(c);
        //qt.Prune();
        //return;
        qtUpdateMarker.Begin();
        foreach (var a in astroids) {
            a.Update();
        }
        qtUpdateMarker.End();

        sm.CleanUpMapTree();
    }
    void OnDrawGizmos() {
        //qt?.OnDrawGizmos();
        //Gizmos.color = Color.green;
        //if (shapes != null) {
        //    foreach (var s in shapes) {
        //        s.shape.OnDrawGizmos();
        //    }
        //}
        //return;
        sm?.OnDrawGizmos();
    }
}
