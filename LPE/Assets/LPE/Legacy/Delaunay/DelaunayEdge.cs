using System;

namespace LPE.Legacy {
    public class DelaunayEdge {
        public DelaunayVertex v1 { get; private set; }
        public DelaunayVertex v2 { get; private set; }

        public DelaunayTriangle t1 { get; private set; }
        public DelaunayTriangle t2 { get; private set; }
        public int constraintID { get; private set; }
        public bool IsConstraint { get; private set; }


        public void SetAsConstraint(int id) {
            IsConstraint = true;
            constraintID = id;
        }

        public void SetVertices(DelaunayVertex v1, DelaunayVertex v2) {
            this.v1 = v1;
            this.v2 = v2;
        }
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