namespace LPE.Triangulation {
    public class DelaunayTriangle {
        public DelaunayEdge e1;
        public DelaunayEdge e2;
        public DelaunayEdge e3;

        public DelaunayVertex v1;
        public DelaunayVertex v2;
        public DelaunayVertex v3;

        public bool super => v1.super || v2.super || v3.super;
        public bool hasConstraint => e1.IsConstraint || e2.IsConstraint || e3.IsConstraint;

        public void SetVertices() {
            v1 = e1.v1;
            v2 = e1.v2;
            v3 = e3.v1;

            v3 = v3 == v2 || v3 == v1 ? e3.v2 : v3;
        }
        public void MakeClockwise(bool value) {
            if (IsClockwise() ^ value) {
                var t = v1;
                v1 = v2;
                v2 = t;
            }
        }
        public bool IsClockwise() {
            return IsClockwise(v1, v2, v3);
        }

        public bool InCircumcircle(DelaunayVertex v) {
            MakeClockwise(false);
            double a = v1.x - v.x;
            double b = v1.y - v.y;
            double c = a * a + b * b;
            double d = v2.x - v.x;
            double e = v2.y - v.y;
            double f = d * d + e * e;
            double g = v3.x - v.x;
            double h = v3.y - v.y;
            double i = g * g + h * h;
            /*
             * a b c 
             * d e f
             * g h i
             */
            return a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g) > 0.00001;
        }
   
    
        public static bool IsClockwise(DelaunayVertex v1, DelaunayVertex v2, DelaunayVertex v3) {
            return v1.x * v2.y + v3.x * v1.y + v2.x * v3.y <
                   v3.x * v2.y + v1.x * v3.y + v2.x * v1.y;

        }

    }
}