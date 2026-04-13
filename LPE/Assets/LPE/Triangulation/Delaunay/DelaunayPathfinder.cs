using LPE;
using LPE.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LPE.Triangulation {
    public class DelaunayPathfinder {
        IReadOnly_OrthogonalDelaunay delaunay;
        /// <summary>
        /// Stores triangle index
        /// </summary>
        PriorityQueue<int> pQueue = new();


        Dictionary<TriangleID, int> id2index = new();

        // these will be cached
        // invalidated if:
        //  1 - triangle is removed
        //  2 - triangle edges constraints are changed (clearance changes)
        //  3 - neighbors change
        DelaunayTriangleData[] triangleData = { };
        List<TriangleID> triangleIDs = new();

        List<float> clearanceE1 = new(); 
        List<float> clearanceE2 = new(); 
        List<float> clearanceE3 = new();


        // these will not be cached
        List<float> g = new(); // cost
        List<float> h = new(); // heuristic
        List<int> cameFrom = new(); // backtracking

        List<int> lastVisitID = new();
        List<int> lastSeenID = new();
        int currentSearchID = 1;

        public DelaunayPathfinder(IReadOnly_OrthogonalDelaunay delaunay) {
            this.delaunay = delaunay;
        }

        int UpdateTriangleCache(TriangleID id) {
            //using (ProfileHelper.Sample("CACHE")) {
            int index = -1;
            if (!id2index.TryGetValue(id, out index)) {
                index = triangleIDs.Count;

                // add data slot
                id2index.Add(id, index);

                if (index >= triangleData.Length) {
                    Array.Resize(ref triangleData, triangleData.Length * 2 + 1);
                }
                triangleIDs.Add(TriangleID.INVALID);
                clearanceE1.Add(-1);
                clearanceE2.Add(-1);
                clearanceE3.Add(-1);
                g.Add(-1);
                h.Add(-1);
                cameFrom.Add(-1);
                lastVisitID.Add(0);
                lastSeenID.Add(0);
            }

            if (triangleIDs[index] != id) {
                triangleIDs[index] = id;
                triangleData[index] = delaunay.GetTriangleData(id, false);
                ref var newData = ref triangleData[index];

                // Compute approximate half-clearance
                // default is length of portal
                // if one of the other edges is a constraint, then must check if can squeeze between that edge and opposite vertex
                // NOT 100% ACCURATE, MUST CHECK OTHER CONSTRAINTS THAT MIGHT BLOCK PASSAGE
                // as a workaround, caller can pad unit radius
                clearanceE1[index] = (newData.v1 - newData.v2).sqrMagnitude;
                clearanceE2[index] = (newData.v2 - newData.v3).sqrMagnitude;
                clearanceE3[index] = (newData.v3 - newData.v1).sqrMagnitude;

                var (alt1, alt2, alt3) = Geometry.TriangleAltitudesSqr(newData.v1, newData.v2, newData.v3);

                var (valid1, valid2, valid3) = Geometry.GetTriangleProjections(newData.v1, newData.v2, newData.v3);
                if (newData.n1 == TriangleID.INVALID) {
                    if (valid3) {
                        clearanceE3[index] = MathF.Min(clearanceE3[index], alt3);
                        clearanceE2[index] = MathF.Min(clearanceE2[index], alt3);
                    }
                }
                if (newData.n2 == TriangleID.INVALID) {
                    if (valid1) {
                        clearanceE1[index] = MathF.Min(clearanceE1[index], alt1);
                        clearanceE3[index] = MathF.Min(clearanceE3[index], alt1);
                    }
                }
                if (newData.n3 == TriangleID.INVALID) {
                    if (valid2) {
                        clearanceE1[index] = MathF.Min(clearanceE1[index], alt2);
                        clearanceE2[index] = MathF.Min(clearanceE2[index], alt2);
                    }
                }
            }

            return index;
            //}
        }

        public void AStar(Vector2 start, Vector2 end, List<AStarTriangle> result, float radius = 0) {
            //using (ProfileHelper.Sample("AStar")) {
                var safety = new LoopSafety(1000);
                safety.SetException();

                currentSearchID++;
                var startEndDist = (start - end).magnitude;
                int finalTriangle = -1;

                using (var scopedTSet = ObjectPool.GetScopedHashSet<TriangleID>()) {
                    float finalTriangleDist = float.PositiveInfinity;

                    var tSet = scopedTSet.item;
                    delaunay.GetTriangles(start, tSet);

                    foreach (var tid in tSet) {
                        // 1. Initialize start triangles and per-search arrays 
                        var t = UpdateTriangleCache(tid);
                        g[t] = 0;
                        h[t] = startEndDist;

                        cameFrom[t] = -1;

                        // 2. Push start into priority queue
                        pQueue.Add(t, -g[t] - h[t]);
                        lastSeenID[t] = currentSearchID; // flags triangle as initialized
                    }

                    while (pQueue.size > 0) {
                        safety.Next();

                        // - Pop best triangle t
                        var t = pQueue.Get();

                        if (lastVisitID[t] == currentSearchID) {
                            continue;
                        }

                        ref var data = ref triangleData[t];

                        // - If t contains end -> done 
                        // TODO - Store end triangles in set and perform quicker lookup
                        if (Geometry.InTriangle(end, data.v1, data.v2, data.v3)) {
                            finalTriangle = t;
                            break;
                        }
                        // tarck best in case of failure
                        if (h[t] < finalTriangleDist) {
                            finalTriangle = t;
                            finalTriangleDist = h[t];
                        }
                        // this triangle is done
                        lastVisitID[t] = currentSearchID;

                        for (int nIndex = 0; nIndex < 3; nIndex++) {
                            var nid = data.GetNeighbor(nIndex);
                            if (nid == TriangleID.INVALID) {
                                continue;
                            }
                            var n = UpdateTriangleCache(nid);

                            if (lastVisitID[n] == currentSearchID) {
                                continue;
                            }


                            ref var nData = ref triangleData[n];

                            // Check clearance for this portal 
                            float ct = -1;
                            float cn = -1;

                            Vector2 ev1 = new Vector2();
                            Vector2 ev2 = new Vector2();

                            if (data.n1 == nid) {
                                ct = clearanceE1[t];
                                ev1 = data.v1;
                                ev2 = data.v2;
                            }
                            else if (data.n2 == nid) {
                                ct = clearanceE2[t];
                                ev1 = data.v2;
                                ev2 = data.v3;
                            }
                            else if (data.n3 == nid) {
                                ct = clearanceE3[t];
                                ev1 = data.v3;
                                ev2 = data.v1;
                            }


                            var tID = triangleIDs[t];
                            if (nData.n1 == tID) { cn = clearanceE1[n]; }
                            else if (nData.n2 == tID) { cn = clearanceE2[n]; }
                            else if (nData.n3 == tID) { cn = clearanceE3[n]; }

                            float maxClear = MathF.Min(ct, cn);

                            if (maxClear < radius * radius * 4) {
                                continue;
                            }

                            // If n not visited, initialize per-search state
                            if (lastSeenID[n] != currentSearchID) {
                                lastSeenID[n] = currentSearchID;
                                g[n] = float.PositiveInfinity;
                            }

                            // get entry point
                            //using (ProfileHelper.Sample("GCOST")) {

                                // Compute new g-cost using portal geometry
                                float gCost = g[t] + (nData.center - data.center).magnitude;
                                // If g is better, update and push to queue 
                                if (gCost < g[n]) {
                                    g[n] = gCost;
                                    h[n] = (nData.center - end).magnitude;

                                    cameFrom[n] = t;

                                    pQueue.Add(n, -g[n] - h[n]);
                                }
                            //}

                        }
                    }
                //}



                // 4. Backtrack from best triangle
                int resultStartIndex = result.Count;
                int length = 0;

                int currentT = finalTriangle;
                TriangleID nextT = TriangleID.INVALID;
                while (safety.Next()) {
                    if (currentT < 0) {
                        break;
                    }

                    Vector2Int v1 = new();
                    Vector2Int v2 = new();


                    ref var d = ref triangleData[currentT];
                    if (d.n1 == nextT) {
                        v1 = d.v1;
                        v2 = d.v2;
                    }
                    else if (d.n2 == nextT) {
                        v1 = d.v2;
                        v2 = d.v3;
                    }
                    else if (d.n3 == nextT) {
                        v1 = d.v3;
                        v2 = d.v1;
                    }


                    nextT = triangleIDs[currentT];
                    // exit portal will be invalid for last triangle (first triangle processed, last triangle in result)
                    // No easy way to enforce, but we can document
                    result.Add(new AStarTriangle(triangleIDs[currentT], v1, v2));
                    length++;
                    currentT = cameFrom[currentT];
                }

                // If for some reason caller has other data in the results list, we don't want to modify those
                // (maybe they want to create a path with multiple waypoints)
                result.Reverse(resultStartIndex, length);
            }
        }
    }

    public struct AStarTriangle {
        public readonly TriangleID triangleID;

    
        public readonly Vector2Int exitPortalV1;
        public readonly Vector2Int exitPortalV2;


     
        public AStarTriangle(TriangleID triangleID, Vector2Int exitPortalV1, Vector2Int exitPortalV2) {
            this.triangleID = triangleID;
            this.exitPortalV1 = exitPortalV1;
            this.exitPortalV2 = exitPortalV2;   
        }
    }
}
