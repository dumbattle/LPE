using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LPE.Triangulation {
    public class Delaunay {
        public HashSet<DelaunayTriangle> triangles = new HashSet<DelaunayTriangle>();
        public Dictionary<Vector2, DelaunayVertex> vertices = new Dictionary<Vector2, DelaunayVertex>();

        float x1 = 999999;
        float x2 = -999999;
        float y1 = 999999;
        float y2 = -999999;
        public DelaunayVertex s1;
        public DelaunayVertex s2;
        public DelaunayVertex s3;

        // cahce
        HashSet<DelaunayTriangle> bad = new HashSet<DelaunayTriangle>();
        HashSet<DelaunayEdge> poly = new HashSet<DelaunayEdge>();
        Queue<DelaunayEdge> intersect = new Queue<DelaunayEdge>();
        List<DelaunayEdge> newEdges = new List<DelaunayEdge>();
        List<DelaunayVertex> orderedVerts = new List<DelaunayVertex>();


        //***************************************************************************************
        // Build Triangulation
        //***************************************************************************************
    
        void AddPoints(IEnumerable<Vector2> points) {
            HashSet<DelaunayVertex> newVerts = new HashSet<DelaunayVertex>();
        
            // ignore repeated vertices
            // resize bounds
            foreach (var p in points) {
                if (vertices.ContainsKey(p)) {
                    continue;
                }
                var v = new DelaunayVertex() { pos = p };
                vertices.Add(p, v);
                newVerts.Add(v);
                x1 = Mathf.Min(x1, v.x);
                x2 = Mathf.Max(x2, v.x);
                y1 = Mathf.Min(y1, v.y);
                y2 = Mathf.Max(y2, v.y);
            }

            if (newVerts.Count == 0) {
                return;
            }


            // super triangle
            float d = (x2 - x1) / 2;
            if (s1 == null) {
                s1 = new DelaunayVertex() { super = true };
                s2 = new DelaunayVertex() { super = true };
                s3 = new DelaunayVertex() { super = true };
                DelaunayTriangle super = DelaunayUtitiliy.CreateTriangle(s1, s2, s3);
                triangles.Add(super);
            }

            s1.pos = new Vector2(x2 + d + 1, y1 - 100);
            s2.pos = new Vector2(x1 - d - 1, y1 - 100);
            s3.pos = new Vector2(x2 - d, y2 + y2 - y1 + 100);

            int i = 0;
            // incremental
            foreach (var v in newVerts) {
                AddVertex(v);
            }
        }
        
        
        public void AddConstraints(params Vector2[] edges) {
            AddPoints(edges);

            for (int i = 0; i < edges.Length; i += 2) {
                DelaunayVertex v1 = vertices[edges[i]];
                DelaunayVertex v2 = vertices[edges[i + 1]];

                // check if constraint already exists => continue
                if (v1.ConnectedTo(v2)) {
                    DelaunayUtitiliy.GetOrCreateEdge(v1, v2).IsConstraint = true;
                    continue;
                }



                // find all intersecting edges
                // Walk
                // start triangle
                DelaunayTriangle t = null;
                DelaunayEdge inter = null;
                foreach (var e in v1.edges) {
                    // intersecting edges?
                    inter = GetIntersecting(e.t1,v1,v2).a;
                    if (inter != null) {
                        t = e.t1;
                        break;
                    }
                    inter = GetIntersecting(e.t2,v1,v2).a;
                    if (inter != null) {
                        t = e.t2;
                        break;
                    }

                    // edge is part of the constraint?
                    var vother = e.v1 == v1 ? e.v2 : e.v1;
                    if (Utility.OnSegment(vother.pos, v1.pos, v2.pos)) {
                        e.IsConstraint = true;
                        // restart using other vertex
                        AddConstraints(vother.pos, v2.pos);
                        break;
                    }
                }

                // find all edges
                var safety = new LoopSafety(1000);

                List<Vector2> newConstraints = null;
                intersect.Clear();
                while (inter != null && safety) {
                    if (inter.IsConstraint) {
                        throw new InvalidOperationException($"Overlaping constraints ({inter.v1.pos}, {inter.v2.pos}), ({v1.pos}, {v2.pos})");
                    }
                    intersect.Enqueue(inter);
                    var t1 = inter.t1;
                    var t2 = inter.t2;

                    t = t1 == t ? t2 : t1; // flip triangle


                    if (t.v1 == v2 || t.v2 == v2 || t.v3 == v2) {
                        break;
                    }
                    var (e1, e2) = GetIntersecting(t,v1,v2);
                    inter = inter == e1 ? e2 : e1; // next edge

                    if (inter == null && e1.v1 != v2 && e1.v2 != v2) {
                        // does walk path intersect a vertex?
                        var nv = Utility.OnSegment(t.v1.pos, v1.pos, v2.pos) ? t.v1 :
                                 Utility.OnSegment(t.v2.pos, v1.pos, v2.pos) ? t.v2 :
                                 Utility.OnSegment(t.v3.pos, v1.pos, v2.pos) ? t.v3 : null;
                         
                        if (nv != null) {
                            // split constraint into 2
                            newConstraints = newConstraints ?? new List<Vector2>();
                            newConstraints.Add(nv.pos);
                            newConstraints.Add(v2.pos);
                            v2 = nv;
                            break;
                        }
                    }
                }


                safety = new LoopSafety(10000);

                // flip intersecting edges
                newEdges.Clear();
                while (intersect.Count > 0 && safety) {
                    var e = intersect.Dequeue();
                    //   check if the 2 triangles are convex => if not put at back of queue
                    if (!IsConvex(e.t1, e.t2)) {
                        intersect.Enqueue(e);
                        continue;
                    }
                    //   swap edge
                    e = FlipEdge(e);
                    if ((e.v1 == v1 && e.v2 == v2) || (e.v1 == v2 && e.v2 == v1)) {
                        e.IsConstraint = true;
                        // is constraint
                        continue;
                    }

                    if (Utility.IsIntersecting(e.v1.pos, e.v2.pos, v1.pos, v2.pos)) {
                        intersect.Enqueue(e);
                    }
                    else {
                        newEdges.Add(e);
                    }
                }

                safety = new LoopSafety(1000);

                bool swap = true;

                // fix new edges
                while(swap && safety.Inc()) {
                    swap = false;

                    for (int j = 0; j < newEdges.Count; j++) {
                        DelaunayEdge e = newEdges[j];
                        if (IsDelaunay(e)) {
                            continue;
                        }

                        e = FlipEdge(e);
                        newEdges[j] = e;
                        swap = true;
                    }
                }
        



                if (newConstraints != null) {
                    AddConstraints(newConstraints.ToArray());
                }




                (DelaunayEdge a, DelaunayEdge b) GetIntersecting(DelaunayTriangle t, DelaunayVertex a, DelaunayVertex b) {
                    DelaunayEdge ra = null;
                    DelaunayEdge rb = null;
                    if (Utility.IsIntersecting(t.e1.v1.pos, t.e1.v2.pos, a.pos, b.pos)) {
                        rb = t.e1;
                    }
                    if (ra == null) {
                        ra = rb;
                        rb = null;
                    }

                    if (Utility.IsIntersecting(t.e2.v1.pos, t.e2.v2.pos, a.pos, b.pos)) {
                        rb = t.e2;
                    }
                    if (ra == null) {
                        ra = rb;
                        rb = null;
                    }
                    if (Utility.IsIntersecting(t.e3.v1.pos, t.e3.v2.pos, a.pos, b.pos)) {
                        rb = t.e3;
                    }
                    if (ra == null) {
                        ra = rb;
                        rb = null;
                    }

                    return (ra, rb);
                }

            }
        }



        public void RemovePoints(params Vector2[] points) {
            foreach (var p in points) {
                var v = vertices[p];
                if (v == null) {
                    continue;
                }
                vertices.Remove(p);
                // get triangles around
                bad.Clear();
                foreach (var e in v.edges) {
                    var t1 = e.t1;
                    var t2 = e.t2;

                    bad.Add(t1);
                    bad.Add(t2);
                }
                // get polygon around point
                poly.Clear();
                foreach (var t in bad) {
                    bool e1 = true;
                    bool e2 = true;
                    bool e3 = true;
                    foreach (var t2 in bad) {
                        if (t == t2) {
                            continue;
                        }

                        if (DelaunayUtitiliy.EdgeCmp(t.e1, t2.e1) ||
                            DelaunayUtitiliy.EdgeCmp(t.e1, t2.e2) ||
                            DelaunayUtitiliy.EdgeCmp(t.e1, t2.e3)) {
                            e1 = false;
                        }
                        if (DelaunayUtitiliy.EdgeCmp(t.e2, t2.e1) ||
                            DelaunayUtitiliy.EdgeCmp(t.e2, t2.e2) ||
                            DelaunayUtitiliy.EdgeCmp(t.e2, t2.e3)) {
                            e2 = false;
                        }
                        if (DelaunayUtitiliy.EdgeCmp(t.e3, t2.e1) ||
                            DelaunayUtitiliy.EdgeCmp(t.e3, t2.e2) ||
                            DelaunayUtitiliy.EdgeCmp(t.e3, t2.e3)) {
                            e3 = false;
                        }
                    }

                    if (e1) {
                        poly.Add(t.e1);
                    }
                    if (e2) {
                        poly.Add(t.e2);
                    }
                    if (e3) {
                        poly.Add(t.e3);
                    }
                }

                // remove triangles
                foreach (var t in bad) {
                    triangles.Remove(t);
                    DelaunayUtitiliy.RemoveTriangle(t);
                }

                // order verts
                orderedVerts.Clear();
                //Debug.Log(poly.Count);
                var f = poly.First();

                if (orderedVerts.Count == 0) {
                    orderedVerts.Add(f.v1);
                    orderedVerts.Add(f.v2);
                }
                poly.Remove(f);

                var safety = new LoopSafety(1000);
                while (poly.Count > 0 && safety.Inc()) {
                    var last = orderedVerts[orderedVerts.Count - 1];
                    foreach (var e in poly) {
                        if (e.v1 == last) {
                            orderedVerts.Add(e.v2);
                        }
                        else if (e.v2 == last) {
                            orderedVerts.Add(e.v1);
                        }
                        else {
                            // no match - keep going
                            continue;
                        }

                        poly.Remove(e);
                        // match - stop
                        break;
                    }
                }

                // last vert is same as first
                orderedVerts.RemoveAt(orderedVerts.Count - 1);



                safety = new LoopSafety(1000);

                //Debug.Log(orderedVerts.Count);
                //foreach (var vert in orderedVerts) {
                //    Debug.Log(vert.pos);
                //}
                while (orderedVerts.Count > 3 && safety.Inc()) {
                    var count = orderedVerts.Count;

                    for (int i = 0; i < count; i++) {
                        var v1 = orderedVerts[i];
                        var v2 = orderedVerts[(i + 1) % count];
                        var v3 = orderedVerts[(i + 2) % count];
                        bool valid = true;

                        // check concavity                        
                        if (v1.ConnectedTo(v3)) {
                            continue;
                        }

                        foreach (var e in v2.edges) {
                            var d = e.v1 == v2 ? e.v2 : e.v1;
                            if(d == v1 || d == v3) {
                                continue;
                            }

                            if(Utility.RaySegment(v2.pos, d.pos - v2.pos, v1.pos, v3.pos)) {
                                valid = false;
                                break;
                            }
                        }
                        if (!valid) {
                            continue;
                        }



                        // check circumcircle

                        for (int j = 0; j < count; j++) {
                            if (j == 0 || j == (i + 1) % count || j == (i + 2) % count) {
                                continue;
                            }
                            var vert = orderedVerts[j];

                            if (Utility.InCircumcircle(vert.pos, v1.pos, v2.pos, v3.pos)) {
                                valid = false;
                                break;
                            }
                        }

                        if (valid) {
                            var t = DelaunayUtitiliy.CreateTriangle(v1, v2, v3);
                            triangles.Add(t);
                            orderedVerts.RemoveAt((i + 1) % count);
                            break;
                        }
                    }
                }
                //Debug.Log(orderedVerts.Count);

                //foreach (var vert in orderedVerts) {
                //    Debug.Log(vert.pos);
                //}
                var tri = DelaunayUtitiliy.CreateTriangle(orderedVerts[0], orderedVerts[1], orderedVerts[2]);
                triangles.Add(tri);
            }
        }

        //---------------------------------------------------------------------------------------
        // Overloads
        //---------------------------------------------------------------------------------------
        public void AddPoints(params Vector2[] points) {
            AddPoints((IEnumerable<Vector2>)points);
        }
        public void AddPoints(List<Vector2> points) {
            AddPoints((IEnumerable<Vector2>)points);
        }

        public void AddConstraints(List<Vector2> edges) {
            AddConstraints(edges.ToArray());
        }
        //---------------------------------------------------------------------------------------
        // Helper Functions
        //---------------------------------------------------------------------------------------

        void AddVertex(DelaunayVertex v) {
            bad.Clear();
            poly.Clear();



            // create polygon and bad set
            bad.Clear();
            LinkedList<DelaunayEdge> pl = new LinkedList<DelaunayEdge>();
            var st = this.Point2Triangle(v.pos);
            bad.Add(st);
            pl.AddLast(st.e1);
            pl.AddLast(st.e2);
            pl.AddLast(st.e3);

            var n = pl.First;

            var safety = new LoopSafety(10000);
            while (n != null && safety) {
                var e = n.Value;
                // stop at constrained edges
                if (e.IsConstraint) {
                    n = n.Next;
                    continue;
                }

                // get other triangle
                var t1 = e.t1;
                var t2 = e.t2;

                var t = bad.Contains(t1) ? t2 : t1;

                // check if other triangle is bad
                if (t != null && t.InCircumcircle(v)) {
                    bad.Add(t);
                    var e1 = t.e1;
                    var e2 = t.e2;
                    var e3 = t.e3;

                    void AddToPoly(LinkedList<DelaunayEdge> pl, DelaunayEdge c, DelaunayEdge e) {
                        // not current edge
                        if (e != c) {
                            var n1 = pl.Find(e);

                            if (n1 == null) {
                                // add
                                pl.AddLast(e);
                            }
                            else {
                                // if already part of poly it will be covered
                                pl.Remove(n1);
                            }
                        }
                    }
                    AddToPoly(pl, e, e1);
                    AddToPoly(pl, e, e2);
                    AddToPoly(pl, e, e3);

                    // remove current edge
                    var n2 = n.Next;
                    pl.Remove(n);
                    n = n2;
                    continue;
                }
                // current edge is part of polys
                var next = n.Next;
                n = next;

            }




            // remove bad triangles
            foreach (var t in bad) {
                triangles.Remove(t);
                DelaunayUtitiliy.RemoveTriangle(t);
            }

            // fix polygon hole
            foreach (var e in pl) {
                var t = DelaunayUtitiliy.CreateTriangle(e.v1, e.v2, v);
                triangles.Add(t);
            }

        }

        DelaunayEdge FlipEdge(DelaunayEdge e) {
            // get opposite vertices
            DelaunayVertex a, b, c;
            DelaunayVertex v1;
            DelaunayVertex v2;
            DelaunayVertex v3 = e.v1;
            DelaunayVertex v4 = e.v2;

            a = e.t1.v1;
            b = e.t1.v2;
            c = e.t1.v3;
            if (a == e.v1 || a == e.v2) a = null;
            if (b == e.v1 || b == e.v2) b = null;
            if (c == e.v1 || c == e.v2) c = null;
            v1 = a ?? b ?? c;

            a = e.t2.v1;
            b = e.t2.v2;
            c = e.t2.v3;
            if (a == e.v1 || a == e.v2) a = null;
            if (b == e.v1 || b == e.v2) b = null;
            if (c == e.v1 || c == e.v2) c = null;
            v2 = a ?? b ?? c;

            // remove old triangles
            triangles.Remove(e.t1);
            triangles.Remove(e.t2);
            DelaunayUtitiliy.RemoveTriangle(e.t1);
            DelaunayUtitiliy.RemoveTriangle(e.t2);

            //vreate new triangles
            var t1 = DelaunayUtitiliy.CreateTriangle(v1, v2, v3);
            var t2 = DelaunayUtitiliy.CreateTriangle(v1, v2, v4);

            triangles.Add(t1);
            triangles.Add(t2);

            return DelaunayUtitiliy.GetOrCreateEdge(v1, v2);
        }
        
        static void FindIntersectingEdges(DelaunayVertex a, DelaunayVertex b, Queue<DelaunayEdge> result) {
            // start triangle
            DelaunayTriangle t = null;
            DelaunayEdge inter = null;
            foreach (var e in a.edges) {
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
            }

            // find all edges
            var safety = new LoopSafety(1000);

            while(inter != null && safety.Inc()) {
                if(inter.IsConstraint) {
                    throw new InvalidOperationException($"Overlaping constraints ({inter.v1.pos}, {inter.v2.pos}), ({a.pos}, {b.pos})");
                }
                result.Enqueue(inter);

                var t1 = inter.t1;
                var t2 = inter.t2;

                t = t1 == t ? t2 : t1; // flip triangle

                if (t.v1 == a || t.v2 == a || t.v3 == a) {
                    break;
                }
                var (e1, e2) = GetIntersecting(t);
                inter = inter == e1 ? e2 : e1; // next edge

            }

            (DelaunayEdge a, DelaunayEdge b) GetIntersecting (DelaunayTriangle t) {
                DelaunayEdge ra = null;
                DelaunayEdge rb = null;
                if (Utility.IsIntersecting(t.e1.v1.pos, t.e1.v2.pos, a.pos, b.pos)) {
                    rb = t.e1;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }

                if (Utility.IsIntersecting(t.e2.v1.pos, t.e2.v2.pos, a.pos, b.pos)) {
                    rb = t.e2;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }
                if (Utility.IsIntersecting(t.e3.v1.pos, t.e3.v2.pos, a.pos, b.pos)) {
                    rb = t.e3;
                }
                if (ra == null) {
                    ra = rb;
                    rb = null;
                }

                return (ra, rb);
            }
        }

        static bool IsConvex(DelaunayTriangle a, DelaunayTriangle b) {
            DelaunayVertex v1 = a.v1;
            DelaunayVertex v2 = a.v2;
            DelaunayVertex v3 = a.v3;
            DelaunayVertex v4 = b.v1;

            if (v4 == v1 || v4 == v2 || v4 == v3) {
                v4 = b.v2;
                if (v4 == v1 || v4 == v2 || v4 == v3) {
                    v4 = b.v3;
                }
            }

            return IsConvex(v1, v2, v3, v4);
        }
        static bool IsConvex(DelaunayVertex a, DelaunayVertex b, DelaunayVertex c, DelaunayVertex d) {
            bool abc = DelaunayTriangle.IsClockwise(a, b, c);
            bool abd = DelaunayTriangle.IsClockwise(a, b, d);
            bool bcd = DelaunayTriangle.IsClockwise(b, c, d);
            bool cad = DelaunayTriangle.IsClockwise(c, a, d);

            if ((abc && abd && bcd & !cad) ||
                (abc && abd && !bcd & cad) ||
                (abc && !abd && bcd & cad) ||
                (!abc && !abd && !bcd & cad) ||
                (!abc && !abd && bcd & !cad) ||
                (!abc && abd && !bcd & !cad)) {
                return true;
            }

            return false;
        }

        static bool IsDelaunay(DelaunayEdge e) {

            // get opposite vertices
            DelaunayVertex a, b, c;
            DelaunayVertex v1, v2;

            a = e.t1.v1;
            b = e.t1.v2;
            c = e.t1.v3;
            if (a == e.v1 || a == e.v2) a = null;
            if (b == e.v1 || b == e.v2) b = null;
            if (c == e.v1 || c == e.v2) c = null;
            v1 = a ?? b ?? c;

            a = e.t2.v1;
            b = e.t2.v2;
            c = e.t2.v3;
            if (a == e.v1 || a == e.v2) a = null;
            if (b == e.v1 || b == e.v2) b = null;
            if (c == e.v1 || c == e.v2) c = null;
            v2 = a ?? b ?? c;


            return !e.t2.InCircumcircle(v1) && !e.t1.InCircumcircle(v2);
        }


    }

}
