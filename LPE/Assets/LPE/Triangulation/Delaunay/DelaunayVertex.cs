using System.Collections.Generic;
using UnityEngine;

namespace LPE.Triangulation {
    public class DelaunayVertex {
        public Vector2 pos;
        public HashSet<DelaunayEdge> edges = new HashSet<DelaunayEdge>();
        public bool super = false;
        public float x => pos.x;
        public float y => pos.y;


        public override int GetHashCode() {
            return pos.GetHashCode();
        }

        public bool HasEdge(DelaunayEdge e) {
            return e.v1 == this || e.v2 == this;
        }
        public bool ConnectedTo(DelaunayVertex v) {
            foreach (var e in edges) {
                if (e.v1 == v || e.v2 == v) {
                    return true;
                }
            }
            return false;
        }
    }
}