using System;

namespace LPE.Triangulation {
    public class DelaunayEdge {
        public DelaunayVertex v1;
        public DelaunayVertex v2;

        public DelaunayTriangle t1;
        public DelaunayTriangle t2;
        public bool IsConstraint = false;

        public void AddTriangle(DelaunayTriangle t) {
            if (t1 == t || t2 == t) {
                throw new InvalidOperationException("Edge already contains triangle");
            }
            if (t1 == null) {
                t1 = t;
                return;
            }
            if (t2 == null) {
                t2 = t;
                return;
            }
            throw new InvalidOperationException($"Edge({v1.pos}, {v2.pos}) already hase 2 triangles");

        }

        public void RemoveTriangle(DelaunayTriangle t) {
            if (t1 == t) {
                t1 = null;
                return;
            }

            if (t2 == t) {
                t2 = null;
                return;
            }

            throw new InvalidOperationException("Edge does not contain triangle");
        }
    }
}