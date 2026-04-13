using UnityEngine;
using System.Collections.Generic;
using LPE.Math;

namespace LPE.Legacy {
    public static class DelaunayAlgorithms {
        static List<Portal> portals = new List<Portal>();
     
        /// <summary>
        /// Not precise around corners when radius is >0. Buffer radius to avoid getting stuck.
        /// Will include start and end positions
        /// </summary>
        public static List<Vector2> Funnel(List<DelaunayTriangle> channel, Vector2 start, Vector2 end, List<Vector2> result = null, float radius = 0) {
            result = result ?? new List<Vector2>();
            if (channel.Count <= 1) {
                result.Add(start);
                result.Add(end);
                return result;
            }
            var last = end;
            bool containsEnd = Geometry.InTriangle(last, channel[channel.Count - 1].v1.pos, channel[channel.Count - 1].v2.pos, channel[channel.Count - 1].v3.pos);
            if (!containsEnd) {
                last = Geometry.ClosestOnTriangle(last, channel[channel.Count - 1].v1.pos, channel[channel.Count - 1].v2.pos, channel[channel.Count - 1].v3.pos);
            }


            portals.Clear();
            DelaunayEdge prevEdge = null;
            (Vector2 v1, Vector2 v2) prevShortened = default;

            for (int i = 0; i < channel.Count - 1; i++) {
                DelaunayTriangle t1 = channel[i];
                DelaunayTriangle t2 = channel[i + 1];
                DelaunayEdge edge = t1.e1 == t2.e1 || t1.e1 == t2.e2 || t1.e1 == t2.e3 ? t1.e1 :
                                    t1.e2 == t2.e1 || t1.e2 == t2.e2 || t1.e2 == t2.e3 ? t1.e2 :
                                    t1.e3 == t2.e1 || t1.e3 == t2.e2 || t1.e3 == t2.e3 ? t1.e3 : null;

                var (v1, v2) = Geometry.ShortenSegment(edge.v1.pos, edge.v2.pos, radius);
                Gizmos.color = Color.blue;

                if (radius > 0 && prevEdge != null) {
                    if (prevEdge.v1 == edge.v1) {
                        portals.Add(new Portal(prevShortened.v1, v2));
                    }
                    else if (prevEdge.v1 == edge.v2) {
                        portals.Add(new Portal(prevShortened.v1, v1));
                    }
                    else if (prevEdge.v2 == edge.v2) {
                        portals.Add(new Portal(prevShortened.v2, v1));
                    }
                    else {
                        portals.Add(new Portal(prevShortened.v2, v2));
                    }
                }
                portals.Add(new Portal(v1, v2));
                prevEdge = edge;
                prevShortened = (v1, v2);
            }

            result.Add(start);

            var ind = 0;

            var p = portals[ind];
            var s = start;
            var a = p.v1;
            var b = p.v2;
            var pa = a;
            var pb = b;

            var ra = Geometry.IsClockwise(s, a, b);
            var rb = Geometry.IsClockwise(s, b, a);


            MainLoop();

            result.Add(last);

            if (!containsEnd) {
                result.Add(end);
            }

            return result;

            void MainLoop() {
                var safety = new LoopSafety(1000);
                while (ind < portals.Count && ind >= 0 && safety.Next()) {
                    ind++;
                    if (ind >= portals.Count) {
                        break;
                    }

                    p = portals[ind];

                    bool aSide = pa == p.v1 || pa == p.v2;

                    Vector2 vnext =
                        aSide
                        ? p.v1 == pa
                            ? p.v2
                            : p.v1
                        : p.v1 == pb
                            ? p.v2
                            : p.v1;

                    if (aSide) {
                        //advance b
                        pb = vnext;

                        // wrong way
                        if (Geometry.IsClockwise(s, b, vnext) != rb) {
                            continue;
                        }

                        // crossover
                        if (Geometry.IsClockwise(s, vnext, a) != rb) {
                            AddVertex(a, s);

                            ind = -1;
                            for (int i = portals.Count - 1; i >= 0; i--) {
                                var val = portals[i];
                                if (val.v1 == a || val.v2 == a) {
                                    break;
                                }
                                ind = i;
                            }

                            if (ind == -1) {
                                break;
                            }

                            p = portals[ind];
                            a = p.v1;
                            b = p.v2;
                            pa = a;
                            pb = b;
                            ra = Geometry.IsClockwise(s, a, b);
                            rb = Geometry.IsClockwise(s, b, a);
                            continue;
                        }
                        b = vnext;
                    }
                    else {
                        pa = vnext;
                        //advance a

                        // wrong way
                        if (Geometry.IsClockwise(s, a, vnext) != ra) {
                            continue;
                        }

                        // crossover
                        if (Geometry.IsClockwise(s, vnext, b) != ra) {
                            AddVertex(b, s);

                            ind = -1;
                            for (int i = portals.Count - 1; i >= 0; i--) {
                                var val = portals[i];
                                if (val.v1 == b || val.v2 == b) {
                                    break;
                                }
                                ind = i;
                            }

                            if (ind == -1) {
                                break;
                            }

                            p = portals[ind];
                            a = p.v1;
                            b = p.v2;
                            pa = a;
                            pb = b;
                            ra = Geometry.IsClockwise(s, a, b);
                            rb = Geometry.IsClockwise(s, b, a);
                            continue;
                        }

                        a = vnext;
                    }
                }

                EndIter();

            }

            void EndIter() {
                // crossover
                if (Geometry.IsClockwise(s, a, last) != ra) {
                    AddVertex(a, s);

                    ind = -1;
                    for (int i = portals.Count - 1; i >= 0; i--) {
                        var val = portals[i];
                        if (val.v1 == a || val.v2 == a) {
                            break;
                        }
                        ind = i;
                    }

                    if (ind == -1) {
                        return;
                    }

                    p = portals[ind];
                    a = p.v1;
                    b = p.v2;
                    pa = a;
                    pb = b;
                    ra = Geometry.IsClockwise(s, a, b);
                    rb = !ra;
                    MainLoop();
                }
                if (Geometry.IsClockwise(s, b, last) != rb) {
                    AddVertex(b, s);


                    ind = -1;
                    for (int i = portals.Count - 1; i >= 0; i--) {
                        var val = portals[i];
                        if (val.v1 == b || val.v2 == b) {
                            break;
                        }
                        ind = i;
                    }

                    if (ind == -1) {
                        return;
                    }

                    p = portals[ind];
                    a = p.v1;
                    b = p.v2;
                    pa = a;
                    pb = b;
                    ra = Geometry.IsClockwise(s, a, b);
                    rb = Geometry.IsClockwise(s, b, a);
                    MainLoop();
                }

            }

            void AddVertex(Vector2 v, Vector2 src) {
                Vector2 pos = v;
                //bool first = true;
                //bool cc = true;

                //foreach (var e in portals) {
                //    Vector2 a;

                //    if (e.v1 == v) {
                //        a = e.v1;
                //    }
                //    else if (e.v2 == v) {
                //        a = e.v2;
                //    }
                //    else {
                //        continue;
                //    }

                //    if (first) {
                //        first = false;
                //        cc = Geometry.IsClockwise(src, a, pos);
                //        pos = a;
                //    }
                //    else {
                //        var cc2 = Geometry.IsClockwise(src, a, pos);
                //        if (cc == cc2) {
                //            pos = a;
                //        }
                //        else {
                //            result.Add(pos);
                //            src = pos;
                //            pos = a;
                //        }
                //    }
                //}

                s = pos;
                result.Add(s);
            }
        }
    }

    struct Portal {
        public Vector2 v1;
        public Vector2 v2;

        public Portal(Vector2 v1, Vector2 v2) {
            this.v1 = v1; 
            this.v2 = v2;
        }
    }

}
