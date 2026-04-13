using System;
using UnityEngine;
namespace LPE.Math {
    public static partial class Geometry {
        public static int Dot(this Vector2Int lhs, Vector2Int rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }
        public static (bool v1Projects, bool v2Projects, bool v3Projects) GetTriangleProjections(Vector2Int v1, Vector2Int v2, Vector2Int v3) {
            // 1. Calculate squared lengths once
            long aSqr = (long)(v2.x - v3.x) * (v2.x - v3.x) + (long)(v2.y - v3.y) * (v2.y - v3.y);
            long bSqr = (long)(v3.x - v1.x) * (v3.x - v1.x) + (long)(v3.y - v1.y) * (v3.y - v1.y);
            long cSqr = (long)(v1.x - v2.x) * (v1.x - v2.x) + (long)(v1.y - v2.y) * (v1.y - v2.y);

            // 2. A vertex projects if neither of the other two angles are obtuse
            // We check if the square of the 'side opposite the angle' is greater than the sum of others
            bool v1Obtuse = aSqr > bSqr + cSqr;
            bool v2Obtuse = bSqr > aSqr + cSqr;
            bool v3Obtuse = cSqr > aSqr + bSqr;

            return (
                !v2Obtuse && !v3Obtuse, // V1 projects on Side 2-3
                !v1Obtuse && !v3Obtuse, // V2 projects on Side 3-1
                !v1Obtuse && !v2Obtuse  // V3 projects on Side 1-2
            );
        }
        public static bool DoesVertexProjectOnSide(Vector2 a, Vector2 b, Vector2 c) {
            Vector2 bc = c - b;
            Vector2 ba = a - b;

            float dot = Vector2.Dot(ba, bc);

            float sqrMagBC = bc.sqrMagnitude;
            if (sqrMagBC < Mathf.Epsilon) return false;
            float t = dot / sqrMagBC;
            return t >= 0 && t <= 1;
        }
        public static bool DoesVertexProjectOnSide(Vector2Int a, Vector2Int b, Vector2Int c) {
            Vector2Int bc = c - b;
            Vector2Int ba = a - b;

            int dot = ba.Dot(bc);

            int sqrMagBC = bc.sqrMagnitude;
            return dot >= 0 && dot <= sqrMagBC;
        }
        public static Vector2 ClosestOnSegment(Vector2 p, Vector2 a, Vector2 b) {
            Vector2 ab = b - a;
            float lengthSquared = ab.sqrMagnitude;

            if (lengthSquared == 0){ 
                return a; 
            }

            float t = Vector2.Dot(p - a, ab) / lengthSquared;

            t = Mathf.Clamp01(t);

            return a + t * ab;
        }
        public static Vector2 ClosestOnTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 ap = p - a;

            float d1 = Vector2.Dot(ab, ap);
            float d2 = Vector2.Dot(ac, ap);
            if (d1 <= 0 && d2 <= 0) return a; //#1

            Vector2 bp = p - b;
            float d3 = Vector2.Dot(ab, bp);
            float d4 = Vector2.Dot(ac, bp);
            if (d3 >= 0 && d4 <= d3) return b; //#2

