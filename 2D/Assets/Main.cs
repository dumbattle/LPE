using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LPE2D;
using LPE;
using Unity.Profiling;

public class Main : MonoBehaviour {
    ProfilerMarker qtUpdateMarker = new ProfilerMarker("Quadtree update");
    ShapeManager sm;

    RectangleShape r2;
    public static int size = 100;
    public static int numAstroids = 500;
    Astroid[] astroids;

    void Start() {
        sm = new ShapeManager(new Vector2(size * 2 + 1, size * 2 + 1));

        //for (int x = -size; x <= size; x++) {
        //    for (int y = -size; y <= size; y++) {
        //        RectangleShape r = new RectangleShape(.4f, .4f);
        //        r.position = new Vector2(x, y);
        //        sm.AddShape(r);
        //    }
        //}
        astroids = new Astroid[numAstroids];
        for (int i = 0; i < numAstroids; i++) {
            astroids[i] = new Astroid();
            sm.AddShape(astroids[i]);
        }
        r2 = new RectangleShape(transform.localScale.x, transform.localScale.y);

        r2.rotation = Vector2.Angle(transform.right, Vector2.right);
        r2.position = transform.position;

        //sm.AddShape(r2);
    }

    private void Update() {
        r2.rotation = Vector2.Angle(transform.right, Vector2.right);
        r2.position = transform.position;
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

public class Astroid : IShape2D {
    CircleShape _rect;
    CircleShape sightRegion;

    public Shape2D shape => _rect;
    public event System.Action OnShapeUpdate {
        add => shape.OnShapeUpdate += value;
        remove => shape.OnShapeUpdate -= value;
    }

    Vector2 direction;
    float angularSpeed;

    List<Astroid> al = new List<Astroid>();

    public Astroid() {
        direction = Random.insideUnitCircle;
        angularSpeed = Random.Range(0f, 10f);
        //_rect = new RectangleShape(Random.Range(.1f, 2f), Random.Range(.1f, 2f));
        _rect = new CircleShape(Random.Range(.5f, 2f));
        _rect.position = new Vector2(Random.Range(-Main.size, Main.size), Random.Range(-Main.size, Main.size));
        sightRegion = new CircleShape(5);
    }
    
    public void Update() {
        _rect.position += direction;
        //_rect.rotation += angularSpeed;

        _rect.position = new Vector2(
            (_rect.position.x + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size, 
            (_rect.position.y + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size);

        al.Clear();
        ShapeManager.partionRoot.GetOverlap(this, al);


        if (al.Count > 0) {
            direction = _rect.CheckCollisionWithCorrection(al[0]).normalized * direction.magnitude;
        }
        al.Clear();
        ShapeManager.partionRoot.GetOverlap(sightRegion, al);
        foreach (var o in al) {

        }
        _rect.UpdateShape();
    }
}