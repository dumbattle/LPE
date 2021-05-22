namespace LPE.Triangulation {
    public static class DelaunayUtitiliy {
        public static DelaunayTriangle CreateTriangle(DelaunayVertex a, DelaunayVertex b, DelaunayVertex c) {
            var e1 = GetOrCreateEdge(a, b);
            var e2 = GetOrCreateEdge(b, c);
            var e3 = GetOrCreateEdge(c, a);
            DelaunayTriangle result = new DelaunayTriangle();
            result.e1 = e1;
            result.e2 = e2;
            result.e3 = e3;
            try {
                e1.AddTriangle(result);
            }
            catch (System.Exception e) {
                UnityEngine.Debug.Log("AB");
                throw;
            }
            try {
                e2.AddTriangle(result);
            }
            catch (System.Exception e) {
                UnityEngine.Debug.Log("BC");
                throw;
            }
            try {
                e3.AddTriangle(result);
            }
            catch (System.Exception e) {
                UnityEngine.Debug.Log("CA");
                throw;
            }
            result.SetVertices();
            result.MakeClockwise(false);

            return result;
        }
        public static void RemoveTriangle(DelaunayTriangle t) {
            var e1 = t.e1;
            var e2 = t.e2;
            var e3 = t.e3;

            e1.RemoveTriangle(t);
            e2.RemoveTriangle(t);
            e3.RemoveTriangle(t);
            TryRemoveEdge(e1);
            TryRemoveEdge(e2);
            TryRemoveEdge(e3);
        }

        static void TryRemoveEdge(DelaunayEdge e) {
            if (e.t1 != null || e.t2 != null) {
                return;
            }
            e.v1.edges.Remove(e);
            e.v2.edges.Remove(e);
        }
        public static DelaunayEdge GetOrCreateEdge(DelaunayVertex a, DelaunayVertex b) {
            foreach (var e in a.edges) {
                if (e.v1 == b || e.v2 == b) {
                    return e;
                }
            }
            DelaunayEdge result = new DelaunayEdge();
            result.v1 = a;
            result.v2 = b;
            a.edges.Add(result);
            b.edges.Add(result);
            return result;
        
        }
        
        public static bool EdgeCmp(DelaunayEdge a, DelaunayEdge b) {
            return (a.v1 == b.v1 && a.v2 == b.v2) || 
                   (a.v1 == b.v2 && a.v2 == b.v1);
        }
    }

}