            Vector2 cp = p - c;
            float d5 = Vector2.Dot(ab, cp);
            float d6 = Vector2.Dot(ac, cp);
            if (d6 >= 0 && d5 <= d6) return c; //#3

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0) {
                float v = d1 / (d1 - d3);
                return a + v * ab; //#4
            }

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0) {
                float v = d2 / (d2 - d6);
                return a + v * ac; //#5
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0) {
               float v = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + v * (c - b); //#6
            }

            float denom = 1 / (va + vb + vc);
            float v2 = vb * denom;
            float w = vc * denom;
            return a + v2 * ab + w * ac; //#0
        }
        public static bool InTriangle(Vector2 pt, Vector2Int t1, Vector2Int t2, Vector2Int t3) { 
            return InTriangle(pt, (Vector2)t1, (Vector2)t2, (Vector2)t3);
        }



        public static bool InTriangle(Vector2 pt, Vector2 t1, Vector2 t2, Vector2 t3) {
            const float EPS = .001f;
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
        public static Vector2 GetCircumcenter(Vector2Int p1, Vector2Int p2, Vector2Int p3) {
            // Convert to double for higher precision during calculation
            double x1 = p1.x, y1 = p1.y;
            double x2 = p2.x, y2 = p2.y;
            double x3 = p3.x, y3 = p3.y;

            // Calculate differences
            double x12 = x1 - x2;
            double y12 = y1 - y2;
            double x32 = x3 - x2;
            double y32 = y3 - y2;

            // Calculate squared magnitudes from the origin to simplify Cramer's rule
            double z1 = x1 * x1 + y1 * y1;
            double z2 = x2 * x2 + y2 * y2;
            double z3 = x3 * x3 + y3 * y3;

            double z12 = z1 - z2;
            double z32 = z3 - z2;

            // Calculate the determinant to find if points are collinear
            double det = x12 * y32 - y12 * x32;

            if (System.Math.Abs(det) < 0.000001) {
                Debug.LogError("Vertices are collinear; circumcenter does not exist.");
                return Vector2.zero;
            }

            // Apply Cramer's rule to find the intersection of perpendicular bisectors
            float cx = (float)((z12 * y32 - z32 * y12) / (2 * det));
            float cy = (float)((x12 * z32 - x32 * z12) / (2 * det));

            return new Vector2(cx, cy);
        }
        public static bool InCircumcircle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
            // Translate triangle so pt is at the origin
            double a = (double)v1.x - pt.x;
            double b = (double)v1.y - pt.y;
            double c = a * a + b * b;

            double d = (double)v2.x - pt.x;
            double e = (double)v2.y - pt.y;
            double f = d * d + e * e;

            double g = (double)v3.x - pt.x;
            double h = (double)v3.y - pt.y;
            double i = g * g + h * h;

            // Determinant for circumcircle test
            double det = a * (e * i - f * h)
                        - b * (d * i - f * g)
                        + c * (d * h - e * g);

            // Correct sign if triangle is clockwise
            if (IsClockwise(v1, v2, v3)) det = -det;
            return det > 0;
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


        public static (float, float, float) TriangleAltitudesSqr(Vector2Int a, Vector2Int b, Vector2Int c) {
            float area2 = Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
            float areaSqr4 = area2 * area2;

            int abSqr = (a - b).sqrMagnitude;
            int bcSqr = (b - c).sqrMagnitude;
            int caSqr = (c - a).sqrMagnitude;

            return (
                areaSqr4 / bcSqr, // Altitude to vertex A
                areaSqr4 / caSqr, // Altitude to vertex B
                areaSqr4 / abSqr  // Altitude to vertex C
            );
        }
        public static float TriangleAltitudeSqr(Vector2Int h, Vector2Int a, Vector2Int b) {
            Vector2Int line = b - a;
            int lenSqr = line.sqrMagnitude;
            if (lenSqr == 0) {
                return (h - a).sqrMagnitude;
            }

            int t = (h - a).Dot(line);
            Vector2 projection = a + (Vector2)(t * line) / lenSqr;

            return (h - projection).sqrMagnitude;
        }
        public static float TriangleAltitudeSqr(Vector2 h, Vector2 a, Vector2 b) {
            Vector2 line = b - a;
            float lenSqr = line.sqrMagnitude;
            if (lenSqr == 0) {
                return (h - a).sqrMagnitude;
            }

            float t = Vector2.Dot(h - a, line) / lenSqr;
            Vector2 projection = a + t * line;

            return (h - projection).sqrMagnitude;
        }
        public static float TriangleArea(Vector2 a, Vector2 b, Vector2 c) {
            return 0.5f * Mathf.Abs(a.x * b.y - a.y * b.x + b.x * c.y - b.y * c.x + a.y * c.x - a.x * c.y);
        }
        public static float TriangleAreaSigned(Vector2 a, Vector2 b, Vector2 c) {
            return 0.5f * (a.x * b.y - a.y * b.x + b.x * c.y - b.y * c.x + a.y * c.x - a.x * c.y);
        }


        public static bool IsConvex(Vector2Int v1, Vector2Int v2, Vector2Int v3, Vector2Int v4, bool allowCollinear=false) {
            // A quad is convex if all internal angles are < 180 degrees.
            // For ordered vertices, this means all cross products of consecutive edges 
            // must have the same sign (either all >= 0 or all <= 0).

            long cp1 = CrossProduct(v1, v2, v3);
            long cp2 = CrossProduct(v2, v3, v4);
            long cp3 = CrossProduct(v3, v4, v1);
            long cp4 = CrossProduct(v4, v1, v2);

            // If collinear points are NOT allowed, a zero cross product makes it concave/degenerate.
            if (!allowCollinear) {
                if (cp1 == 0 || cp2 == 0 || cp3 == 0 || cp4 == 0) return false;
            }

            // Check if all signs are consistent (all positive or all negative).
            // Using >= and <= handles the allowCollinear case.
            bool allPos = cp1 >= 0 && cp2 >= 0 && cp3 >= 0 && cp4 >= 0;
            bool allNeg = cp1 <= 0 && cp2 <= 0 && cp3 <= 0 && cp4 <= 0;

            return allPos || allNeg;


            static long CrossProduct(Vector2Int a, Vector2Int b, Vector2Int c) {
                // Using long to prevent overflow during intermediate integer multiplication.
                return (long)(b.x - a.x) * (c.y - b.y) - (long)(b.y - a.y) * (c.x - b.x);
            }
        }
        /// <summary>
        /// Check 2 line segments are intersection
        /// </summary>
        public static bool IsIntersecting(Vector2Int a1, Vector2Int a2, Vector2Int b1, Vector2Int b2, bool requireProperCrossing=false) {
            TriangleOrientation o1 = Orientation(a1, a2, b1);
            TriangleOrientation o2 = Orientation(a1, a2, b2);
            TriangleOrientation o3 = Orientation(b1, b2, a1);
            TriangleOrientation o4 = Orientation(b1, b2, a2);

            if (requireProperCrossing) {
                if (o1 == TriangleOrientation.colinear) {
                    return false;
                }
                if (o2 == TriangleOrientation.colinear) {
                    return false;
                }
                if (o3 == TriangleOrientation.colinear) {
                    return false;
                }
                if (o4 == TriangleOrientation.colinear) {
                    return false;
                }
            }

            // Proper intersection (crossing)
            if (o1 != o2 && o3 != o4)
                return true;

            if (o1 == TriangleOrientation.colinear && OnSegment(a1, b1, a2)) { return true; }
            if (o2 == TriangleOrientation.colinear && OnSegment(a1, b2, a2)) { return true; }
            if (o3 == TriangleOrientation.colinear && OnSegment(b1, a1, b2)) { return true; }
            if (o4 == TriangleOrientation.colinear && OnSegment(b1, a2, b2)) { return true; }

            return false;


            static bool OnSegment(Vector2Int a, Vector2Int q, Vector2Int b) {
                return q.x >= Mathf.Min(a.x, b.x) &&
                       q.x <= Mathf.Max(a.x, b.x) &&
                       q.y >= Mathf.Min(a.y, b.y) &&
                       q.y <= Mathf.Max(a.y, b.y);
            }
        }

        public static bool IsColinear(Vector2Int a, Vector2Int b, Vector2Int c) {
            return Orientation(a, b, c) == TriangleOrientation.colinear;
        }
        public static bool IsClockwise(Vector2Int a, Vector2Int b, Vector2Int c) {
            return Orientation(a, b, c) == TriangleOrientation.clockwise;
        }
        public static bool IsCounterClockwise(Vector2Int a, Vector2Int b, Vector2Int c) {
            return Orientation(a, b, c) == TriangleOrientation.counterClockwise;
        }

        public static bool IsColinear(Vector2 a, Vector2 b, Vector2 c) {
            return Orientation(a, b, c) == TriangleOrientation.colinear;
        }
        public static bool IsClockwise(Vector2 a, Vector2 b, Vector2 c) {
            return Orientation(a, b, c) == TriangleOrientation.clockwise;
        }
        public static bool IsClockwiseDecimal(Vector2 a, Vector2 b, Vector2 c) {
            return OrientationDecimal(a, b, c) == TriangleOrientation.clockwise;
        }
        public static bool IsCounterClockwise(Vector2 a, Vector2 b, Vector2 c) {
            return Orientation(a, b, c) == TriangleOrientation.counterClockwise;
        }



        public static bool IsOnSegment(Vector2 a, Vector2 b, Vector2 pt, bool allowEndpoints = false) {
            if (!IsColinear(a, b, pt)) {
                return false;
            }
            // Step 2: Check if pt is within the bounding box of the segment
            if (!allowEndpoints) {
                if (pt == a) { return false; }
                if (pt == b) { return false; }
            }


            return pt.x >= Mathf.Min(a.x, b.x) && pt.x <= Mathf.Max(a.x, b.x) &&
                   pt.y >= Mathf.Min(a.y, b.y) && pt.y <= Mathf.Max(a.y, b.y);
        }
        public static bool IsOnSegment(Vector2Int a, Vector2Int b, Vector2Int pt, bool allowEndpoints = false) {
            if (!IsColinear(a, b, pt)) {
                return false;
            }
            // Step 2: Check if pt is within the bounding box of the segment
            if (!allowEndpoints) {
                if (pt == a) { return false; }
                if (pt == b) { return false; }
            }


            return pt.x >= Mathf.Min(a.x, b.x) && pt.x <= Mathf.Max(a.x, b.x) &&
                   pt.y >= Mathf.Min(a.y, b.y) && pt.y <= Mathf.Max(a.y, b.y);
        }
     
        public static TriangleOrientation Orientation(Vector2Int a, Vector2Int b, Vector2Int c) {
            long val = ((long)b.y - a.y) * ((long)c.x - b.x)
                     - ((long)b.x - a.x) * ((long)c.y - b.y);

            if (val == 0) {
                return TriangleOrientation.colinear;
            }
            return (val > 0) ? TriangleOrientation.clockwise : TriangleOrientation.counterClockwise;
        }
        public static TriangleOrientation Orientation(Vector2 a, Vector2 b, Vector2 c) {
            double val = ((double)b.y - (double)a.y) * ((double)c.x - (double)b.x)
                     - ((double)b.x - (double)a.x) * ((double)c.y - (double)b.y);
            const double EPS = 0.000001;
            if (val < EPS && val > -EPS) {
                return TriangleOrientation.colinear;
            }
            return (val > 0) ? TriangleOrientation.clockwise : TriangleOrientation.counterClockwise;
        }
        public static TriangleOrientation OrientationDecimal(Vector2 a, Vector2 b, Vector2 c) {
            decimal val = ((decimal)b.y - (decimal)a.y) * ((decimal)c.x - (decimal)b.x)
                     - ((decimal)b.x - (decimal)a.x) * ((decimal)c.y - (decimal)b.y);
            const decimal EPS = 0.0000001M;
            if ( val < EPS && val > -EPS) {
                return TriangleOrientation.colinear;
            }
            return (val > 0) ? TriangleOrientation.clockwise : TriangleOrientation.counterClockwise;
        }
    }

    public enum TriangleOrientation {
        clockwise,
        counterClockwise,
        colinear
    }
}