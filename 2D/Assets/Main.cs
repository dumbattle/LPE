using System.Linq;
using UnityEngine;
using LPE2D;
using LPE;
using Unity.Profiling;

public class Main : MonoBehaviour {
    public static Main main;
    ProfilerMarker qtUpdateMarker = new ProfilerMarker("Quadtree update");
    ShapeManager sm;

    public static int size = 100;
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
        main = this;
        sm = new ShapeManager(new Vector2(size * 2 + 1, size * 2 + 1));

        astroids = new Astroid[numAstroids];
        for (int i = 0; i < numAstroids; i++) {
            astroids[i] = new Astroid();
            sm.AddShape(astroids[i]);
        }


    }

    private void Update() {
        qtUpdateMarker.Begin();
        foreach (var a in astroids) {
            a.Update();
        }
        qtUpdateMarker.End();

        sm.CleanUpMapTree();
    }
    void OnDrawGizmos() {
        sm?.OnDrawGizmos();
    }
}
