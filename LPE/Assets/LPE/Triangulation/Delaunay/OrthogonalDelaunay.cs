using LPE;
using LPE.Math;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LPE.Triangulation {
    public interface IReadOnly_OrthogonalDelaunay {
        void GetTriangles(Vector2 point, HashSet<TriangleID> results);
        /// <summary>
        /// Contains neighbors, edges, and vertices
        /// Neighbors are in CCW order
        /// portals and vertices are also in CCW order
        /// p1 is portal (v1-v2) to n1, etc
        /// </summary>
        DelaunayTriangleData GetTriangleData(TriangleID triangleID, bool allowConstraintCrossing);
        Vector2 GetCenter(TriangleID triangleID);
        bool IsValidTriangle(TriangleID triangleID);


    }

    public class OrthogonalDelaunay : IReadOnly_OrthogonalDelaunay {
        /*
         *      Vertices always CCW
         *      neighbors always aligned
         *               v0
         *             /    \
         *         n0 /      \   n2  
         *           /        \
         *         v1----------v2 
         *                n1
         */
        // --- Vertices ---
        List<Vector2Int> vertexPos = new();
        List<bool> vertexValid = new();
        List<int> vertexNumConstraints = new(); 
        List<int> vertexTriangleHead = new();

        Stack<int> availableVertexSlots = new();

        // --- Triangles ---
        (int v0, int v1, int v2)[]  triangleVerts = { }; // vertex indexc
        (int n0, int n1, int n2)[] triangleNeighbors = { }; // vertex indexc



        List<int> triangleID = new(); // external handle public

        List<bool> triangleValid = new();
        Stack<int> availableTriangleSlots = new();


        // --- Constraints ---
    
        HashSet<PackedEdge> constrainedEdges = new();

        Dictionary<PackedEdge, int> constraintToDivide = new();

        // --- Cache ---
        List<int> _triangleSeenCache = new List<int>();
        int _triangleSeenGeneration = 1;

        int lastQueriedTriangle = -1;
        int lastAddedTri = -1;

        public OrthogonalDelaunay() {
            // super triangle vertices

            var v0 = ClaimVertexSlot(new Vector2Int(0, 1 << 20));
            var v1 = ClaimVertexSlot(new Vector2Int(-(1 << 20), -(1 << 20)));
            var v2 = ClaimVertexSlot(new Vector2Int(+(1 << 20), -(1 << 20)));


            // create the single super triangle
            var t = ClaimTriangle(v0, v1, v2);
        }

        public void DebugLog() {
            void Log<T>(string label, List<T> l) {
                Debug.Log(label + l.ContentString());
            }

            Log("vertexPos", vertexPos);
        }
        //************************************************************************************************
        #region Construction
        //************************************************************************************************

        public void AddPoint(Vector2Int point) {
            //
            using (var enclosingTScope = ObjectPool.GetScopedList<int>()) {
                var enclosingResuls = GetEnclosingElement(point);

                if (enclosingResuls.isVertex) {
                    return;
                }


                if (enclosingResuls.isEdge) {
                    // on edge, split
                    var ev1 = enclosingResuls.ev1;
                    var ev2 = enclosingResuls.ev2;

                    if (constrainedEdges.Contains(PackEdge(ev1, ev2))) {
                        RemoveConstraint(vertexPos[ev1], vertexPos[ev2]);
                        AddConstraint(vertexPos[ev1], point);
                        AddConstraint(vertexPos[ev2], point);
                        return;
                    }
                }
                // TODO
                var enclosingTriangles = enclosingTScope.item;

                enclosingTriangles.Add(enclosingResuls.triangleIndex);

                if (enclosingResuls.isEdge) {
                    enclosingTriangles.Add(GetOtherHalfEdge(enclosingResuls).triangleIndex);
                }

                // write vertext
                int newVert = ClaimVertexSlot(point);
               
                // collect bad triangles
                var badTriangles = ObjectPool.GetHashSet<int>();
                GetBadTriangles(point, enclosingTriangles, badTriangles);

                // Find Cavity
                var cavityEdges = ObjectPool.GetList<(int, int)>();
                var edge2Neighbor = ObjectPool.GetDictionary<PackedEdge, int>();
                var v2next = ObjectPool.GetDictionary<int, int>();
                GetCavity(badTriangles, cavityEdges, edge2Neighbor, v2next);

                // write triangles
                var newTriangles = ObjectPool.GetList<int>();
                ClaimNewTriangles(newVert, cavityEdges, v2next, newTriangles);

                // remve bad triangles
                RemoveBadTriangles(badTriangles);

                // write neighbors
                UpdateNeighbors(edge2Neighbor, newTriangles);
              

                ObjectPool.ReturnList(cavityEdges);
                ObjectPool.ReturnHashSet(badTriangles);
                ObjectPool.ReturnList(newTriangles);
                ObjectPool.ReturnDictionary(v2next);
                ObjectPool.ReturnDictionary(edge2Neighbor);
            }



            void GetBadTriangles(Vector2Int point, List<int> seed, HashSet<int> badTriangles) {
                _triangleSeenGeneration++;
                var candidates = ObjectPool.GetStack<int>(); // for flood fill


                foreach (var t in seed) {
                    candidates.Push(t);
                    _triangleSeenCache[t] = _triangleSeenGeneration;
                }

                while (candidates.Count > 0) {
                    var tInd = candidates.Pop();
                    if (tInd < 0) {
                        continue;
                    }

                    var (v0, v1, v2) = triangleVerts[tInd];
                    //var v0 = triangleV0[tInd];
                    //var v1 = triangleV1[tInd];
                    //var v2 = triangleV2[tInd];

                    var p0 = vertexPos[v0];
                    var p1 = vertexPos[v1];
                    var p2 = vertexPos[v2];

                    var cc = Geometry.InCircumcircle(point, p0, p1, p2);

                    if (cc) {
                        badTriangles.Add(tInd);

                        var (n0, n1, n2) = triangleNeighbors[tInd];


                        PackedEdge pe0 = PackEdge(v0, v1);
                        PackedEdge pe1 = PackEdge(v2, v0);
                        PackedEdge pe2 = PackEdge(v1, v2);
                        
                        bool c0 = constrainedEdges.Contains(pe0);
                        bool c2 = constrainedEdges.Contains(pe1);
                        bool c1 = constrainedEdges.Contains(pe2);

                        if (n0 >= 0 && _triangleSeenCache[n0] != _triangleSeenGeneration && !c0) {
                            candidates.Push(n0);
                            _triangleSeenCache[n0] = _triangleSeenGeneration;
                        }
                        if (n1 >= 0 && _triangleSeenCache[n1] != _triangleSeenGeneration && !c1) {
                            candidates.Push(n1);
                            _triangleSeenCache[n1] = _triangleSeenGeneration;
                        }
                        if (n2 >= 0 && _triangleSeenCache[n2] != _triangleSeenGeneration && !c2) {
                            _triangleSeenCache[n2] = _triangleSeenGeneration;
                            candidates.Push(n2);
                        }
                    }
                }

                ObjectPool.ReturnStack(candidates);
            }


            void ClaimNewTriangles(int newVert, List<(int, int)> cavityEdges, Dictionary<int, int> v2next, List<int> newTriangles) {
                int startV = cavityEdges[0].Item1;
                int currentV = startV;
                while (true) {
                    int nextV = v2next[currentV];
                    var t = ClaimTriangle(currentV, nextV, newVert);
                    newTriangles.Add(t);

                    currentV = nextV;
                    if (currentV == startV) {
                        break;
                    }
                }
            }

            void UpdateNeighbors(Dictionary<PackedEdge, int> edge2Neighbor, List<int> newTriangles) {
                var newCount = newTriangles.Count;
                for (int i = 0; i < newCount; i++) {
                    // new triangle
                    int t = newTriangles[i];
                    var (v0, v1, _) = triangleVerts[t];

                    var eBoundary = PackEdge(v0, v1);
                    //var eBoundary = PackEdge(triangleV0[t], triangleV1[t]);
                    int oldNeighbor = edge2Neighbor.TryGetValue(eBoundary, out var n) ? n : -1;

                    triangleNeighbors[t] = (
                        oldNeighbor,
                        newTriangles[(i + 1) % newCount],
                        newTriangles[(i - 1 + newCount) % newCount]);

                    
                    if (oldNeighbor >= 0) {
                        // existing neighbor
                        ReplaceNeighborWithSelf(GetTriangleEdgeElement(t, v0));
                        //ReplaceNeighborWithSelf(GetTriangleEdgeElement(t, triangleV0[t]));
                    }
                }
            }
        }

        public void AddConstraint(Vector2Int p1, Vector2Int p2) {
            // add points
            AddPoint(p1);
            AddPoint(p2);

            // get vertices
            var v1 = GetEnclosingElement(p1);
            var v2 = GetEnclosingElement(p2);


            if (!v1.isVertex || !v2.isVertex) {
                // this should never occur 
                // except maybe due to rounding error?
                throw new System.Exception("something went wrong");
            }

            AddConstraint(v1, v2);
        }

        public void RemoveConstraint(Vector2Int p1, Vector2Int p2) {
                var safety = new LoopSafety(100);
                safety.SetLog();

                // get vertices
                var v1 = GetEnclosingElement(p1);
                var v2 = GetEnclosingElement(p2);
                var vidx1 = v1.vertexIndex;
                var vidx2 = v2.vertexIndex;

                if (!v1.isVertex || !v2.isVertex) {
                    // this should never occur 
                    // except maybe due to rounding error?
                    throw new System.Exception("INVALID CONSTRAINT REMOVAL A");
                }

                var packed = PackEdge(vidx1, vidx2);
                if (!constrainedEdges.Contains(packed)) {
                    if (constraintToDivide.TryGetValue(packed, out int vMid)) {
                        RemoveConstraint(vertexPos[v1.vertexIndex], vertexPos[vMid]);
                        RemoveConstraint(vertexPos[v2.vertexIndex], vertexPos[vMid]);
                        constraintToDivide.Remove(packed);
                        return;
                    }
                    throw new System.InvalidOperationException("INVALID CONSTRAINT REMOVAL B");
                }

                constrainedEdges.Remove(packed);
                TriangleElement targetE1 = TriangleElement.INVALID;
                TriangleElement targetE2 = TriangleElement.INVALID;

                // find edge
                (targetE1, targetE2) = GetEdgeElement(vidx1, vidx2);


                triangleID[targetE1.triangleIndex]++;
                triangleID[targetE2.triangleIndex]++;

                vertexNumConstraints[vidx1]--;
                vertexNumConstraints[vidx2]--;

                bool remove1 = vertexNumConstraints[vidx1] == 0;
                bool remove2 = vertexNumConstraints[vidx2] == 0;


            if (remove1) RemovePoint(vertexPos[vidx1]);
            if (remove2) RemovePoint(vertexPos[vidx2]);

            if (!remove1 && !remove2) {
                var candidates = ObjectPool.GetQueue<(int, int)>();

                // starting edge
                candidates.Enqueue((vidx1, vidx2));

                RestoreDelaunay(candidates);

                ObjectPool.ReturnQueue(candidates);
            }
        }

        public void RemovePoint(Vector2Int p) {
            var safety = new LoopSafety(10000);



            // get vertex
            var encResults = GetEnclosingElement(p);


            int v = encResults.vertexIndex;

            if (v < 0) {
                return;
            }

            // get bad triangles
            var badTriangles = ObjectPool.GetHashSet<int>();


            var startingTri = encResults.triangleIndex;
            var currentTri = startingTri;
            while (true) {
                badTriangles.Add(currentTri);

                currentTri = WalkVertex(v, currentTri);

                if (currentTri < 0) {
                    throw new Exception();
                }
                if (currentTri == startingTri) {
                    break;
                }
            }

            // get cavity

            var cavityEdges = ObjectPool.GetList<(int, int)>();
            var edge2Neighbor = ObjectPool.GetDictionary<PackedEdge, int>();
            var v2next = ObjectPool.GetDictionary<int, int>();
            GetCavity(badTriangles, cavityEdges, edge2Neighbor, v2next);

            // cavity as vertices
            var cavityVerts = ObjectPool.GetList<int>();
            int start = cavityEdges[0].Item1;
            int vcur = start;

            do {
                cavityVerts.Add(vcur);
                vcur = v2next[vcur];
            }
            while (vcur != start);



            // create new edges (earclip)
            var newEdges = ObjectPool.GetList<TriangleElement>();
            var newTriangles = ObjectPool.GetList<int>();
            var buffer = ObjectPool.GetList<int>();
            int remaining = cavityVerts.Count - 2; // how many triangles to create


            while (safety.Next()) {
                if (remaining <= 0) {
                    break;
                }
                for (int i = 0; i < cavityVerts.Count; i++) {
                    if (remaining <= 0) {
                        break;
                    }
                    var vp = buffer.Count > 0 ? buffer[buffer.Count - 1] : cavityVerts[cavityVerts.Count - 1]; // don't use vertices that are no longer part of boundary
                    var vc = cavityVerts[i];
                    var vn = cavityVerts[(i + 1) % cavityVerts.Count];
                    bool nbuf = i == cavityVerts.Count - 1 && buffer.Count > 0;

                    if (nbuf) {
                        vn = buffer[0];
                    }

                    // this part of the cavity is concave, skip for now
                    if (!Geometry.IsCounterClockwise(vertexPos[vp], vertexPos[vc], vertexPos[vn])) {
                        buffer.Add(vc);
                        continue;
                    }


                    static bool sign(Vector2Int p1, Vector2Int p2, Vector2Int p3) {
                        return ((long)p1.x - p3.x) * ((long)p2.y - p3.y) >= ((long)p2.x - p3.x) * ((long)p1.y - p3.y);
                    }


                    bool valid = true;

                    for (int j = (nbuf ? 1 : 0); j < buffer.Count - 1; j++) {
                        var jp = vertexPos[buffer[j]];

                        Vector2Int pp = vertexPos[vp];
                        Vector2Int pc = vertexPos[vc];
                        Vector2Int pn = vertexPos[vn];

                        if (sign(jp, pp, pc) && sign(jp, pc, pn) && sign(jp, pn, pp)) {
                            valid = false;
                            break;
                        }
                    }


                    if (!valid) {
                        buffer.Add(vc);
                        continue;
                    }

                    for (int j = i + 2; j < cavityVerts.Count; j++) {
                        var jp = vertexPos[cavityVerts[j]];

                        Vector2Int pp = vertexPos[vp];
                        Vector2Int pc = vertexPos[vc];
                        Vector2Int pn = vertexPos[vn];
                        if (sign(jp, pp, pc) && sign(jp, pc, pn) && sign(jp, pn, pp)) {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid) {
                        buffer.Add(vc);
                        continue;
                    }


                    remaining--;

                    var t = ClaimTriangle(vp, vc, vn);
                    newEdges.Add(TriangleElement.AsEdge(t, vn, vp));
                    newTriangles.Add(t);


                    // update neighbors
                    var epc = PackEdge(vp, vc);
                    var ecn = PackEdge(vc, vn);
                    var enp = PackEdge(vn, vp);

                    ref var tn = ref triangleNeighbors[t];

                    if (edge2Neighbor.TryGetValue(epc, out int oldNeighbor)) {
                        // this is on the edge of the cavity, update neighbors
                        tn.n0 = oldNeighbor;
                        ReplaceNeighborWithSelf(TriangleElement.AsEdge(t, vp, vc));
                        edge2Neighbor.Remove(epc);
                    }
                    else {
                        // this is inside the cavity, and will form part of the new boundary
                        edge2Neighbor.Add(epc, t);
                        tn.n0 = -1;
                    }

                    if (edge2Neighbor.TryGetValue(ecn, out oldNeighbor)) {
                        tn.n1 = oldNeighbor;
                        ReplaceNeighborWithSelf(TriangleElement.AsEdge(t, vc, vn));
                        edge2Neighbor.Remove(ecn);
                    }
                    else {
                        edge2Neighbor.Add(ecn, t);
                        tn.n1 = -1;
                    }

                    if (edge2Neighbor.TryGetValue(enp, out oldNeighbor)) {
                        tn.n2 = oldNeighbor;
                        ReplaceNeighborWithSelf(TriangleElement.AsEdge(t, vn, vp));
                        edge2Neighbor.Remove(enp);
                    }
                    else {
                        edge2Neighbor.Add(enp, t);
                        tn.n2 = -1;
                    }
                }

                var temp = cavityVerts;
                cavityVerts = buffer;
                buffer = temp;
                buffer.Clear();
            }


            RemoveBadTriangles(badTriangles);

            // remove vertex
            availableVertexSlots.Push(v);
            vertexValid[v] = false;

            // flip bad edges
            safety.SetLog();
            var candidates = ObjectPool.GetQueue<(int, int)>();
            for (int i = 0; i < newEdges.Count - 1; i++) { // ignore last, as it will either be a duplicate or an outer edge
                TriangleElement e = newEdges[i];
                candidates.Enqueue((e.ev1, e.ev2));
            }

            RestoreDelaunay(candidates);

            ObjectPool.ReturnQueue(candidates);
            ObjectPool.ReturnList(newTriangles);
            ObjectPool.ReturnList(buffer);
            ObjectPool.ReturnList(cavityVerts);
            ObjectPool.ReturnList(newEdges);
            ObjectPool.ReturnList(cavityEdges);
            ObjectPool.ReturnDictionary(v2next);
            ObjectPool.ReturnDictionary(edge2Neighbor);
            ObjectPool.ReturnHashSet(badTriangles);
        }

        #endregion
        //************************************************************************************************
        #region Pathfinding API
        //************************************************************************************************

        public void GetTriangles(Vector2 point, HashSet<TriangleID> results) {
            var enc = GetEnclosingElement(point);

            if (enc.isVertex) {
                if (enc.vertexIndex < 3) {
                    // if super, walk both directions
                    // realistically shouldn't happen
                    var start = enc.triangleIndex;
                    var current = start;
                    var safety = new LoopSafety(100);
                    safety.SetException();

                    while (true) {
                        safety.Next();

                        results.Add(new TriangleID(triangleID[current], current));
                        current = WalkVertex(enc.vertexIndex, current);

                        if (current < 0) {
                            break;
                        }
                    }
                    //
                    start = enc.triangleIndex;
                    current = start;

                    while (true) {
                        safety.Next();

                        results.Add(new TriangleID(triangleID[current], current));
                        current = WalkVertexReverse(enc.vertexIndex, current);

                        if (current < 0) {
                            break;
                        }
                    }
                }
                else {
                    var start = enc.triangleIndex;
                    var current = start;
                    var safety = new LoopSafety(100);
                    safety.SetException();

                    while (true) {
                        safety.Next();

                        results.Add(new TriangleID(triangleID[current], current));
                        current = WalkVertex(enc.vertexIndex, current);

                        if (current == start) {
                            break;
                        }
                    }
                }
            }
            else if (enc.isEdge) {
                var opposite = GetOtherHalfEdge(enc);

                var ti = enc.triangleIndex;
                var to = opposite.triangleIndex;

                results.Add(new TriangleID(triangleID[ti], ti));
                results.Add(new TriangleID(triangleID[to], to));
            }
            else if (enc.isTriangle){
                results.Add(new TriangleID(triangleID[enc.triangleIndex], enc.triangleIndex));
            }
            else {
                throw new Exception("Something went wrong");
            }
        }

        public DelaunayTriangleData GetTriangleData(TriangleID triangleID, bool allowConstraintCrossing = true) {
            var t = triangleID.index;

            var (n1, n2, n3) = triangleNeighbors[t];
            var (v1, v2, v3) = triangleVerts[t];

            var vp1 = vertexPos[v1];
            var vp2 = vertexPos[v2];
            var vp3 = vertexPos[v3];

            TriangleID r1, r2, r3;
            if (allowConstraintCrossing) {
                r1 = n1 < 0 ? TriangleID.INVALID : new TriangleID(this.triangleID[n1], n1);
                r2 = n2 < 0 ? TriangleID.INVALID : new TriangleID(this.triangleID[n2], n2);
                r3 = n3 < 0 ? TriangleID.INVALID : new TriangleID(this.triangleID[n3], n3);
            }
            else {
                r1 = n1 < 0 || constrainedEdges.Contains(PackEdge(v1, v2)) ? TriangleID.INVALID : new TriangleID(this.triangleID[n1], n1);
                r2 = n2 < 0 || constrainedEdges.Contains(PackEdge(v2, v3)) ? TriangleID.INVALID : new TriangleID(this.triangleID[n2], n2);
                r3 = n3 < 0 || constrainedEdges.Contains(PackEdge(v3, v1)) ? TriangleID.INVALID : new TriangleID(this.triangleID[n3], n3);
            }

            return new DelaunayTriangleData(r1, r2, r3, vp1, vp2, vp3);
        }

        public Vector2 GetCenter(TriangleID triangleID) {
            return TriangleCenter(triangleID.index);
        }

        public bool IsValidTriangle(TriangleID triangleID) {
            return triangleValid[triangleID.index] && this.triangleID[triangleID.index] == triangleID.id;
        }

        #endregion
        //************************************************************************************************
        #region Other API
        //************************************************************************************************

        public void GetVerticesUnchecked(TriangleID id, out Vector2Int v1, out Vector2Int v2, out Vector2Int v3) {
            var (i1, i2, i3) = triangleVerts[id.index];
            v1 = vertexPos[i1];
            v2 = vertexPos[i2];
            v3 = vertexPos[i3];
        }


        #endregion
        //************************************************************************************************
        #region Internal
        //************************************************************************************************

        void RestoreDelaunay(Queue<(int, int)> candidates, bool debug=false) {
            var inQueue = ObjectPool.GetHashSet<PackedEdge>();
            foreach (var (a, b) in candidates) {
                inQueue.Add(PackEdge(a, b));
            }

            var safety = new LoopSafety(1000);
            safety.SetException();

            while (candidates.Count > 0) {
                safety.Next();
                // next edge
                var (ev1, ev2) = candidates.Dequeue();
                inQueue.Remove(PackEdge(ev1, ev2));

                // constrained, don't flip
                if (constrainedEdges.Contains(PackEdge(ev1, ev2))) {
                    continue;
                }

                // Get Edge Element
                var (e1, e2) = GetEdgeElement(ev1, ev2);

                // on edge
                if (e1.triangleIndex < 0 || e2.triangleIndex < 0) {
                    continue;
                }

                // Label all vertices
                var vo1 = GetOppositeVertex(e1);
                var vo2 = GetOppositeVertex(e2);

                var pe1 = vertexPos[ev1];
                var pe2 = vertexPos[ev2];
                var pvo1 = vertexPos[vo1];
                var pvo2 = vertexPos[vo2];

                // if bad edge
                if (Geometry.InCircumcircle(pvo1, pe1, pe2, pvo2)) {
                    if (!EdgeIsFlippable(e1, e2)) {
                        // not flippable now
                        // if an adjacent edge is flipped, it will add this one back
                        continue;
                    }

                    // flip
                    var (ne1, ne2) = FlipEdge(e1, e2);

                    // new triangles 
                    var t1 = ne1.triangleIndex;
                    var t2 = ne2.triangleIndex;

                    // next edges
                    void CheckEdge(int tri, int e1, int e2) {
                        // contrained, don't flip
                        PackedEdge pe = PackEdge(e1, e2);
                        if (constrainedEdges.Contains(pe)) {
                            return;
                        }

                        // Already in queue
                        if (inQueue.Contains(pe)) {
                            return;
                        }

                        // Get opposite triangle
                        var ev = triangleVerts[tri];
                        var en = triangleNeighbors[tri];
                        int o =
                            ev.v0 == e1 ? en.n0 :
                            ev.v1 == e1 ? en.n1 :
                            en.n2;

                        // new edge, already good
                        if (o == t1 || o == t2) {
                            return;
                        }
                        // queue edge
                        candidates.Enqueue((e1, e2));
                        inQueue.Add(pe);
                    }
                 
                    // Check edges of new triangles
                    var (v0, v1, v2) = triangleVerts[t1];
                    CheckEdge(t1, v0, v1);
                    CheckEdge(t1, v1, v2);
                    CheckEdge(t1, v2, v0);

                    (v0, v1, v2) = triangleVerts[t2];
                    CheckEdge(t2, v0, v1);
                    CheckEdge(t2, v1, v2);
                    CheckEdge(t2, v2, v0);
                }
            }
            ObjectPool.ReturnHashSet(inQueue);
        }


        void AddConstraint(TriangleElement v1, TriangleElement v2) {
            var safety = new LoopSafety(1000); // for debugging. Remove when everything works
            safety.SetException("AddConstraint");
            var vidx1 = v1.vertexIndex;
            var vidx2 = v2.vertexIndex;

            if (vidx1 < 3 || vidx2 < 3) {
                Debug.LogWarning("Trying to create super-constraint");
                return;
            }

            var p1 = vertexPos[vidx1];
            var p2 = vertexPos[vidx2];

            // get target edge
            var targetEdge = PackEdge(vidx1, vidx2);

            // constraint already exists
            if (constrainedEdges.Contains(targetEdge)) {
                return;
            }

            // edge exists
            int startTri = v1.triangleIndex;
            int currentTri = startTri;


            while (true) {
                safety.Next();


                var (cv0, cv1, cv2)  = triangleVerts[currentTri];

                if (cv0 == vidx2 || cv1 == vidx2 || cv2 == vidx2) {
                    if (!constrainedEdges.Contains(targetEdge)) {
                        constrainedEdges.Add(targetEdge);
                        vertexNumConstraints[vidx1]++;
                        vertexNumConstraints[vidx2]++;
                    }
                    return;
                }

                currentTri = WalkVertex(vidx1,currentTri);
                if (currentTri == startTri) {
                    break;
                }
            }

            // get starting walk triangle 

            int walkStartingTriangle = -1;
            currentTri = startTri;
            while (true) {
                safety.Next();

                // get opposite edge
                int tv1 = -1;
                int tv2 = -1;

                var (cv0, cv1, cv2)  = triangleVerts[currentTri];
                if (cv0 == vidx1) {
                    tv1 = cv1;
                    tv2 = cv2;
                }
                if (cv1 == vidx1) {
                    tv1 = cv2;
                    tv2 = cv0;
                }
                if (cv2 == vidx1) {
                    tv1 = cv0;
                    tv2 = cv1;
                }

                // this vertex is on the target constraint
                if (Geometry.OnSegment(vertexPos[tv1], vertexPos[vidx1], vertexPos[vidx2])) {
                    // split
                    constraintToDivide.Add(PackEdge(vidx1, vidx2), tv1);
                    AddConstraint(v1, TriangleElement.AsVertex(currentTri, tv1));
                    AddConstraint(v2, TriangleElement.AsVertex(currentTri, tv1));
                    return;
                }


                bool intersects =
                    Geometry.IsClockwise(vertexPos[vidx1], vertexPos[vidx2], vertexPos[tv1])
                        != Geometry.IsClockwise(vertexPos[vidx1], vertexPos[vidx2], vertexPos[tv2])
                    &&
                    Geometry.IsClockwise(vertexPos[tv1], vertexPos[tv2], vertexPos[vidx1])
                        != Geometry.IsClockwise(vertexPos[tv1], vertexPos[tv2], vertexPos[vidx2]);
                if (intersects) {
                    walkStartingTriangle = currentTri;
                    break;
                }


                currentTri = WalkVertex(vidx1, currentTri);
                if (currentTri == startTri) {
                    break;
                }
            }

            if (walkStartingTriangle < 0) {
                // this should never occur 
                // except maybe due to rounding error?
                throw new System.Exception("something went wrong");
            }

            using (var invalidEdgesScope = ObjectPool.GetScopedList<(int e1, int e2)>()) {
                var invalidEdges = invalidEdgesScope.item;

                int currentTriangle = walkStartingTriangle;
                PackedEdge lastEdge = new PackedEdge(int.MaxValue, int.MaxValue);
                while (true) {
                    safety.Next();
                    var (tv0, tv1, tv2)  = triangleVerts[currentTriangle];

                    var vp0 = vertexPos[tv0];
                    var vp1 = vertexPos[tv1];
                    var vp2 = vertexPos[tv2];
                    // reached the end
                    if (tv0 == vidx2) { break; }
                    if (tv1 == vidx2) { break; }
                    if (tv2 == vidx2) { break; }


                    // target constraint crosses an existing point
                    if (Geometry.IsOnSegment(p1, p2, vp0)) {
                        constraintToDivide.Add(PackEdge(vidx1, vidx2), tv0);
                        AddConstraint(v1, TriangleElement.AsVertex(currentTriangle, tv0));
                        AddConstraint(v2, TriangleElement.AsVertex(currentTriangle, tv0));
                        return;
                    }

                    if (Geometry.IsOnSegment(p1, p2, vp1)) {
                        constraintToDivide.Add(PackEdge(vidx1, vidx2), tv1);
                        AddConstraint(v1, TriangleElement.AsVertex(currentTriangle, tv1));
                        AddConstraint(v2, TriangleElement.AsVertex(currentTriangle, tv1));
                        return;
                    }

                    if (Geometry.IsOnSegment(p1, p2, vp2)) {
                        constraintToDivide.Add(PackEdge(vidx1, vidx2), tv2);
                        AddConstraint(v1, TriangleElement.AsVertex(currentTriangle, tv2));
                        AddConstraint(v2, TriangleElement.AsVertex(currentTriangle, tv2));
                        return;
                    }

                    TriangleElement nextEdge = TriangleElement.INVALID;
                    int nextT = -1;

                    // find the next edge/triangle
                    var (n0, n1, n2) = triangleNeighbors[currentTriangle];
                    if (Geometry.IsIntersecting(p1, p2, vp0, vp1, true)) {
                        if (PackEdge(tv0, tv1) != lastEdge) {
                            nextEdge = TriangleElement.AsEdge(currentTriangle, tv0, tv1);
                            nextT = n0;
                        }
                    }
                    if (Geometry.IsIntersecting(p1, p2, vp1, vp2, true)) {
                        if (PackEdge(tv1, tv2) != lastEdge) {
                            nextEdge = TriangleElement.AsEdge(currentTriangle, tv1, tv2);
                            nextT = n1;
                        }
                    }
                    if (Geometry.IsIntersecting(p1, p2, vp2, vp0, true)) {
                        if (PackEdge(tv2, tv0) != lastEdge) {
                            nextEdge = TriangleElement.AsEdge(currentTriangle, tv2, tv0);
                            nextT = n2;
                        }
                    }

                    if (nextT < 0) {
                        throw new Exception("Something went wrong");
                    }
                    lastEdge = PackEdge(nextEdge.ev1, nextEdge.ev2);


                    if (constrainedEdges.Contains(lastEdge)) {
                        throw new InvalidOperationException($"OVERLAPPING CONSTRAINTS\n EXISTING: {vertexPos[nextEdge.ev1]}-{vertexPos[nextEdge.ev2]}\nNEW: {p1}-{p2}\n(new constraint may be a smaller split, if the constraint intersects a points)");

                    }
                    invalidEdges.Add((nextEdge.ev1, nextEdge.ev2));
                    currentTriangle = nextT;
                }


                var buffer = ObjectPool.GetList<(int e1, int e2)>();

                var b1 = invalidEdges;
                var b2 = buffer;
                safety.SetLog();
                while (safety.Next()) {
                    foreach (var e in b1) {
                        var (x, _) = FlipEdge(e.e1, e.e2);

                        if (Geometry.IsIntersecting(p1, p2, vertexPos[x.ev1], vertexPos[x.ev2], requireProperCrossing: true)) {
                            b2.Add((x.ev1, x.ev2));
                        }
                    }

                    if (b2.Count == 0) {
                        break;
                    }

                    var temp = b1;
                    b1 = b2;
                    b2 = temp;
                    b2.Clear();
                }
                constrainedEdges.Add(targetEdge);

                vertexNumConstraints[vidx1]++;
                vertexNumConstraints[vidx2]++;

                ObjectPool.ReturnList(buffer);
            }
        }

        TriangleElement GetTriangleEdgeElement(int triangleIndex, int vertexIndex) {
            var (v0, v1, v2)  = triangleVerts[triangleIndex];

            if (v0 == vertexIndex) { return TriangleElement.AsEdge(triangleIndex, v0, v1); }
            if (v1 == vertexIndex) { return TriangleElement.AsEdge(triangleIndex, v1, v2); }
            if (v2 == vertexIndex) { return TriangleElement.AsEdge(triangleIndex, v2, v0); }

            throw new InvalidOperationException("Vertex does not belong to triangle");
        }

        /// <summary>
        /// Returns the new edge (or old if no changes were made)
        /// </summary>
        (TriangleElement v1, TriangleElement v2) FlipEdge(int v1, int v2) {
            // Triangles may be destroy by other flips, so we can't use TriangleElement
            TriangleElement halfEdgeA, halfEdgeB;
            (halfEdgeA, halfEdgeB) = GetEdgeElement(v1, v2);
            return FlipEdge(halfEdgeA, halfEdgeB);
        }
        /// <summary>
        /// Returns the new edge (or old if no changes were made)
        /// </summary>
        (TriangleElement v1, TriangleElement v2) FlipEdge(TriangleElement halfEdgeA, TriangleElement halfEdgeB) {
            if (!EdgeIsFlippable(halfEdgeA, halfEdgeB)) {
                return (halfEdgeA, halfEdgeB); // process this edge again later
            }


            var ev1 = halfEdgeA.ev1;
            var ev2 = halfEdgeA.ev2;
            var avo = GetOppositeVertex(halfEdgeA);
            var bvo = GetOppositeVertex(halfEdgeB);


            // new triangles
            int newT1 = ClaimTriangle(bvo, avo, ev1);
            int newT2 = ClaimTriangle(avo, bvo, ev2);


            // neighbors for new triangles
            ref var nt1 = ref triangleNeighbors[newT1];
            ref var nt2 = ref triangleNeighbors[newT2];


            // ta = ev1, ev2, avo
            // tb = ev2, ev1, bvo
            var va = triangleVerts[halfEdgeA.triangleIndex];
            var vb = triangleVerts[halfEdgeB.triangleIndex];


            var na = triangleNeighbors[halfEdgeA.triangleIndex];
            var nb = triangleNeighbors[halfEdgeB.triangleIndex];

            var n2a =
                va.v0 == ev1 ? (na.n1, na.n2) :
                va.v1 == ev1 ? (na.n2, na.n0) :
                (na.n0, na.n1);
            var n1b =
                vb.v0 == ev2 ? (nb.n1, nb.n2) :
                vb.v1 == ev2 ? (nb.n2, nb.n0) :
                (nb.n0, nb.n1);


            var (v0, v1, v2) = triangleVerts[newT1];

            nt1.n0 = newT2;
            nt1.n1 = n2a.Item2;
            nt1.n2 = n1b.Item1;

            (v0, v1, v2) = triangleVerts[newT2];
            nt2.n0 = newT1;
            nt2.n1 = n1b.Item2;
            nt2.n2 = n2a.Item1;

            // 4 surrounding triangles

            ReplaceNeighborWithSelf(TriangleElement.AsEdge(newT1, ev1, bvo));
            ReplaceNeighborWithSelf(TriangleElement.AsEdge(newT1, avo, ev1));

            ReplaceNeighborWithSelf(TriangleElement.AsEdge(newT2, ev2, avo));
            ReplaceNeighborWithSelf(TriangleElement.AsEdge(newT2, bvo, ev2));

            RemoveTriangle(halfEdgeA.triangleIndex);
            RemoveTriangle(halfEdgeB.triangleIndex);

            var ra = GetTriangleEdgeElement(newT1, bvo);
            var rb = GetTriangleEdgeElement(newT2, avo);

            return (ra, rb);
        }

   

        void RemoveBadTriangles(HashSet<int> badTriangles) {
            foreach (var t in badTriangles) {
                RemoveTriangle(t);
            }
        }

        private void RemoveTriangle(int t) {
            triangleValid[t] = false;
            availableTriangleSlots.Push(t);
        }

        void ReplaceNeighborWithSelf(TriangleElement edge) {
            var tri = edge.triangleIndex;
            if (tri < 0) {
                return;
            }
            var oldNeighbor = GetOtherHalfEdge(edge).triangleIndex;

            if (oldNeighbor < 0) {
                return;
            }
            triangleID[oldNeighbor]++;

            var (v0, v1, v2)  = triangleVerts[oldNeighbor];
            ref var nOld = ref triangleNeighbors[oldNeighbor];

            if (v0 == edge.ev2) {
                nOld.n0 = tri;
                return;
            }
            if (v1 == edge.ev2) {
                nOld.n1 = tri;
                return;
            }
            if (v2 == edge.ev2) {
                nOld.n2 = tri;
                return;
            }
            throw new System.Exception("Something went wrong");
        }

        void GetCavity(HashSet<int> badTriangles, List<(int, int)> cavityEdges, Dictionary<PackedEdge, int> edge2Neighbor, Dictionary<int, int> v2next) {
            foreach (var t in badTriangles) {
                var (v0, v1, v2)  = triangleVerts[t];

                var (n0, n1, n2) = triangleNeighbors[t];
  

                PackedEdge e01 = PackEdge(v0, v1);
                PackedEdge e12 = PackEdge(v1, v2);
                PackedEdge e20 = PackEdge(v2, v0);

                if (!badTriangles.Contains(n0)) {
                    if (!edge2Neighbor.ContainsKey(e01)) {
                        edge2Neighbor.Add(e01, n0);
                        cavityEdges.Add((v0, v1));
                    }
                }

                if (!badTriangles.Contains(n1)) {
                    if (!edge2Neighbor.ContainsKey(e12)) {
                        edge2Neighbor.Add(e12, n1);
                        cavityEdges.Add((v1, v2));
                    }
                }

                if (!badTriangles.Contains(n2)) {
                    if (!edge2Neighbor.ContainsKey(e20)) {
                        edge2Neighbor.Add(e20, n2);
                        cavityEdges.Add((v2, v0));
                    }
                }
            }


            // order edges
            foreach (var (a, b) in cavityEdges) {
                v2next?.Add(a, b);
            }

        }



        #endregion
        //************************************************************************************************
        #region Claims
        //************************************************************************************************


        int ClaimVertexSlot(Vector2Int value) {
            if (availableVertexSlots.Count == 0) {
                availableVertexSlots.Push(vertexPos.Count);
                vertexPos.Add(new Vector2Int());
                vertexValid.Add(false);
                vertexNumConstraints.Add(0);
                vertexTriangleHead.Add(-1);
            }

            // add vertex and set default values
            int newVert = availableVertexSlots.Pop();
            vertexPos[newVert] = value;
            vertexValid[newVert] = true;
            vertexNumConstraints[newVert] = 0;

            return newVert;

        }


        int ClaimTriangle(int v0, int v1, int v2) {
            int result = -1;
            if (availableTriangleSlots.Count == 0) {
                result = triangleID.Count;

                if (triangleVerts.Length <= result) {
                    Debug.Assert(triangleVerts.Length == triangleNeighbors.Length);
                    Array.Resize(ref triangleVerts, triangleVerts.Length * 2 + 1);
                    Array.Resize(ref triangleNeighbors, triangleNeighbors.Length * 2 + 1);
                }

          
            

                _triangleSeenCache.Add(0);
                triangleValid.Add(true);
                triangleID.Add(-1);
            }
            else {
                result = availableTriangleSlots.Pop();
            }

            triangleVerts[result] = (v0,v1, v2);

            triangleNeighbors[result] = (-1, -1, -1);

            triangleValid[result] = true;
            triangleID[result]++;
            lastAddedTri = result;


            vertexTriangleHead[v0] = result;
            vertexTriangleHead[v1] = result;
            vertexTriangleHead[v2] = result;
            return result;
        }

   
        #endregion
        //************************************************************************************************
        #region Validation
        //************************************************************************************************
        /// <summary>
        /// Returns null if no errors
        /// </summary>
        public string ValidateTriangles() {
            // all arrays should have same length
            var validCount = triangleValid.Count;

            if (validCount != triangleVerts.Length) { return $"TRIANGULATION INVALID - List count mismatch\nValid: {validCount}\nV0: {triangleVerts.Length}"; }

            if (validCount != triangleNeighbors.Length) { return $"TRIANGULATION INVALID - List count mismatch\nValid: {validCount}\nV0: {triangleNeighbors.Length}"; }

            if (validCount != triangleID.Count) { return $"TRIANGULATION INVALID - List count mismatch\nValid: {validCount}\nV0: {triangleID.Count}"; }


            // Check each triangle
            for (int t = 0; t < triangleValid.Count; t++) {
                if (!triangleValid[t]) {
                    continue;
                }
                var (v0, v1, v2)  = triangleVerts[t];

                var (n0, n1, n2) = triangleNeighbors[t];

                // All verticies are valid
                if (!vertexValid[v0]) { return $"TRIANGLE {t} INVALID - Vertex 0 is invalid"; }
                if (!vertexValid[v1]) { return $"TRIANGLE {t} INVALID - Vertex 1 is invalid"; }
                if (!vertexValid[v2]) { return $"TRIANGLE {t} INVALID - Vertex 2 is invalid"; }

                // Degenerate
                if (vertexPos[v0] == vertexPos[v1] ||
                    vertexPos[v1] == vertexPos[v2] ||
                    vertexPos[v2] == vertexPos[v0]) {
                    return $"TRIANGLE {t} INVALID - Degenerate (duplicate vertices)";
                }

                // Verticies are CCW
                if (!LPE.Math.Geometry.IsCounterClockwise(vertexPos[v0], vertexPos[v1], vertexPos[v2])) {
                    return $"TRIANGLE {t} INVALID - Vertices are not in CCW order {vertexPos[v0]} {vertexPos[v1]} {vertexPos[v2]} {LPE.Math.Geometry.Orientation(vertexPos[v0], vertexPos[v1], vertexPos[v2])}";
                }

                string CheckNeighbor(int n, int va, int vb, int label) {
                    // Neighbors are valid or -1
                    if (n == -1) {
                        // if neighbor is -1, edge must contain super vertex
                        if (va < 3 || vb < 3) {
                            return null;
                        }
                        else {
                            return $"TRIANGLE {t} NEIGHBOR {label} INVALID - Neighbor is -1, but edge doess not contain super vertex";
                        }
                    }
                    else {
                        var (v0, v1, v2)  = triangleVerts[n];
                        var (n0, n1, n2) = triangleNeighbors[t];
                        if (n0 == t) {
                            if (v0 != vb || v1 != va) {
                                return $"TRIANGLE {t} NEIGHBOR {label} INVALID - (n0) Edges do not match or is misaligned";
                            }
                        }
                        else if (n1 == t) {
                            if (v1 != vb || v2 != va) {
                                return $"TRIANGLE {t} NEIGHBOR {label} INVALID - (n1) Edges do not match or is misaligned";
                            }
                        }
                        else if (n2 == t) {
                            if (v2 != vb || v0 != va) {
                                return $"TRIANGLE {t} NEIGHBOR {label} INVALID - (n2) Edges do not match or is misaligned";
                            }
                        }
                        else {
                            return $"TRIANGLE {t} NEIGHBOR {label} INVALID - Neighbor does not have {t} as one of its neighbor";
                        }
                    }
                    // Neigherbors and vertices aligned
                    return null;
                }

                var s = CheckNeighbor(n0, v0, v1, 0) ?? CheckNeighbor(n1, v1, v2, 1) ?? CheckNeighbor(n2, v2, v0, 2);
                if (s != null) {
                    return s;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns null if no errors
        /// </summary>
        public string ValidateVertices() {
            var vCount = vertexValid.Count;

            if (vCount != vertexPos.Count) { return $"TRIANGULATION INVALID - List count mismatch\nExpeccted: {vCount}\vertexPos: {vertexPos.Count}"; }

            for (int v = 0; v < vCount; v++) {
                if (!vertexValid[v]) {
                    continue;
                }

            }

            return null;
        }

        #endregion
        //************************************************************************************************
        #region Gizmos
        //************************************************************************************************

        public void DrawTriangleGizmos() {
            var testV0 = new Vector2Int(0, 0);
            var testV1 = new Vector2Int(2, 0);
            var v0ele = GetEnclosingElement(testV0);
            var v1ele = GetEnclosingElement(testV0);


            for (int i = 0; i < triangleID.Count; i++) {
                if (!triangleValid[i]) {
                    continue;
                }
                var (vi0, vi1, vi2)  = triangleVerts[i];
                //var vi0 = triangleV0[i];
                //var vi1 = triangleV1[i];
                //var vi2 = triangleV2[i];

                var v0 = (Vector2)vertexPos[vi0];
                var v1 = (Vector2)vertexPos[vi1];
                var v2 = (Vector2)vertexPos[vi2];

                // TODO - change color if constraint

                if (vi0 > 2 && vi1 > 2) {
                    Gizmos.color = constrainedEdges.Contains(PackEdge(vi0, vi1)) ? Color.red : Color.green;
                }
                else {
                    Gizmos.color = Color.yellow;
                }


                Gizmos.DrawLine(v0, v1);
                if (vi2 > 2 && vi1 > 2) {
                    Gizmos.color = constrainedEdges.Contains(PackEdge(vi1, vi2)) ? Color.red : Color.green;
                }
                else {
                    Gizmos.color = Color.yellow;
                }
                Gizmos.DrawLine(v1, v2);


                if (vi0 > 2 && vi2 > 2) {
                    Gizmos.color = constrainedEdges.Contains(PackEdge(vi2, vi0)) ? Color.red : Color.green;
                }
                else {
                    Gizmos.color = Color.yellow;
                }
                Gizmos.DrawLine(v2, v0);
            }
        }
        public void DrawTriangleNeighborGizmos() {
            Gizmos.color = Color.cyan;

            for (int t = 0; t < triangleValid.Count; t++) {
                if (!triangleValid[t])
                    continue;

                var (v0, v1, v2)  = triangleVerts[t];
                if (v0 < 3 || v1 < 3 || v2 < 3) {
                    continue;
                }

                Vector3 centerT = TriangleCenter(t);

                var (n0, n1, n2) = triangleNeighbors[t];
                foreach (var n in new[] { n0, n1, n2 }) {
                    if (n < 0 || !triangleValid[n])
                        continue;

                    var (nv0, nv1, nv2)  = triangleVerts[t];
                    if (nv0 < 3 || nv1 < 3 || nv2 < 3) {
                        continue;
                    }

                    Vector3 centerN = TriangleCenter(n);
                    centerN = centerN * 0.9f + centerT * 0.1f;
                    Gizmos.DrawLine(centerT, centerN);
                }
            }
        }



        #endregion
        //************************************************************************************************
        #region Helpers
        //************************************************************************************************
        
        
        (TriangleElement e1, TriangleElement e2) GetEdgeElement(int v1, int v2) {
            TriangleElement r1 = new TriangleElement();
            TriangleElement r2 = new TriangleElement();
            var safety = new LoopSafety(200);
            safety.SetException();


            // walk vertex
            int triangleHead = vertexTriangleHead[v1];
            int current = triangleHead;

            while (safety.Next()) {
                // check for edge
                bool foundEdge = CheckTriangle(v1, v2, ref r1, ref r2, current);
                if (foundEdge) {
                    return (r1, r2);
                }

                // next
                current = WalkVertex(v1, current);
                if (current == triangleHead) {
                    break;
                }
                if (current < 0) {
                    break;
                }
            }
            
            // hit convex hull
            if (current == -1) {
                // restart in opposite direction
                current = WalkVertexReverse(v1, triangleHead); // skip first

                while (safety.Next()) {
                    bool foundEdge = CheckTriangle(v1, v2, ref r1, ref r2, current);
                    if (foundEdge) {
                        return (r1, r2);
                    }

                    // next
                    current = WalkVertexReverse(v1, current);
                    if (current == triangleHead) {
                        break;
                    }
                    if (current < 0) {
                        break;
                    }
                }
            }


            throw new Exception($"Edge does not exist: {vertexPos[v1]} {vertexPos[v2]}");


            // Returns true if found edge
            bool CheckTriangle(int v1, int v2, ref TriangleElement r1, ref TriangleElement r2, int current) {
                int cv1 = -1;
                int cv2 = -1;

                 var (vi0, vi1, vi2)  = triangleVerts[current];
                // check for v2
                if (vi0 == v2) cv2 = 0;
                if (vi1 == v2) cv2 = 1;
                if (vi2 == v2) cv2 = 2;

                if (cv2 >= 0) {
                    // find v1
                    if (vi0 == v1) cv1 = 0;
                    if (vi1 == v1) cv1 = 1;
                    if (vi2 == v1) cv1 = 2;

                    if (cv1 < 0) {
                        // we are walking around v1, so this triangle should contain it
                        throw new Exception();
                    }

                    // 01, 12, 20
                    if ((cv2 - cv1 + 3) % 3 == 1) {
                        r1 = TriangleElement.AsEdge(current, v1, v2);
                        r2 = GetOtherHalfEdge(r1);
                        return true;
                    }
                    // 10, 21, 02
                    if ((cv2 - cv1 + 3) % 3 == 2) {
                        r1 = TriangleElement.AsEdge(current, v2, v1);
                        r2 = GetOtherHalfEdge(r1);
                        return true;
                    }
                }

                return false;
            }
        }

        public (int, int) GetEdgeFromTriangles(int triA, int triB) {
            var (a1, a2, a3)  = triangleVerts[triA];
            var (b1, b2, b3)  = triangleVerts[triB];

            int r1 = -1;
            int r2 = -1;

            if (a1 == b1 || a1 == b2 || a1 == b3) {
                r1 = a1;
            }

            if (a2 == b1 || a2 == b2 || a2 == b3) {
                if (r1 < 0) {
                    r1 = a2;
                }
                else {
                    r2 = a2;
                }
            }
            if (a3 == b1 || a3 == b2 || a3 == b3) {
                if (r1 < 0) {
                    r1 = a3;
                }
                else {
                    r2 = a3;
                }
            }

            return (r1, r2);
        }

        Vector3 TriangleCenter(int t) {
            var (vi0, vi1, vi2)  = triangleVerts[t];
            Vector2 v0 = vertexPos[vi0];
            Vector2 v1 = vertexPos[vi1];
            Vector2 v2 = vertexPos[vi2];
            return (v0 + v1 + v2) / 3f;
        }

        (int, int) GetEdgeVertices(int t1, int t2) {
            var (a0, a1, a2) = triangleVerts[t1];
            var (b0, b1, b2) = triangleVerts[t2];

           

            // find shared edge
            int sharedA = -1;
            int sharedB = -1;

            if (a0 == b0 || a0 == b1 || a0 == b2) {
                sharedA = a0;
            }

            if (a1 == b0 || a1 == b1 || a1 == b2) {
                if (sharedA < 0) {
                    sharedA = a1;
                }
                else {
                    sharedB = a1;
                }
            }
            if (a2 == b0 || a2 == b1 || a2 == b2) {
                if (sharedA < 0) {
                    sharedA = a2;
                }
                else {
                    sharedB = a2;
                }
            }

            if (sharedA < 0 || sharedB < 0) {
                return (-1, -1);
            }
            var (ra, _, _) = triangleVerts[sharedA];
            var (rb, _, _) = triangleVerts[sharedB];
            return (ra, rb);

        }

        bool EdgeIsConstrained(int t1, int t2) {
            if (t2 < 0) return false;

            // find shared edge
            var (a0, a1, a2) = triangleVerts[t1];
            var (b0, b1, b2) = triangleVerts[t2];

            // find the two shared vertices
            int sharedA = -1, sharedB = -1;

            if (a0 == b0 || a0 == b1 || a0 == b2) sharedA = a0;
            if (a1 == b0 || a1 == b1 || a1 == b2) {
                if (sharedA < 0) sharedA = a1;
                else sharedB = a1;
            }
            if (a2 == b0 || a2 == b1 || a2 == b2) {
                if (sharedA < 0) sharedA = a2;
                else sharedB = a2;
            }

            if (sharedA < 0 || sharedB < 0)
                return false; // no shared edge (should not happen)

            PackedEdge key = PackEdge(sharedA, sharedB);
            return constrainedEdges.Contains(key);
        }

        PackedEdge PackEdge(int a, int b) {
            return new PackedEdge(a, b);
        }

        bool IsSuperTriangle(int t) {
            var (v0, v1, v2) = triangleVerts[t];
            return v0 < 3 || v1 < 3 || v2 < 3;
        }


        
        TriangleElement GetEnclosingElement(Vector2 point, int startHint = -1) {
            int currentTriangle = startHint < 0 ? lastQueriedTriangle : startHint;
            if (currentTriangle < 0 || !triangleValid[currentTriangle]) {
                currentTriangle = lastAddedTri;
            }
            currentTriangle = lastAddedTri;

            int prevTriangle = -1;
            int targetTriangle = -1;

            var safety = new LoopSafety(1000);
            safety.SetException();

            while (currentTriangle >= 0) {
                safety.Next();
                var (v0, v1, v2) = triangleVerts[currentTriangle];
               
                Vector2Int p0 = vertexPos[v0];
                Vector2Int p1 = vertexPos[v1];
                Vector2Int p2 = vertexPos[v2];

                if (Geometry.InTriangle(point, p0, p1, p2)) {
                    targetTriangle = currentTriangle;
                    break;
                }

                var (n0, n1, n2) = triangleNeighbors[currentTriangle];
                int edgeToCross = -1;
                if (n0 != prevTriangle && Geometry.IsClockwise(point, p0, p1)) {
                    edgeToCross = 0;
                }
                else if (n1 != prevTriangle && Geometry.IsClockwise(point, p1, p2)) {
                    edgeToCross = 1;
                }
                else if (n2 != prevTriangle && Geometry.IsClockwise(point, p2, p0)) {
                    edgeToCross = 2;
                }


                if (edgeToCross == -1) {
                    break;
                }

                prevTriangle = currentTriangle;
                switch (edgeToCross) {
                    case 0:
                        currentTriangle = n0;
                        break;
                    case 1:
                        currentTriangle = n1;
                        break;
                    case 2:
                        currentTriangle = n2;
                        break;
                    default:
                        currentTriangle = -1;
                        break;
                }
            }


            if (targetTriangle >= 0) {
                lastQueriedTriangle = targetTriangle;

                var (v0, v1, v2) = triangleVerts[targetTriangle];
    
                var p0 = vertexPos[v0];
                var p1 = vertexPos[v1];
                var p2 = vertexPos[v2];

                // point is on vertex
                if (p0 == point) return TriangleElement.AsVertex(targetTriangle, v0);
                if (p1 == point) return TriangleElement.AsVertex(targetTriangle, v1);
                if (p2 == point) return TriangleElement.AsVertex(targetTriangle, v2);


                // point is on edge
                if (Geometry.IsOnSegment(p0, p1, point)) return TriangleElement.AsEdge(targetTriangle, v0, v1);
                if (Geometry.IsOnSegment(p1, p2, point)) return TriangleElement.AsEdge(targetTriangle, v1, v2);
                if (Geometry.IsOnSegment(p2, p0, point)) return TriangleElement.AsEdge(targetTriangle, v2, v0);

                // point is inside 1 triangle
                return TriangleElement.AsTriangle(targetTriangle);
            }

            // failed
            throw new Exception($"{point}");
        }
        
        string TriangleString(int t) {
            var (v0, v1, v2) = triangleVerts[t];
            return $"{vertexPos[v0]} {vertexPos[v1]} {vertexPos[v2]}";
        }

        bool EdgeIsFlippable(TriangleElement halfEdgeA, TriangleElement halfEdgeB) {
            // on hull
            if (halfEdgeA.triangleIndex < 0 || halfEdgeB.triangleIndex < 0) {
                return false;
            }

            var ev1 = halfEdgeA.ev1;
            var ev2 = halfEdgeA.ev2;
            var avo = GetOppositeVertex(halfEdgeA);
            var bvo = GetOppositeVertex(halfEdgeB);


            if (!Geometry.IsConvex(vertexPos[avo], vertexPos[ev1], vertexPos[bvo], vertexPos[ev2])) {
                return false;
            }
            return true;
        }

        int GetOppositeVertex(TriangleElement halfEdge) {
            if (!halfEdge.isEdge) {
                throw new ArgumentException("Edge element is not an edge");
            }

            var (v0, v1, v2) = triangleVerts[halfEdge.triangleIndex];
          

            
            if (v0 == halfEdge.ev1) {
                return v2;
            }
            if (v1 == halfEdge.ev1) {
                return v0;
            }
            if (v2 == halfEdge.ev1) {
                return v1; 
            }
            throw new ArgumentException("Something went wrong");
        }

        /// <summary>
        /// Returns the other side of the triangle as an edge
        /// </summary>
        TriangleElement GetOtherHalfEdge(TriangleElement edge) {
            if (!edge.isEdge) {
                throw new ArgumentException("Edge element is not an edge");
            }

            var (v0, v1, v2) = triangleVerts[edge.triangleIndex];
            var (n0, n1, n2) = triangleNeighbors[edge.triangleIndex];


            if (v0 == edge.ev1) {
                if (v1 != edge.ev2) { // edges must be in CCW orientation relative to triangle
                    // throw instead of fix because something went wrong upstream
                    throw new ArgumentException("Edge element vertices are not properly ordered");
                }

                return TriangleElement.AsEdge(n0, v1, v0); // orientation is reversed
            }
            if (v1 == edge.ev1) {
                if (v2 != edge.ev2) { 
                    throw new ArgumentException("Edge element vertices are not properly ordered");
                }

                return TriangleElement.AsEdge(n1, v2, v1); 
            }
            if (v2 == edge.ev1) {
                if (v0 != edge.ev2) {
                    throw new ArgumentException("Edge element vertices are not properly ordered");
                }

                return TriangleElement.AsEdge(n2, v0, v2);
            }
            throw new ArgumentException("Something went wrong");
        }

        int WalkVertexReverse(int vert, int currentTri) {
            var (v0, v1, v2) = triangleVerts[currentTri];
            var (n0, n1, n2) = triangleNeighbors[currentTri];
            if (v0 == vert) {
                return n2;
            }
            if (v1 == vert) {
                return n0;
            }
            if (v2 == vert) {
                return n1;
            }
            throw new System.InvalidOperationException("Vert does not belong to triangle");
        }
        int WalkVertex(int vert, int currentTri) {
            var (v0, v1, v2) = triangleVerts[currentTri];
            var (n0, n1, n2) = triangleNeighbors[currentTri];
            if (v0 == vert) {
                return n0;
            }
            if (v1 == vert) {
                return n1;
            }
            if (v2 == vert) {
                return n2;
            }
            throw new System.InvalidOperationException("Vert does not belong to triangle");
        }

        #endregion
        //************************************************************************************************

        /// <summary>
        /// Edges represents a half-edge (edge + 1 of the triangles)
        /// </summary>
        readonly struct TriangleElement : IEquatable<TriangleElement> {
            public static TriangleElement INVALID => new TriangleElement(-1, Type.None, -1, -1);
            public enum Type { None, Triangle, Edge, Vertex }

            public bool isInvalid => elementType == Type.None;
            public bool isVertex => elementType == Type.Vertex;
            public bool isEdge => elementType == Type.Edge;
            public bool isTriangle => elementType == Type.Triangle;

            public readonly int triangleIndex;
            public int vertexIndex => v1;
            public int ev1 => v1;
            public int ev2 => v2;


            readonly Type elementType;
            readonly int v1;
            readonly int v2;


            // Private master constructor
            TriangleElement(int triangleIndex, Type type, int a, int b) {
                this.triangleIndex = triangleIndex;
                this.elementType = type;
                this.v1 = a;
                this.v2 = b;
            }

            public static TriangleElement AsTriangle(int triangleIndex)
                => new TriangleElement(triangleIndex, Type.Triangle, -1, -1);

            public static TriangleElement AsVertex(int triangleIndex, int vertIndex)
                => new TriangleElement(triangleIndex, Type.Vertex, vertIndex, -1);

            public static TriangleElement AsEdge(int triangleIndex, int e1, int e2)
                => new TriangleElement(triangleIndex, Type.Edge, e1, e2);

            // Equality implementation for performance
            public bool Equals(TriangleElement other) =>
                triangleIndex == other.triangleIndex &&
                elementType == other.elementType &&
                v1 == other.v1 &&
                v2 == other.v2;
        }



        struct PackedEdge : IEquatable<PackedEdge> {
            public readonly int A, B;

            public PackedEdge(int a, int b) {
                if (a > b) {
                    A = b;
                    B = a; 
                }
                else { 
                    A = a;
                    B = b;
                }
            }

            public override int GetHashCode() {
                return unchecked(A * 314159 + B);
            }

            public bool Equals(PackedEdge other) => A == other.A && B == other.B;
            public override bool Equals(object obj) => obj is PackedEdge e && Equals(e);

            
            public static bool operator ==(PackedEdge left, PackedEdge right) => left.Equals(right);
            public static bool operator !=(PackedEdge left, PackedEdge right) => !left.Equals(right);
        }
    }

    public readonly struct TriangleID : System.IEquatable<TriangleID> {
        public static readonly TriangleID INVALID = new TriangleID(-1, -1);

        public readonly int id;
        public readonly int index;

        public TriangleID(int id, int index) {
            this.id = id;
            this.index = index;
        }

        public bool Equals(TriangleID other) {
            return id == other.id && index == other.index;
        }

        public override bool Equals(object obj) {
            return obj is TriangleID other && Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(id, index);
        }

        public static bool operator ==(TriangleID left, TriangleID right) {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleID left, TriangleID right) {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Neighbors are in CCW order
    /// portals and vertices are also in CCW order
    /// p1 is portal to n1, etc
    /// </summary>
    public struct DelaunayTriangleData {
        public readonly TriangleID n1;
        public readonly TriangleID n2;
        public readonly TriangleID n3;

        public readonly Vector2Int v1;
        public readonly Vector2Int v2;
        public readonly Vector2Int v3;
        public readonly Vector2 center;

        public (Vector2Int, Vector2Int) p1 => (v1, v2);
        public (Vector2Int, Vector2Int) p2 => (v2, v3);
        public (Vector2Int, Vector2Int) p3 => (v3, v1);

        public DelaunayTriangleData(TriangleID n1, TriangleID n2, TriangleID n3, Vector2Int v1, Vector2Int v2, Vector2Int v3) {
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            center = (Vector2)(v1 + v2 + v3) / 3;
        }

        public TriangleID GetNeighbor(int i) {
            if (i == 0) { return n1; }
            if (i == 1) { return n2; }
            if (i == 2) { return n3; }

            throw new System.IndexOutOfRangeException();
        }

        public void GetPortal(int i, out Vector2Int a, out Vector2Int b) {
            if (i == 0) { a = v1; b = v2; return; }
            if (i == 1) { a = v2; b = v3; return; }
            if (i == 2) { a = v3; b = v1; return; }

            throw new System.IndexOutOfRangeException();
        }

        public Vector2Int GetVertex(int i) {
            if (i == 0) { return v1; }
            if (i == 1) { return v2; }
            if (i == 2) { return v3; }

            throw new System.IndexOutOfRangeException();
        }



        //**********************************************************************************
        // more performant options
        //**********************************************************************************

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TriangleID GetNeighborUnchecked(int i) => i == 0 ? n1 : i == 1 ? n2 : n3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetVertexUnchecked(int i) => i == 0 ? v1 : i == 1 ? v2 : v3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPortalUnchecked(int i, out Vector2Int a, out Vector2Int b) {
            if (i == 0) { a = v1; b = v2; return; }
            if (i == 1) { a = v2; b = v3; return; }
            a = v3; b = v1;
        }

        //**********************************************************************************
        // more performant options
        //**********************************************************************************
    }


    public static class DelaunayAlgortihms {
        public static void Funnel(this List<AStarTriangle> path, Vector2 start, Vector2 end, List<Vector2> result, float radius = 0) {
            if (path.Count == 0) {
                result.Add(start);
                result.Add(end);
                return;
            }

        
            var safety = new LoopSafety(100);
            safety.SetException();

  
            Vector2 apex = start;
            Vector2 left = start;
            Vector2 right = start;

            int apexIndex = 0;
            int leftIndex = 0;
            int rightIndex = 0;
            result.Add(start);

            for (int i = 0; i < path.Count; i++) {
                safety.Next();
                var t = path[i];

                Vector2 nextLeft = (i == path.Count - 1) ? end : (Vector2)t.exitPortalV2;
                Vector2 nextRight = (i == path.Count - 1) ? end : (Vector2)t.exitPortalV1;

                if (radius > 0 && i < path.Count - 1) {
                    // close enough
                    (nextLeft, nextRight) = Geometry.ShortenSegment(nextLeft, nextRight, radius);
                }

                if (Geometry.Orientation(apex, right, nextRight) != TriangleOrientation.clockwise) {
                    if (apex == right || Geometry.Orientation(apex, left, nextRight) != TriangleOrientation.counterClockwise) {
                        right = nextRight;
                        rightIndex = i;
                    }
                    else {
                        result.Add(left);
                        apex = left;
                        apexIndex = leftIndex;

                        right = apex;
                        left = apex;
                        rightIndex = apexIndex;
                        leftIndex = apexIndex;
                        i = apexIndex;
                        continue;
                    }
                }

                // Update Left Side
                if (Geometry.Orientation(apex, left, nextLeft) != TriangleOrientation.counterClockwise) {
                    if (apex == left || Geometry.Orientation(apex, right, nextLeft) != TriangleOrientation.clockwise) {
                        left = nextLeft;
                        leftIndex = i;
                    }
                    else {
                        apex = right;
                        apexIndex = rightIndex;

                        result.Add(right);

                        right = apex;
                        left = apex;

                        rightIndex = apexIndex;
                        leftIndex = apexIndex;
                        i = apexIndex;
                        continue;
                    }
                }
            }

            if (result[result.Count - 1] != end) {
                result.Add(end);
            }
        }
    }

}
