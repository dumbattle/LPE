using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE.Steering;
using LPE.SpacePartition;
using Unity.Profiling;

public class SteeringMain : MonoBehaviour {
    public GameObject target;
    public GameObject dir;
    public GameObject start;

    public int numAgents;
    public GameObject[] objects = new GameObject[0];

    RectangleShape[] objRects;

    List<TestAgent> agents;

    Partition2D<TestAgent> partition;


    void Start() {
        agents = new List<TestAgent>();

        partition = new Grid2D<TestAgent>(new Vector2(-50, -50), new Vector2(50, 50), new Vector2Int(100, 100));

        for (int i = 0; i < numAgents; i++) {
            var size = .2f;
            var pos = Random.insideUnitCircle * i / 10f;
            var a = new TestAgent(
                pos,
                new Vector2(0, 0),
                size
            );
            agents.Add(a);


            partition.Add(a, a.shape.AABB());
        }

        objRects = new RectangleShape[objects.Length];

        for (int i = 0; i < objRects.Length; i++) {
            objRects[i] = new RectangleShape(objects[i].transform.localScale.x, objects[i].transform.localScale.y);
            objRects[i].position = objects[i].transform.position;
            objRects[i].UpdateShape();
        }
    }

    HashSet<TestAgent> nearby = new HashSet<TestAgent>();
    CircleShape cc = new CircleShape(1);
    
    void Update() {
        foreach (var a in agents) {
            var tar = target.transform.position;
            if ((a.pos - (Vector2)target.transform.position).magnitude < 3) {
                continue;
                tar = a.pos;
            }

            nearby.Clear();


            cc.radius = a.size * 2;
            cc.position = a.pos;

            nearby.Clear();
            partition.QueryItems(cc.AABB(), nearby);


            var s = Steering.Basic(tar, a, nearby);


            a.dir = Vector2.ClampMagnitude(s, .1f);


            //// don't overlap other agents
            //float t = 1;
            //foreach (var other in nearby) {
            //    var tt = LPE.Math.Geometry.CircleCast_Circle(a.pos, a.size, other.pos, other.size, a.dir);

            //    if (tt >= 0 && tt < t) {
            //        t = tt;
            //    }
            //}
            //a.dir *= t;


            a.Translate(a.dir);

            // get out of obstacles
            Vector2 cv = new Vector2(0, 0);

            foreach (var r in objRects) {
                var c = a.shape.CheckCollisionWithCorrection(r);
                cv += c;
            }
            a.Translate(cv);


            partition.UpdateItem(a, a.shape.AABB());
        }

        partition.CleanUp();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        foreach (var o in objects) {
            var b = GetRect(o);
            DrawBox(b.x1, b.x2, b.y1, b.y2);
        }
        if (agents == null) {
            return;
        }

        Gizmos.color = Color.green;
        foreach (var a in agents) {
            Gizmos.DrawWireSphere(a.pos, a.size);
        }



    }


    (float x1, float x2, float y1, float y2) GetRect(GameObject o) {
        return (
            o.transform.position.x - o.transform.lossyScale.x / 2,
            o.transform.position.x + o.transform.lossyScale.x / 2,
            o.transform.position.y - o.transform.lossyScale.y / 2,
            o.transform.position.y + o.transform.lossyScale.y / 2);
    }

    void DrawBox(float x1, float x2, float y1, float y2) {
        Gizmos.DrawLine(new Vector3(x1, y1), new Vector3(x1, y2));
        Gizmos.DrawLine(new Vector3(x1, y1), new Vector3(x2, y1));
        Gizmos.DrawLine(new Vector3(x2, y2), new Vector3(x1, y2));
        Gizmos.DrawLine(new Vector3(x2, y2), new Vector3(x2, y1));
    }
}

class TestAgent : ISteerAgent, IShape2D {
    CircleShape c;

    public Shape2D shape => ((IShape2D)c).shape;
    public Vector2 pos { get; set; }

    public Vector2 dir { get; set; }

    public float size { get; set; }

    public event System.Action OnShapeUpdate {
        add {
            ((IShape2D)c).OnShapeUpdate += value;
        }

        remove {
            ((IShape2D)c).OnShapeUpdate -= value;
        }
    }

    public TestAgent (Vector2 pos, Vector2 dir, float size) : base() {
        this.pos = pos;
        this.dir = dir;
        this.size = size;

        c = new CircleShape(size) { position = pos };
    }

    public void Translate(Vector2 dir) {
        pos += dir;
        c.position = pos;
    }

}
