using UnityEngine;
using System.Collections.Generic;


namespace LPE.Math {
    public static partial class Geometry {
        public static bool IsRaySegment(Vector2 o, Vector2 d, Vector2 a, Vector2 b) {
            Vector2 v1 = o - a;
            Vector2 v2 = b - a;
            Vector2 v3 = new Vector2(-d.y, d.x);

            float dot = Vector2.Dot(v2, v3);
            if (Mathf.Abs(dot) < 0.000001)
                return false;

            float t1 = (v2.x * v1.y - v1.x * v2.y) / dot;
            float t2 = Vector2.Dot(v1, v3) / dot;

            if (t1 >= 0.0 && t2 >= 0.0 && t2 <= 1.0) {
                return true;
            }

            return false;
        }
    
        public static bool OnSegment(Vector2 p, Vector2 a, Vector2 b) {
            if (a == b) {
                return false;
            }
            if (a == p || p == b) {
                return true;
            }

            // verticals
            if (Mathf.Abs(b.x - a.x) <= 0.0001f) {
                if ((p.x - a.x) != 0) {
                    return false;
                }
                var rr = (p.y - a.y) / (b.y - a.y);
                return rr >= 0 && rr <= 1;
            }

            // check slopes
            if (!Mathf.Approximately((b.y - a.y) * (p.x - a.x), (p.y - a.y) * (b.x - a.x))) {
                return false;
            }

            //    0 < (p.x - a.x) < (b.x - a.x)
            var rp = (p.x - a.x);
            var rb = (b.x - a.x);
            var r = rp / rb;
            return 0 <= r && r <= 1;
        }
     
        /// <summary>
        /// Casts circle (c1,r1) along dir. Return dist to collision with (c2,r2)
        /// -1 if no collision
        /// returns distance as a fraction of dir.magnitude, not in absolute units
        /// </summary>
        public static float CircleCast_Circle(Vector2 c1, float r1, Vector2 c2, float r2, Vector2 dir) {
            double r = r1 + r2;
            double x1 = c1.x;
            double x2 = c2.x;
            double y1 = c1.y;
            double y2 = c2.y;
            double dx = x2 - x1;
            double dy = y2 - y1;
            double c = dx * dx + dy * dy - r * r;

            // already colliding
            if (c <  - .001f) {
                return 0;
            }
            double a = dir.x * dir.x +  dir.y * dir.y;
            double b = -2 * dx * dir.x - 2 * dy * dir.y;

            var (s1, _) = SolveQuadratic(a, b, c);
            if (double.IsNaN(s1) || s1 < 0) {
                return -1;
            }

            return (float)s1;
        }

        /// <summary>
        /// ax^2 + bx + c. Smaller value is returned first. (x, NaN) => 1 Solution. (NaN, NaN) => no solution
        /// </summary>
        public static (double, double) SolveQuadratic(double a, double b, double c) {
            const float eps = .00001f;
            double det = b * b - 4 * a * c;

            if (det <-eps) {
                return (double.NaN, double.NaN);
            }

            if (det > eps) {
                det = System.Math.Sqrt(det);
                var sgn = Sign(b);

                var s1 = (-b - sgn * det) / (2 * a);
                var s2 =  2 * c / ((-b - sgn * det) );

                if (s1 > s2) {
                    return (s2, s1);
                }
                else {
                    return (s1, s2);
                }
            }

            return (-b / (2 * a), double.NaN);

            int Sign(double d) {
                return d >= 0 ? 1 : -1;
            }
        }
    
        public static (Vector2 a, Vector2 b)ShortenSegment(Vector2 a, Vector2 b, float amnt) {
            if (amnt == 0) {
                return (a, b);
            }
            var dir = (b - a).normalized * amnt;
            return (a + dir, b - dir);
        }
   
        public static Vector2 Rotate(Vector2 point, float degrees) {
            float cosRot = Mathf.Cos(Mathf.Deg2Rad * degrees);
            float sinRot = Mathf.Sin(Mathf.Deg2Rad * degrees);

            return new Vector2(
                point.x * cosRot - point.y * sinRot,
                point.y * cosRot + point.x * sinRot
            );
        }

        /// <summary>
        /// Rotates points in-place around orgin
        /// </summary>
        public static void Rotate(Vector2[] points, float degrees) {
            float cosRot = Mathf.Cos(Mathf.Deg2Rad * degrees);
            float sinRot = Mathf.Sin(Mathf.Deg2Rad * degrees);

            for (int i = 0; i < points.Length; i++) {
                Vector2 point = points[i];

                points[i] = new Vector2(
                    point.x * cosRot - point.y * sinRot,
                    point.y * cosRot + point.x * sinRot
                );
            }

        }

