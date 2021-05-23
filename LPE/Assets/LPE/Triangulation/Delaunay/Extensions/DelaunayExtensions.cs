using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using LPE.Math;

namespace LPE.Triangulation {
    public static partial class DelaunayExtensions {
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
            var safety = new LoopSafety(100000);

            while (safety.Inc()) {
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
    

        public static List<DelaunayTriangle> AStar(this Delaunay d, Vector2 start, Vector2 end, float radius = 0) {
            float seDist = (start - end).magnitude;
            DelaunayTriangle tstart = d.Point2Triangle(start);
            DelaunayTriangle tend = d.Point2Triangle(end);
            DelaunayTriangle closest = null;
            var closeDist = Mathf.Infinity;
            PriorityQueue<DelaunayTriangle> queue = new PriorityQueue<DelaunayTriangle>();
            Dictionary<DelaunayTriangle, AStarCache> cache = new Dictionary<DelaunayTriangle, AStarCache>();
            List<DelaunayTriangle> result = new List<DelaunayTriangle>();

            cache.Add(tstart, new AStarCache());
            cache.Add(tend, new AStarCache());
            var cs = cache[tstart];
            var ce = cache[tend];
            cs.g = 0;
            cs.h = seDist;
            cs.start = true;
            cs.startPos = start;

            queue.Add(tstart, 0);
            var safety = new LoopSafety(100);

            while (!queue.isEmpty && safety) {
                var t = queue.Get();
                var ct = cache[t];
                if (t == tend) {
                    BackTrack(tend, result, cache);
                    var f = Funnel(result, start, end);
                    ce.g = PathLength(f);
                    break;
                }
                if (ct.h < closeDist) {
                    closest = t;
                    closeDist = ct.h;
                }
                //if (ct.f > ce.g) {
                //    break;
                //}

                var t1 = t.e1.t1 == t ? t.e1.t2 : t.e1.t1;
                var t2 = t.e2.t1 == t ? t.e2.t2 : t.e2.t1;
                var t3 = t.e3.t1 == t ? t.e3.t2 : t.e3.t1;

                CheckNeighbor(t1, t.e1);
                CheckNeighbor(t2, t.e2);
                CheckNeighbor(t3, t.e3);

                void CheckNeighbor(DelaunayTriangle n, DelaunayEdge e) {
                    // don't cross constraints
                    if (e.IsConstraint) {
                        return;
                    }
                    // too small
                    if ((e.v1.pos - e.v2.pos).sqrMagnitude < radius * radius) {
                        return;
                    }
                    // out of bounds
                    if (n==null) {
                        return;
                    }

                    // update g
                    if (!cache.ContainsKey(n)) {
                        cache.Add(n, new AStarCache());
                        cache[n].h = Mathf.Sqrt(Mathf.Min((end - e.v1.pos).sqrMagnitude, (end - e.v2.pos).sqrMagnitude));

                    }
                    var c = cache[n];
                    float gg = Mathf.Max(
                        Mathf.Sqrt(Mathf.Min((start - e.v1.pos).sqrMagnitude, (start - e.v2.pos).sqrMagnitude)),
                        seDist - c.h,
                        ct.g + ct.h - c.h,
                        ct.g + ct.DistToEdge(e)
                    );

                    if (gg < c.g) {
                        c.prev = t;
                        c.entry = e;
                        c.g = gg;
                        queue.Add(n, -c.f);
                    }
                }
            }


            if (ce.prev == null) {
                tend = closest;
            }

         
            BackTrack(tend, result, cache);
            return result;
             
            static void BackTrack(DelaunayTriangle t, List<DelaunayTriangle> output, Dictionary<DelaunayTriangle, AStarCache> cache) {
                var safety = new LoopSafety(100);
                output.Clear();
                while (safety && (t != null)) {
                    output.Add(t);
                    t = cache[t].prev;
                }

                output.Reverse();
            }
        }


        public static List<Vector2> Funnel(List<DelaunayTriangle> channel, Vector2 start, Vector2 end) {
            List<Vector2> result = new List<Vector2>();
            LinkedList<DelaunayEdge> portals = new LinkedList<DelaunayEdge>();
            for (int i = 0; i < channel.Count - 1; i++) {
                DelaunayTriangle t1 = channel[i];
                DelaunayTriangle t2 = channel[i + 1];
                portals.AddLast(
                    t1.e1 == t2.e1 || t1.e1 == t2.e2 || t1.e1 == t2.e3 ? t1.e1 :
                    t1.e2 == t2.e1 || t1.e2 == t2.e2 || t1.e2 == t2.e3 ? t1.e2 :
                    t1.e3 == t2.e1 || t1.e3 == t2.e2 || t1.e3 == t2.e3 ? t1.e3 : null);
            }

            result.Add(start);

            var en = portals.First;

            var p = en.Value;
            var s = start;
            var a = p.v1;
            var b = p.v2;
            var pa = a;
            var pb = b;

            var ra = Geometry.IsClockwise(s, a.pos, b.pos);
            var rb = !ra;

            var safety = new LoopSafety(1000);

            while (en != null && safety) {
                en = en.Next;
                if (en == null) {
                    break;
                }
                p = en.Value;

                var aSide = pa == p.v1 || pa == p.v2;

                var vnext = 
                    aSide 
                    ? p.v1 == pa 
                        ? p.v2 
                        : p.v1
                    : p.v1 == pb 
                        ? p.v2 
                        : p.v1;

                if (aSide) {
                    pb = vnext;
                    //advance b

                    // wrong way
                    if (Geometry.IsClockwise(s, b.pos, vnext.pos) != rb) {
                        continue;
                    }

                    // crossover
                    if (Geometry.IsClockwise(s, vnext.pos, a.pos) != rb) {
                        s = a.pos;
                        result.Add(s);
                        en = portals.Last;

                        while (en!= null) {
                            if (en.Value.v1 == a || en.Value.v2 == a) {
                                en = en.Next;
                                break;
                            }
                            en = en.Previous;
                        }
                        if (en == null) {
                            break;
                        }
                        p = en.Value;
                        a = p.v1;
                        b = p.v2;
                        pa = a;
                        pb = b;
                        ra = Geometry.IsClockwise(s, a.pos, b.pos);
                        rb = !ra;
                        continue;
                    }
                    b = vnext;
                }
                else {
                    pa = vnext;
                    //advance a
                    var ra2 = Geometry.IsClockwise(s, a.pos, vnext.pos);

                    // wrong way
                    if (ra2 != ra) {
                        continue;
                    }

                    // crossover
                    var rs = Geometry.IsClockwise(s, vnext.pos, b.pos);
                    if (rs != ra) {
                        s = b.pos;
                        result.Add(s);

                        en = portals.Last;

                        while (en != null) {
                            if (en.Value.v1 == b || en.Value.v2 == b) {
                                en = en.Next;
                                break;
                            }
                            en = en.Previous;
                        }
                        if (en  == null) {
                            break;
                        }
                        p = en.Value;
                        a = p.v1;
                        b = p.v2;
                        pa = a;
                        pb = b;
                        ra = Geometry.IsClockwise(s, a.pos, b.pos);
                        rb = !ra;
                        continue;
                    }
                    a = vnext;
                }

            }

            // one more iteration with end
            //advance b
            // wrong way
            if (Geometry.IsClockwise(s, b.pos, end) != rb) {
                result.Add(b.pos);
            }

            // crossover
            if (Geometry.IsClockwise(s, end, a.pos) != rb) {
                result.Add(a.pos);
            }

            result.Add(end);
            return result;

        }

        public static float PathLength(List<Vector2> p) {
            float result = 0;
            for (int i = 0; i < p.Count - 1; i++) {
                result += (p[i] - p[i + 1]).magnitude;
            }
            return result;
        }
        class AStarCache {
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

            public float DistToEdge(DelaunayEdge e) {
                float result = 0;
                if (start) {
                    result =  Mathf.Sqrt(Mathf.Min((startPos - e.v1.pos).sqrMagnitude, (startPos - e.v2.pos).sqrMagnitude));
                }
                else {
                    result = 
                        Mathf.Sqrt(
                            Mathf.Min(
                                (entry.v1.pos - e.v1.pos).sqrMagnitude,
                                (entry.v1.pos - e.v2.pos).sqrMagnitude,
                                (entry.v2.pos - e.v1.pos).sqrMagnitude,
                                (entry.v2.pos - e.v2.pos).sqrMagnitude));
                }

                return result;
            }
        }
    }
}
