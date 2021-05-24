using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE.Steering;


public class SteeringMain : MonoBehaviour {
    public GameObject target;
    public GameObject dir;
    public GameObject start;

    public int numAgents;

    List<SteerAgent> agents;


    void Start() {
        //for (int i = 0; i < 100; i++) {
        //    var a = Random.Range(-100f, 100f);
        //    var b = Random.Range(-100f, 100f);
        //    var c = Random.Range(-100f, 100f);

        //    var (s1, s2) = LPE.Math.Geometry.SolveQuadratic(a, b, c);

        //    if (s1 != null) {
        //        var t = a * s1.Value * s1.Value + b * s1.Value + c;
        //        if (Mathf.Abs((float)t) > .000001f) {
        //            print(t);
        //        }
        //    }
        //    if (s2 != null) {
        //        var t = a * s2.Value * s2.Value + b * s2.Value + c;
        //        if (Mathf.Abs((float)t) > .000001f) {
        //            print(t);
        //        }

        //        if (s2 < s1) {
        //            print(s1.Value);
        //            print(s2.Value);
        //        }
        //    }
        //}
        //return;
        agents = new List<SteerAgent>();

        for (int i = 0; i < numAgents; i++) {
            agents.Add(new SteerAgent() {
                pos = Random.insideUnitCircle * i / 10f,
                dir = new Vector2(0, 0),
                size = 1f
            });

        }
    }

    void Update() {
        //return;
        foreach (var a in agents) {
            var tar = target.transform.position;
            if ((a.pos - (Vector2)target.transform.position).magnitude < 3) {
                continue;
                tar = a.pos;
                a.dir = Vector2.zero;
            }
            var s = Steering.Basic(tar, a, agents);

            a.dir = Vector2.ClampMagnitude(s, .1f);


            // don't walk into other agents
            //float t = 1;

            //foreach (var other in agents) {
            //    if (other == a) {
            //        continue;
            //    }
            //    // sphere cast in direction
            //    var tt = LPE.Math.Geometry.CircleCast_Circle(a.pos, a.size, other.pos, other.size, a.dir);

            //    // shorten dir
            //    if (tt >= 0 && tt < t) {
            //        t = tt;
            //    }
            //}

            //if (t < 1) {
            //    a.dir *= t;
            //}

            a.pos += a.dir;
        }
    }

    private void OnDrawGizmos() {
        //var c1 = start.transform.position;
        //var r1 = start.transform.lossyScale.x;
        //var c2 = target.transform.position;
        //var r2 = target.transform.lossyScale.x;

        //var d = dir.transform.position - start.transform.position;
        //var t = LPE.Math.Geometry.CircleCast_Circle(c1, r1, c2, r2, d);

        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(c1, r1);

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(c2, r2);

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(c1, c1 + d * 100);

        //if (t > 0) {

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(c1 + d * t, r1);
        //    Debug.Log(LPE.Math.Geometry.CircleCast_Circle(c1 + d * t, r1, c2, r2, d));

        //}
        if (agents == null) {
            return;
        }

        Gizmos.color = Color.green;
        foreach (var a in agents) {
            Gizmos.DrawWireSphere(a.pos, a.size);
        }
    }
}
