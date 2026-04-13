using UnityEngine;
using System.Collections.Generic;
using LPE.Math;
using Unity.Profiling;

namespace LPE.Legacy {
    public class DelaunayPathfinder {
        static ProfilerMarker pm = new ProfilerMarker("A*");
        static ProfilerMarker pm2 = new ProfilerMarker("A* Priority Queue");
        static ProfilerMarker pm3 = new ProfilerMarker("A* Sub");
        PriorityQueue<DelaunayTriangle> queue = new PriorityQueue<DelaunayTriangle>();
        Dictionary<DelaunayTriangle, AStarCache> cache = new Dictionary<DelaunayTriangle, AStarCache>();
        public Delaunay d;


        public void Set(Delaunay d) {
            this.d = d;
        }

        /// <summary>
        /// Call this whenever the triangulation changes in order to reduce memory (possibly)
        /// </summary>
        public void ClearCache() {
            foreach (var kv in cache) {
                AStarCache.Return(kv.Value);
            }

            cache.Clear();
        }


        public List<DelaunayTriangle> AStar(Vector2 start, Vector2 end, List<DelaunayTriangle> result = null, float radius = 0) {
            float seDist = (start - end).magnitude;
            DelaunayTriangle tstart = d.Point2Triangle(start);
            DelaunayTriangle tend = d.Point2Triangle(end);
            result = result ?? new List<DelaunayTriangle>();

            if (tstart == tend) {
                result.Add(tstart);
                return result;
            }
            radius = radius * radius;
            pm.Begin();
            queue.Clear();


            if (!cache.ContainsKey(tstart)) {
                cache.Add(tstart, AStarCache.Get(tstart));
            }
            if (!cache.ContainsKey(tend)) {
                cache.Add(tend, AStarCache.Get(tend));
            }

            // in case we don't make it to the end, go to closest point instead
            DelaunayTriangle closest = null;
            float closestDist = -1;

            var cs = cache[tstart];
            var ce = cache[tend];
            cs.g = 0;
            cs.h = seDist;

            queue.Add(tstart, 0);

            // cache highest priority triangle outside of pritority queue to avoid slow insert and remove operations O(log n)
            DelaunayTriangle highest = null;
            float highestPriority = 0;

            while (!queue.isEmpty || highest != null) {
                DelaunayTriangle t = highest;
                highest = null;

                if (t == null) {
                    t = queue.Get();
                }

                AStarCache cacheTri = cache[t];

                if (t == tend) {
                    break;
                }

                var t1 = t.e1.t1 == t ? t.e1.t2 : t.e1.t1;
                var t2 = t.e2.t1 == t ? t.e2.t2 : t.e2.t1;
                var t3 = t.e3.t1 == t ? t.e3.t2 : t.e3.t1;

                if (cacheTri.entry != t.e1) CheckNeighbor(t1, t.e1, cacheTri.entry);
                if (cacheTri.entry != t.e2) CheckNeighbor(t2, t.e2, cacheTri.entry);
                if (cacheTri.entry != t.e3) CheckNeighbor(t3, t.e3, cacheTri.entry);

                void CheckNeighbor(DelaunayTriangle n, DelaunayEdge e, DelaunayEdge prevEntry) {
                    // out of bounds
                    if (n == null) {
                        return;
                    }
                    bool validWidth;
                    if (prevEntry != null) {
                        //TODO - check if third edge of input triangle is a constraint
                        DelaunayVertex vvSame = null;

                        if (prevEntry.v1 == e.v1) {
                            vvSame = prevEntry.v1;
                        }
                        else if (prevEntry.v1 == e.v2) {
                            vvSame = prevEntry.v1;
                        }
                        else if (prevEntry.v2 == e.v1) {
                            vvSame = prevEntry.v2;
                        }
                        else if (prevEntry.v2 == e.v2) {
                            vvSame = prevEntry.v2;
                        }

                        //float w = Geometry.TriangleAltitudeSqr(vvSame.pos, vv1.pos, vv2.pos);
                        validWidth = cacheTri.GetAltitude(vvSame) >= radius * 4;
                    }
                    else {
                        validWidth = true;
                    }


                    if (e.IsConstraint || !validWidth) {
                        return;
                    }

                    Vector2 v1 = e.v1.pos;
                    Vector2 v2 = e.v2.pos;
                    AStarCache cacheN;

                    if (!cache.ContainsKey(n)) {
                        cacheN = AStarCache.Get(n);
                        cache.Add(n, cacheN);
                    }
                    else {
                        cacheN = cache[n];
                    }
                    cacheN.h = Mathf.Sqrt(Mathf.Min((end - v1).sqrMagnitude, (end - v2).sqrMagnitude));


                    // if 2 constaints, invalid triangle since there is no way out (unless it is the end)
                    bool validConstraints = n == tend || n.NumConstrainedEdges() < 2;
                    if (validConstraints) {
                        // estimate g
                        float g = Mathf.Sqrt(Mathf.Min((start - v1).sqrMagnitude, (start - v2).sqrMagnitude));


                        if (seDist - cacheN.h > g) {
                            g = seDist - cacheN.h;
                        }

                        if (cacheTri.f - cacheN.h > g) {
                            g = cacheTri.f - cacheN.h;
                        }

                        if (cacheTri.g > g) {
                            g = cacheTri.g;
                        }

                        if (g < cacheN.g) {
                            cacheN.prev = t;
                            cacheN.entry = e;
                            cacheN.g = g;

                            var priority = -cacheN.f;


                            if (queue.size != 0 && priority <= queue.PeekPriority()) {
                                //pm2.Begin();
                                queue.Add(n, priority);
                                //pm2.End();
                            }
                            else {
                                // store in priority cache
                                if (highest == null) {
                                    highest = n;
                                    highestPriority = priority;
                                }
                                else {
                                    // check against existing
                                    if (priority > highestPriority) {
                                        //pm2.Begin();
                                        queue.Add(highest, highestPriority);
                                        //pm2.End();
                                        highest = n;
                                        highestPriority = priority;
                                    }
                                    else {
                                        //pm2.Begin();
                                        queue.Add(n, priority);
                                        //pm2.End();
                                    }
                                }
                            }
                        }
                    }

                    // check if is closet to end point 
                    if (cacheN.h < closestDist) {
                        closest = n;
                        closestDist = cacheN.h;
                    }
                }
            }

            //Debug.Log(count);
            //Debug.Log(maxQueueSize);
            if (ce.prev == null) {
                tend = closest;
            }


            BackTrack(tend, result, cache);


            foreach (var kv in cache) {
                kv.Value.prev = null;
                kv.Value.entry = null;
                kv.Value.g = Mathf.Infinity;
                //kv.Value.h = -1;
            }

            pm.End();
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

        public List<DelaunayTriangle> AStar(Vector2 start, Vector2 end, int ignoreConstrainID, List<DelaunayTriangle> result = null, float radius = 0) {
            float seDist = (start - end).magnitude;
            DelaunayTriangle tstart = d.Point2Triangle(start);
            DelaunayTriangle tend = d.Point2Triangle(end);
            result = result ?? new List<DelaunayTriangle>();

            if (tstart == tend) {
                result.Add(tstart);
                return result;
            }
            radius = radius * radius;
            pm.Begin();
            queue.Clear();
            //cache.Clear();

            if (!cache.ContainsKey(tstart)) {
                cache.Add(tstart, AStarCache.Get(tstart));
            }
            if (!cache.ContainsKey(tend)) {
                cache.Add(tend, AStarCache.Get(tend));
            }

            // in case we don't make it to the end, go to closest point instead
            DelaunayTriangle closest = null;
            float closestDist = -1;

            var cs = cache[tstart];
            var ce = cache[tend];
            cs.g = 0;
            cs.h = seDist;

            queue.Add(tstart, 0);

            // cache highest priority triangle outside of pritority queue to avoid slow insert and remove operations O(log n)
            DelaunayTriangle highest = null;
            float highestPriority = 0;

            while (!queue.isEmpty || highest != null) {
                DelaunayTriangle t = highest;
                highest = null;

                if (t == null) {
                    t = queue.Get();
                }

                AStarCache ct = cache[t];

                if (t == tend) {
                    break;
                }

                var t1 = t.e1.t1 == t ? t.e1.t2 : t.e1.t1;
                var t2 = t.e2.t1 == t ? t.e2.t2 : t.e2.t1;
                var t3 = t.e3.t1 == t ? t.e3.t2 : t.e3.t1;

                if (ct.entry != t.e1) CheckNeighbor(t1, t.e1, ct.entry);
                if (ct.entry != t.e2) CheckNeighbor(t2, t.e2, ct.entry);
                if (ct.entry != t.e3) CheckNeighbor(t3, t.e3, ct.entry);

                void CheckNeighbor(DelaunayTriangle n, DelaunayEdge e, DelaunayEdge prevEntry) {
                    // out of bounds
                    if (n == null) {
                        return;
                    }
                    bool validWidth;
                    if (prevEntry != null) {
                        //TODO - check if third edge of input triangle is a constraint
                        DelaunayVertex vvSame = null;

                        if (prevEntry.v1 == e.v1) {
                            vvSame = prevEntry.v1;
                        }
                        else if (prevEntry.v1 == e.v2) {
                            vvSame = prevEntry.v1;
                        }
                        else if (prevEntry.v2 == e.v1) {
                            vvSame = prevEntry.v2;
                        }
                        else if (prevEntry.v2 == e.v2) {
                            vvSame = prevEntry.v2;
                        }

                        //float w = Geometry.TriangleAltitudeSqr(vvSame.pos, vv1.pos, vv2.pos);
                        validWidth = ct.GetAltitude(vvSame) >= radius * 4;
                    }
                    else {
                        validWidth = true;
                    }

                    bool validConstraint = !e.IsConstraint || e.constraintID == ignoreConstrainID;
                    if (!validConstraint || !validWidth) {
                        return;
                    }

                    Vector2 v1 = e.v1.pos;
                    Vector2 v2 = e.v2.pos;
                    AStarCache c;

                    if (!cache.ContainsKey(n)) {
                        c = AStarCache.Get(n);
                        cache.Add(n, c);
                    }
                    else {
                        c = cache[n];
                    }
                    c.h = Mathf.Sqrt(Mathf.Min((end - v1).sqrMagnitude, (end - v2).sqrMagnitude));


                    // if 2 constaints, invalid triangle since there is no way out (unless it is the end)
                    bool validConstraints = n == tend || n.NumConstrainedEdges(ignoreConstrainID) < 2;
                    if (validConstraints) {
                        // estimate g
                        float g = Mathf.Sqrt(Mathf.Min((start - v1).sqrMagnitude, (start - v2).sqrMagnitude));


                        if (seDist - c.h > g) {
                            g = seDist - c.h;
                        }

                        if (ct.f - c.h > g) {
                            g = ct.f - c.h;
                        }

                        if (ct.g > g) {
                            g = ct.g;
                        }

                        if (g < c.g) {
                            c.prev = t;
                            c.entry = e;
                            c.g = g;

                            var priority = -c.f;


                            if (queue.size != 0 && priority <= queue.PeekPriority()) {
                                //pm2.Begin();
                                queue.Add(n, priority);
                                //pm2.End();
                            }
                            else {
                                // store in priority cache
                                if (highest == null) {
                                    highest = n;
                                    highestPriority = priority;
                                }
                                else {
                                    // check against existing
                                    if (priority > highestPriority) {
                                        //pm2.Begin();
                                        queue.Add(highest, highestPriority);
                                        //pm2.End();
                                        highest = n;
                                        highestPriority = priority;
                                    }
                                    else {
                                        //pm2.Begin();
                                        queue.Add(n, priority);
                                        //pm2.End();
                                    }
                                }
                            }
                        }
                    }

                    // check if is closet to end point 
                    if (c.h < closestDist) {
                        closest = n;
                        closestDist = c.h;
                    }
                }
            }

            //Debug.Log(count);
            //Debug.Log(maxQueueSize);
            if (ce.prev == null) {
                tend = closest;
            }


            BackTrack(tend, result, cache);


            foreach (var kv in cache) {
                kv.Value.prev = null;
                kv.Value.entry = null;
                kv.Value.g = Mathf.Infinity;
                //kv.Value.h = -1;
            }

            pm.End();
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

        class AStarCache {
            static ObjectPool<AStarCache> _pool = new ObjectPool<AStarCache>(() => new AStarCache());
            public static AStarCache Get(DelaunayTriangle t) { var result = _pool.Get(); result.Init(t); return result; }
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

            DelaunayVertex v1;
            DelaunayVertex v2;
            DelaunayVertex v3;

            float altitude1;
            float altitude2;
            float altitude3;


            private AStarCache() {
            }

            void Init(DelaunayTriangle t) {
                prev = null;
                entry = null;
                g = Mathf.Infinity;
                h = -1;
                v1 = t.v1;
                v2 = t.v2;
                v3 = t.v3;

                altitude1 = -1;
                altitude2 = -1;
                altitude3 = -1;
            }

            public float GetAltitude(DelaunayVertex v) {
                if (v == v1) {
                    if (altitude1 < 0) {
                        altitude1 = Geometry.TriangleAltitudeSqr(v1.pos, v2.pos, v3.pos);
                    }

                    return altitude1;
                }
                if (v == v2) {
                    if (altitude2 < 0) {
                        altitude2 = Geometry.TriangleAltitudeSqr(v2.pos, v1.pos, v3.pos);
                    }

                    return altitude2;
                }
                if (v == v3) {
                    if (altitude3 < 0) {
                        altitude3 = Geometry.TriangleAltitudeSqr(v3.pos, v2.pos, v1.pos);
                    }

                    return altitude3;
                }
                throw new System.ArgumentException("Vertex does not belong");
            }
        }
    }
}
