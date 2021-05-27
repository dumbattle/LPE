using UnityEngine;
using System.Collections.Generic;
using LPE.Math;

namespace LPE.Triangulation {
    public static class DelaunayAlgorithms {
        static List<DelaunayEdge> portals = new List<DelaunayEdge>();
        public static List<Vector2> Funnel(List<DelaunayTriangle> channel, Vector2 start, Vector2 end, List<Vector2> result = null, float radius = 0) {
            result = result ?? new List<Vector2>();
            portals.Clear();
            for (int i = 0; i < channel.Count - 1; i++) {
                DelaunayTriangle t1 = channel[i];
                DelaunayTriangle t2 = channel[i + 1];

                DelaunayEdge edge = t1.e1 == t2.e1 || t1.e1 == t2.e2 || t1.e1 == t2.e3 ? t1.e1 :
                                    t1.e2 == t2.e1 || t1.e2 == t2.e2 || t1.e2 == t2.e3 ? t1.e2 :
                                    t1.e3 == t2.e1 || t1.e3 == t2.e2 || t1.e3 == t2.e3 ? t1.e3 : null;
                portals.Add(edge);

            }

            result.Add(start);

            var ind = 0;

            var p = portals[ind];
            var s = start;
            var a = p.v1;
            var b = p.v2;
            var pa = a;
            var pb = b;

            var ra = Geometry.IsClockwise(s, a.pos, b.pos);
            var rb = !ra;


            MainLoop();

            result.Add(end);



      
            return result;

            void MainLoop() {
            var safety = new LoopSafety(1000);
                while (ind < portals.Count && ind >= 0 && safety.Inc()) {
                    ind++;
                    if (ind >= portals.Count) {
                        break;
                    }

                    p = portals[ind];

                    bool aSide = pa == p.v1 || pa == p.v2;

                    DelaunayVertex vnext =
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
                            AddVertex(a,s);

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
                            ra = Geometry.IsClockwise(s, a.pos, b.pos);
                            rb = !ra;
                            continue;
                        }
                        b = vnext;
                    }
                    else {
                        pa = vnext;
                        //advance a

                        // wrong way
                        if (Geometry.IsClockwise(s, a.pos, vnext.pos) != ra) {
                            continue;
                        }

                        // crossover
                        if (Geometry.IsClockwise(s, vnext.pos, b.pos) != ra) {
                            AddVertex(b,s);

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
                            ra = Geometry.IsClockwise(s, a.pos, b.pos);
                            rb = !ra;
                            continue;
                        }

                        a = vnext;
                    }

                }
           
                EndIter();

            }

            void EndIter() {
                // one more iteration with end
                //advance b
                // wrong way
                if (Geometry.IsClockwise(s, b.pos, end) != rb) {
                    AddVertex(b,s);

                    
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
                    ra = Geometry.IsClockwise(s, a.pos, b.pos);
                    rb = !ra;
                    MainLoop();
                }

                // crossover
                if (Geometry.IsClockwise(s, end, a.pos) != rb) {
                    AddVertex(a,s);

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
                    ra = Geometry.IsClockwise(s, a.pos, b.pos);
                    rb = !ra;
                    MainLoop();
                }
            }
        
            void AddVertex(DelaunayVertex v, Vector2 src) {
                bool first = true;
                Vector2 pos = v.pos;
                bool cc = true; ;
                foreach (var e in portals) {
                    Vector2 a;

                    if(e.v1 == v) {
                        (a, _) = Geometry.ShortenSegment(e.v1.pos,e.v2.pos,radius);
                    }
                    else if ( e.v2 == v) {
                        (a, _) = Geometry.ShortenSegment(e.v2.pos, e.v1.pos, radius);

                    }
                    else {
                        continue;
                    }

                    if (first) {
                        first = false;
                        cc = Geometry.IsClockwise(src, a, pos);
                        pos = a;
                    }
                    else {
                        var cc2 = Geometry.IsClockwise(src, a, pos);
                        if (cc == cc2) {
                            pos = a;
                        }
                        else {
                            result.Add(pos);
                            src = pos;
                            pos = a;
                        }
                    }
                }
                
                s = pos;
                result.Add(s);

            }
        }

    }
}
