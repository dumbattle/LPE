using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using LPE.Math;

namespace LPE.Triangulation {
    public static partial class DelaunayExtensions {
        static Unity.Profiling.ProfilerMarker pm = new Unity.Profiling.ProfilerMarker("test");
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
            return null;
        }

        static PriorityQueue<DelaunayTriangle> queue = new PriorityQueue<DelaunayTriangle>();
        static Dictionary<DelaunayTriangle, AStarCache> cache = new Dictionary<DelaunayTriangle, AStarCache>();
        public static List<DelaunayTriangle> AStar(this Delaunay d, Vector2 start, Vector2 end, List<DelaunayTriangle> result = null, float radius = 0) {
            float seDist = (start - end).magnitude;
            DelaunayTriangle tstart = d.Point2Triangle(start);
            DelaunayTriangle tend = d.Point2Triangle(end);
            result = result ?? new List<DelaunayTriangle>();
            if (tstart == tend) {
                result.Add(tstart);

                return result;
            }
            queue.Clear();
            cache.Clear();

            cache.Add(tstart, AStarCache.Get());
            cache.Add(tend, AStarCache.Get());
            var cs = cache[tstart];
            var ce = cache[tend];
            cs.g = 0;
            cs.h = seDist;
            cs.start = true;
            cs.startPos = start;
            queue.Add(tstart, 0);
            while (!queue.isEmpty) {
                var t = queue.Get();
                var ct = cache[t];
                if (t == tend) {
                    break;
                }

                var t1 = t.e1.t1 == t ? t.e1.t2 : t.e1.t1;
                var t2 = t.e2.t1 == t ? t.e2.t2 : t.e2.t1;
                var t3 = t.e3.t1 == t ? t.e3.t2 : t.e3.t1;
                if (ct.entry != t.e1) CheckNeighbor(t1, t.e1);
                if (ct.entry != t.e2) CheckNeighbor(t2, t.e2);
                if (ct.entry != t.e3) CheckNeighbor(t3, t.e3);
                void CheckNeighbor(DelaunayTriangle n, DelaunayEdge e) {
                    // don't cross constraints
                    if (e.IsConstraint) {
                        return;
                    }
                    // too small
                    Vector2 v1 = e.v1.pos;
                    Vector2 v2 = e.v2.pos;
                    if ((v1 - v2).sqrMagnitude < radius * radius) {
                        return;
                    }
                    // out of bounds
                    if (n==null) {
                        return;
                    }
                    if (!cache.ContainsKey(n)) {
                        cache.Add(n, AStarCache.Get());
                        cache[n].h = Mathf.Sqrt(Mathf.Min((end - v1).sqrMagnitude, (end - v2).sqrMagnitude));
                    }

                    var c = cache[n];

                    // estimate g
                    float gg = Mathf.Sqrt(Mathf.Min((start - v1).sqrMagnitude, (start - v2).sqrMagnitude));
                    float a = seDist - c.h;


                    if (a > gg) {
                        gg =a;
                    }

                    a = ct.f - c.h;
                    if (a > gg) {
                        gg = a;
                    }

                    a = ct.g;
                    if (a > gg) {
                        gg = a;
                    }

                    if (gg < c.g) {
                        c.prev = t;
                        c.entry = e;
                        c.g = gg;
                        queue.Add(n, -c.f);
                    }
                }
            }


            if (ce.prev == null) {
                return null;
            }


            BackTrack(tend, result, cache);

            foreach (var kv in cache) {
                AStarCache.Return(kv.Value);
            }
            cache.Clear();
            return result;

            static void BackTrack(DelaunayTriangle t, List<DelaunayTriangle> output, Dictionary<DelaunayTriangle, AStarCache> cache) {
                output.Clear();
                while (t != null) {
                    output.Add(t);
                    t = cache[t].prev;
                }

                output.Reverse();
            }
        }


        public static float PathLength(List<Vector2> p) {
            float result = 0;
            for (int i = 0; i < p.Count - 1; i++) {
                result += (p[i] - p[i + 1]).magnitude;
            }
            return result;
        }
        
        class AStarCache {
            static ObjectPool<AStarCache> _pool = new ObjectPool<AStarCache>(() => new AStarCache());
            public static AStarCache Get() { var result = _pool.Get(); result.Init(); return result; }
            public static void Return(AStarCache asc) => _pool.Return(asc);
            public DelaunayTriangle prev;
            public DelaunayEdge entry;
            /// <summary>
            /// cost from start to here
            /// </summary>
            public float g = Mathf.Infinity;
            /// <summary>
            /// estimated cost from start to end through here
            /// </summary>
            public float f => g + h;
            /// <summary>
            /// estimated cost from here to end
            /// </summary>
            public float h = -1;

            public bool start = false;
            public Vector2 startPos;

            private AStarCache() { }
            void Init() {
                prev = null;
                entry = null;
                g = Mathf.Infinity;
                h = -1;
                start = false;
                startPos = new Vector2(0, 0);
            }
            public float DistToEdge(DelaunayEdge e) {
                float result = 0;
                if (start) {
                    result =  Mathf.Sqrt(Mathf.Min((startPos - e.v1.pos).sqrMagnitude, (startPos - e.v2.pos).sqrMagnitude));
                }
                else {
                    var a = Mathf.Min(
                        (entry.v1.pos - e.v1.pos).sqrMagnitude,
                        (entry.v1.pos - e.v2.pos).sqrMagnitude);
                    var b = Mathf.Min(
                        (entry.v2.pos - e.v1.pos).sqrMagnitude,
                        (entry.v2.pos - e.v2.pos).sqrMagnitude);
                    result = 
                        Mathf.Sqrt(
                            Mathf.Min(
                                a,
                                b));
                }

                return result;
            }
        }
    
    
        public static void DrawGizmos(this Delaunay d) {
            foreach (var t in d.triangles) {
                if (t.super) {
                    continue;
                }
                Gizmos.DrawLine(t.e1.v1.pos, t.e1.v2.pos);
                Gizmos.DrawLine(t.e2.v1.pos, t.e2.v2.pos);
                Gizmos.DrawLine(t.e3.v1.pos, t.e3.v2.pos);
            }
        }    
    }
}
