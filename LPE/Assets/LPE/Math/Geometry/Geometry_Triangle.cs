using UnityEngine;
namespace LPE.Math {
    public static partial class Geometry {
      
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
     
    }
}