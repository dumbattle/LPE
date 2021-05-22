using UnityEngine;

namespace LPE.Triangulation {
    public static class Utility {
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
        public static bool InTriangle(Vector2 pt, Vector2 t1, Vector2 t2, Vector2 t3) {
            const float EPS = .00000001f;
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, t1, t2);
            d2 = sign(pt, t2, t3);
            d3 = sign(pt, t3, t1);

            has_neg = (d1 < -EPS) || (d2 < -EPS) || (d3 < -EPS);
            has_pos = (d1 > EPS) || (d2 > EPS) || (d3 > EPS);

            return !(has_neg && has_pos);

            float sign(Vector2 p1, Vector2 p2, Vector2 p3) {
                return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }

        }
        public static bool InCircumcircle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
            if (IsClockwise(v1, v2, v3)) {
                var temp = v1;
                v1 = v2;
                v2 = temp;
            }

            float a = v1.x - pt.x;
            float b = v1.y - pt.y;
            float c = a * a + b * b;
            float d = v2.x - pt.x;
            float e = v2.y - pt.y;
            float f = d * d + e * e;
            float g = v3.x - pt.x;
            float h = v3.y - pt.y;
            float i = g * g + h * h;
            /*
             * a b c 
             * d e f
             * g h i
             */
            return a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g) > 0;

        }
        public static double InCircumcircleF(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
            if (IsClockwise(v1, v2, v3)) {
                var temp = v1;
                v1 = v2;
                v2 = temp;
            }

            double a = v1.x - pt.x;
            double b = v1.y - pt.y;
            double c = a * a + b * b;
            double d = v2.x - pt.x;
            double e = v2.y - pt.y;
            double f = d * d + e * e;
            double g = v3.x - pt.x;
            double h = v3.y - pt.y;
            double i = g * g + h * h;
            /*
             * a b c 
             * d e f
             * g h i
             */
            return a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);

        }

        public static bool IsClockwise(Vector2 v1, Vector2 v2, Vector2 v3) {

            return v1.x * v2.y + v3.x * v1.y + v2.x * v3.y <
                   v3.x * v2.y + v1.x * v3.y + v2.x * v1.y;
        }
        public static bool IsIntersecting(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            if (a == c || a == d || b == c || b == d) {
                return false;
            }
            const float EPS = 1e-10f;
            bool result = false;


            float denom = (d.y - c.y) * (b.x - a.x) - (d.x - c.x) * (b.y - a.y);

            if (denom > EPS || denom < -EPS) {
                float u_a = ((d.x - c.x) * (a.y - c.y) - (d.y - c.y) * (a.x - c.x)) / denom;
                float u_b = ((b.x - a.x) * (a.y - c.y) - (b.y - a.y) * (a.x - c.x)) / denom;
                float zero = EPS;
                float one = 1f - EPS;

                //Are intersecting if u_a and u_b are between 0 and 1
                if (u_a > zero && u_a < one && u_b > zero && u_b < one) {
                    result = true;
                }
            }

            return result;

        }

        public static bool RaySegment(Vector2 o, Vector2 d, Vector2 a, Vector2 b) {

            //d.Normalize();
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

    }

}