        /// <summary>
        /// Has 3 modes.  Flexible, but probably very inefficient (compared to optimal)
        /// </summary>
        /// <param name="s">Will be first in results list</param>
        /// <param name="e">will be last in results list</param>
        /// <param name="results">If null, a new list will be allocated</param>
        /// <param name="mode">1 - skinny line. 2 - 4-way connections. 3 - corner hit=> all 4 tiles added</param>
        /// <returns>Provided results list, or a new one if not provided.  Points will be ordered</returns>
        public static List<Vector2Int> GetLine(Vector2 s, Vector2 e, List<Vector2Int> results, int mode = 0) {
            results ??= new List<Vector2Int>();

            // cache
            Vector2Int start = new Vector2Int(Mathf.FloorToInt(s.x), Mathf.FloorToInt(s.y));
            Vector2Int end = new Vector2Int(Mathf.FloorToInt(e.x), Mathf.FloorToInt(e.y));

            Vector2 dir = e - s;
            Vector2 invDir = new Vector2(1f / dir.x, 1f / dir.y);
            int dx = dir.x == 0 ? 0 : dir.x > 0 ? 1 : -1;
            int dy = dir.y == 0 ? 0 : dir.y > 0 ? 1 : -1;

            bool yOverX = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
            bool xOverY = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

            // seed algorithm
            Vector2Int last = start;
            results.Add(start);

            int safety = 100000; // don't trust this to work
            while (true) {
                safety--;
                if (safety < 0) {
                    Debug.Log("Warning possible infinite loop detected, breaking loop");
                    break;
                }

                // redundant?
                if (last == end) {
                    break;
                }

                // next possible tiles
                Vector2Int nx = last + new Vector2Int(dx, 0);
                Vector2Int ny = last + new Vector2Int(0, dy);
                Vector2Int nxy = last + new Vector2Int(dx, dy);

                // check tiles for intersections
                bool xHit = dx != 0 && LPE.Math.Geometry.RayAABBIntersection((nx, nx + Vector2Int.one), s, invDir);
                bool xyHit = LPE.Math.Geometry.RayAABBIntersection((nxy, nxy + Vector2Int.one), s, invDir);
                bool yHit = dy != 0 && LPE.Math.Geometry.RayAABBIntersection((ny, ny + Vector2Int.one), s, invDir);

                // Mode 0 = skinny, only 1 tile should be selected
                if (mode <= 0) {
                    if (xyHit) {
                        // hit corner, always go with diagonal
                        if (xHit && yHit) {
                            yHit = false;
                            xHit = false;
                        }
                        if (xHit) {
                            float y = dir.y > 0 ? nx.y + 1 : nx.y;
                            float ty = y - s.y;
                            float tx = ty * dir.x / dir.y;
                            float x = tx + s.x;
                            x = x - Mathf.Floor(x);
                            if (x > 0.5f || yOverX) {
                                xHit = false;
                            }
                            else {
                                xyHit = false;
                            }
                        }
                        if (yHit) {
                            float x = dir.x > 0 ? ny.x + 1 : ny.x;
                            float tx = x - s.x;
                            float ty = tx * dir.y / dir.x;
                            float y = ty + s.y;
                            y = y - Mathf.Floor(y);

                            if (y > 0.5f || xOverY) {
                                yHit = false;
                            }
                            else {
                                xyHit = false;
                            }
                        }
                    }
                }

                // Mode 1, 2 tiles can be selected
                if (mode == 1) {
                    if (xyHit) {
                        if (xHit && yHit) {
                            yHit = yOverX || !xOverY;
                            xHit = xOverY;
                        }
                    }
                }

                // if adjacent is end, exit early to stop diagonal from being added
                if (nx == end) {
                    results.Add(end);
                    break;
                }
                if (ny == end) {
                    results.Add(end);
                    break;
                }

                // add hits
                if (xHit) {
                    results.Add(nx);
                }

                if (yHit) {
                    results.Add(ny);
                }

                if (xyHit) {
                    results.Add(nxy);
                }

                // which tile should we continue from
                // mode = 2 is tricky
                if (xOverY && mode >= 2) {
                    last =
                        xHit ? nx :
                        nxy;
                }
                else if (yOverX && mode >= 2) {
                    last =
                        yHit ? ny :
                        nxy;

                }
                else {
                    // default to diagonal
                    last =
                        xyHit ? nxy :
                        yHit ? ny :
                        nx;
                }

                // redundant?
                if (nxy == end) {
                    break;
                }
            }

            return results;
        }

    }
}

