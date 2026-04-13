using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using LPE.Math;
using Unity.Profiling;

namespace LPE.Legacy {
    public static partial class DelaunayExtensions {
        static ProfilerMarker pm = new ProfilerMarker("A*");
        static ProfilerMarker pm2 = new ProfilerMarker("A* Priority Queue");
        static ProfilerMarker pm3 = new ProfilerMarker("A* Sub");
        static void print(object o) {
            Debug.Log(o);
        }

        public static DelaunayTriangle Point2Triangle(this Delaunay d, Vector2 v) {
            // random point
           return d.Point2Triangle(v, d.s1);
        }

        public static DelaunayTriangle Point2Triangle(this Delaunay d, Vector2 v, DelaunayVertex hint) {
            if (d.vertices.ContainsKey(v) && d.vertices[v].edges.Count > 0) {
                return d.vertices[v].edges.First().t1;
            }
            // random point
            DelaunayVertex startV = hint;


            // start triangle
            DelaunayTriangle t = null;
            DelaunayEdge inter = null;

            foreach (var e in startV.edges) {
                // intersecting edges?
                inter = GetIntersecting(e.t1).a;
                if (inter != null) {
                    t = e.t1;
                    break;
                }
                inter = GetIntersecting(e.t2).a;
                if (inter != null) {
                    t = e.t2;
                    break;
                }

                // in start triangle?
                if (e.t1 != null && Geometry.InTriangle(v, e.t1.v1.pos, e.t1.v2.pos, e.t1.v3.pos)) {
                    return e.t1;
                }
                if (e.t2 != null && Geometry.InTriangle(v, e.t2.v1.pos, e.t2.v2.pos, e.t2.v3.pos)) {
                    return e.t2;
                }

                // edge is on path?
                var vother = e.v1 == startV ? e.v2 : e.v1;
                if (Geometry.OnSegment(vother.pos, startV.pos, v)) {
                    // restart using other vertex
                    return d.Point2Triangle(v, vother);
                }

            }

            if (t == null) {
                // out of bounds
                return null;
            }

            // walk
            while (true) {
                // flip triangle
                var t1 = inter.t1;
                var t2 = inter.t2;

                t = t1 == t ? t2 : t1;

                if (t == null) {
                    // out of bounds
                    return null;
                }

                var (e1, e2) = GetIntersecting(t);
                inter = inter == e1 ? e2 : e1; // next edge

                if (inter == null) {
                    // inside?
                    if (Geometry.InTriangle(v, t.v1.pos, t.v2.pos, t.v3.pos)) {
                        return t;
                    }

                    // does walk path intersect a vertex?
                    var nv = Geometry.OnSegment(t.v1.pos, startV.pos, v) ? t.v1 :
                             Geometry.OnSegment(t.v2.pos, startV.pos, v) ? t.v2 :
                             Geometry.OnSegment(t.v3.pos, startV.pos, v) ? t.v3 : null;

                 
                    if (nv != null) {
                        // restart walk from that vertex
                        return d.Point2Triangle(v, nv);
                    }

                    // rounding errors - current triangle is suitable
                    return t;

                }

            }


            (DelaunayEdge a, DelaunayEdge b) GetIntersecting(DelaunayTriangle t) {
                if (t == null) {
                    return (null, null);
                }
                DelaunayEdge ra = null;
                DelaunayEdge rb = null;
                if (Geometry.IsIntersecting(t.e1.v1.pos, t.e1.v2.pos, startV.pos, v)) {
                    rb = t.e1;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }

                if (Geometry.IsIntersecting(t.e2.v1.pos, t.e2.v2.pos, startV.pos, v)) {
                    rb = t.e2;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }
                if (Geometry.IsIntersecting(t.e3.v1.pos, t.e3.v2.pos, startV.pos, v)) {
                    rb = t.e3;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }

                return (ra, rb);
            }
        }

  
        public static float PathLength(List<Vector2> p) {
            float result = 0;
            for (int i = 0; i < p.Count - 1; i++) {
                result += (p[i] - p[i + 1]).magnitude;
            }
            return result;
        }
     
        public static void DrawGizmos(this Delaunay d, Color color, Color constraintColor) {
            foreach (var t in d.triangles) {
                if (t.super) {
                    continue;
                }
                Gizmos.color = t.e1.IsConstraint ? constraintColor : color;
                Gizmos.DrawLine(t.e1.v1.pos, t.e1.v2.pos);
                Gizmos.color = t.e2.IsConstraint ? constraintColor : color;
                Gizmos.DrawLine(t.e2.v1.pos, t.e2.v2.pos);
                Gizmos.color = t.e3.IsConstraint ? constraintColor : color;
                Gizmos.DrawLine(t.e3.v1.pos, t.e3.v2.pos);
            }
        }


        public static void DrawGizmos(this Delaunay d, Vector3 offset) {
            foreach (var t in d.triangles) {
                if (t.super) {
                    continue;
                }
                Gizmos.color = t.e1.IsConstraint ? Color.black : Color.blue;
                Gizmos.DrawLine((Vector3)t.e1.v1.pos + offset, (Vector3)t.e1.v2.pos + offset);
                Gizmos.color = t.e2.IsConstraint ? Color.black : Color.blue;
                Gizmos.DrawLine((Vector3)t.e2.v1.pos + offset, (Vector3)t.e2.v2.pos + offset);
                Gizmos.color = t.e3.IsConstraint ? Color.black : Color.blue;
                Gizmos.DrawLine((Vector3)t.e3.v1.pos + offset, (Vector3)t.e3.v2.pos + offset);
            }
        }
    }
}
