using UnityEngine;
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
        public static bool IsColinear(Vector2 p, Vector2 a, Vector2 b) {
            if (a == b) {
                return true;
            }
            if (a == p || p == b) {
                return true;
            }

            // verticals
            if (Mathf.Abs(b.x - a.x) <= 0.0001f) {
                return Mathf.Abs(p.x - a.x) <= 0.0001f;
            }

            // check slopes
            if (!Mathf.Approximately((b.y - a.y) * (p.x - a.x), (p.y - a.y) * (b.x - a.x))) {
                return false;
            }

            return true;

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
       
        
        public static bool IsIntersecting(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2) {
                return false;
            }
            const float EPS = 1e-10f;
            bool result = false;


            float denom = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);

            if (denom > EPS || denom < -EPS) {
                float u_a = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denom;
                float u_b = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denom;
                float zero = EPS;
                float one = 1f - EPS;

                //Are intersecting if u_a and u_b are between 0 and 1
                if (u_a > zero && u_a < one && u_b > zero && u_b < one) {
                    result = true;
                }
            }

            return result;

        }


        /// <summary>
        /// Circle - Circle intersection
        /// touch => should touching be counted as intersecting
        /// </summary>
        public static bool IsIntersecting(Vector2 c1, float r1, Vector2 c2, float r2, bool touch = false) {
            if (touch) {
                return (c1 - c2).sqrMagnitude <= (r1 + r2) * (r1 + r2);
            }
            else {
                return (c1 - c2).sqrMagnitude <  (r1 + r2) * (r1 + r2);
            }
        }
       
        
        /// <summary>
        /// Casts circle (c1,r1) along dir. Return dist to collision with (c2,r2)
        /// -1 if no collision
        /// -2 if started colliding
        /// returns distance as a fraction of dir.magnitude, not in absolute units
        /// </summary>
        public static float CircleCast_Circle(Vector2 c1, float r1, Vector2 c2, float r2, Vector2 dir) {
            double r = r1 + r2;
            double x1 = c1.x;
            double x2 = c2.x;
            double y1 = c1.y;
            double y2 = c2.y;
            // already colliding
            if (x1 * x2 + y1 * y2 < r * r - .001f) {
                return -2;
            }
            double x = c2.x - c1.x;
            double y = c2.y - c1.y;
            double a = dir.x * dir.x +  dir.y * dir.y;
            double b = -2 * x * dir.x - 2 * y * dir.y;
            double c = x * x + y * y - r * r;

            var (s1, _) = SolveQuadratic(a, b, c);
            if (s1 == null || s1 < 0) {
                return -1;
            }

            return (float)s1.Value;
        }

        /// <summary>
        /// ax^2 + bx + c
        /// </summary>
        public static (double?, double?) SolveQuadratic(double a, double b, double c) {
            const float eps = .00001f;
            double det = b * b - 4 * a * c;

            if (det <-eps) {
                return (null, null);
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

            return (-b / (2 * a), null);

            int Sign(double d) {
                return d >= 0 ? 1 : -1;
            }
        }
    
    
        public static bool AABBIntersection(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax) {
            return
                amin.x < bmax.x &&
                amax.x > bmin.x &&
                amin.y < bmax.y &&
                amax.y > bmin.y;
        }
    
        public static (Vector2 a, Vector2 b)ShortenSegment(Vector2 a, Vector2 b, float amnt) {
            if (amnt == 0) {
                return (a, b);
            }
            var dir = (b - a).normalized * amnt;
            return (a + dir, b - dir);
        }
   
        public static (Vector2 min, Vector2 max) CircleAABB(Vector2 pos, float r) {
            return (
                pos - new Vector2(r,r),
                pos + new Vector2(r,r)
                );
        }
    }
}

