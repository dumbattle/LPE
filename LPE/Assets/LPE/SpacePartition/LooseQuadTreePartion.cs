using System.Collections.Generic;
using UnityEngine;

namespace LPE.SpacePartition {
    public class LooseQuadTreePartion<T> {
        static ObjectPool<LooseQuadTreePartion<T>> partitionPool;
        static LooseQuadTreePartion() {
            partitionPool = new ObjectPool<LooseQuadTreePartion<T>>(() => new LooseQuadTreePartion<T>());
        }


        public LooseQuadTreePartion<T> q1 { get; private set; }
        public LooseQuadTreePartion<T> q2 { get; private set; }
        public LooseQuadTreePartion<T> q3 { get; private set; }
        public LooseQuadTreePartion<T> q4 { get; private set; }


        public bool leaf => q1 == null;
        public int count => _shapes.Count;

        public Vector2 min { get; private set; }
        public Vector2 max { get; private set; }

        Vector2 boundsMin;
        Vector2 boundsMax;

        int maxDepth;
        int maxOccupants = 1;

        Dictionary<T, (Vector2 min, Vector2 max)> _shapes = new Dictionary<T, (Vector2 min, Vector2 max)>();

        public LooseQuadTreePartion() { }

        void CalculateBounds() {
            if (leaf) {
                boundsMin = new Vector2(-1, -1);
                boundsMax = new Vector2(-1, -1);

                if (_shapes.Count == 0) {
                    return;
                }
                else {
                    float minX = float.MaxValue;
                    float maxX = float.MinValue;

                    float minY = float.MaxValue;
                    float maxY = float.MinValue;

                    foreach (var s in _shapes) {
                        (Vector2 smin, Vector2 smax) = s.Value;

                        minX = smin.x < minX ? smin.x : minX;
                        maxX = smax.x > maxX ? smax.x : maxX;

                        minY = smin.y < minY ? smin.y : minY;
                        maxY = smax.y > maxY ? smax.y : maxY;
                    }

                    boundsMin = new Vector2(minX, minY);
                    boundsMax = new Vector2(maxX, maxY);
                }
            }
            else {
                float minX = float.MaxValue;
                float maxX = float.MinValue;

                float minY = float.MaxValue;
                float maxY = float.MinValue;

                if (q1.boundsMax.x != -1) {
                    minX = minX > q1.boundsMin.x ? q1.boundsMin.x : minX;
                    maxX = maxX < q1.boundsMax.x ? q1.boundsMax.x : maxX;

                    minY = minY > q1.boundsMin.y ? q1.boundsMin.y : minY;
                    maxY = maxY < q1.boundsMax.y ? q1.boundsMax.y : maxY;
                }
                if (q2.boundsMax.x != -1) {
                    minX = minX > q2.boundsMin.x ? q2.boundsMin.x : minX;
                    maxX = maxX < q2.boundsMax.x ? q2.boundsMax.x : maxX;

                    minY = minY > q2.boundsMin.y ? q2.boundsMin.y : minY;
                    maxY = maxY < q2.boundsMax.y ? q2.boundsMax.y : maxY;
                }
                if (q3.boundsMax.x != -1) {
                    minX = minX > q3.boundsMin.x ? q3.boundsMin.x : minX;
                    maxX = maxX < q3.boundsMax.x ? q3.boundsMax.x : maxX;

                    minY = minY > q3.boundsMin.y ? q3.boundsMin.y : minY;
                    maxY = maxY < q3.boundsMax.y ? q3.boundsMax.y : maxY;
                }
                if (q4.boundsMax.x != -1) {
                    minX = minX > q4.boundsMin.x ? q4.boundsMin.x : minX;
                    maxX = maxX < q4.boundsMax.x ? q4.boundsMax.x : maxX;

                    minY = minY > q4.boundsMin.y ? q4.boundsMin.y : minY;
                    maxY = maxY < q4.boundsMax.y ? q4.boundsMax.y : maxY;
                }

                boundsMin = new Vector2(minX, minY);
                boundsMax = new Vector2(maxX, maxY);
            }
        }
        public void Initialize(Vector2 regionMin, Vector2 regionMax, int maxDepth, int maxOccupants) {
            min = regionMin;
            max = regionMax;

            boundsMin = new Vector2(-1, -1);
            boundsMax = new Vector2(-1, -1);

            this.maxDepth = maxDepth;
            this.maxOccupants = maxOccupants;


            _shapes.Clear();
        }

        public void UpdateShape(T s, (Vector2 min, Vector2 max) aabb) {
            Vector2 p = (min + max) / 2;

            var c = _shapes.ContainsKey(s);

            if (c) {
                if (Overlap(min, max, p)) {
                    if (!leaf) {
                        q1.UpdateShape(s, aabb);
                        q2.UpdateShape(s, aabb);
                        q3.UpdateShape(s, aabb);
                        q4.UpdateShape(s, aabb);
                    }
                }
                else {
                    RemoveShape(s);
                }

                CalculateBounds();
            }
            else {
                if (Overlap(min, max, p)) {
                    AddShape(s, aabb);
                }
            }
        }

        public void AddShape(T s, (Vector2 min, Vector2 max) aabb) {
            _shapes.Add(s, aabb);

            if (leaf) {
                if (_shapes.Count > maxOccupants && maxDepth > 0) {
                    SubPartition();
                }
            }
            else {
                var p = (min + max) / 2;
                if (Overlap(q1.min, q1.max, p)) {
                    q1.AddShape(s, aabb);
                }
                if (Overlap(q2.min, q2.max, p)) {
                    q2.AddShape(s, aabb);
                }
                if (Overlap(q3.min, q3.max, p)) {
                    q3.AddShape(s, aabb);
                }
                if (Overlap(q4.min, q4.max, p)) {
                    q4.AddShape(s, aabb);
                }
            }

            CalculateBounds();
        }


        public void RemoveShape(T s) {
            if (!_shapes.ContainsKey(s)) {
                return;
            }
            _shapes.Remove(s);

            if (!leaf) {
                q1.RemoveShape(s);
                q2.RemoveShape(s);
                q3.RemoveShape(s);
                q4.RemoveShape(s);
            }
            CalculateBounds();
        }

        public void CleanUp() {
            if (leaf) {
                return;
            }

            if (_shapes.Count <= maxOccupants) {
                ReturnChildren();
            }
            else {
                q1.CleanUp();
                q2.CleanUp();
                q3.CleanUp();
                q4.CleanUp();
            }
        }

        void Return() {
            ReturnChildren();
            partitionPool.Return(this);
        }

        private void ReturnChildren() {
            q1?.Return();
            q2?.Return();
            q3?.Return();
            q4?.Return();

            q1 = null;
            q2 = null;
            q3 = null;
            q4 = null;
        }

      
        public void GetOverlap((Vector2 min, Vector2 max) aabb, HashSet<T> results) {
            if (_shapes.Count == 0) {
                return;
            }


            if (leaf) {
                foreach (var s2 in _shapes) {
                    if (Math.Geometry.AABBIntersection(aabb.min, aabb.max, s2.Value.min, s2.Value.max)) { 
                        results.Add(s2.Key);
                    }
                }
            }
            else {
                if (Overlap(aabb.min, aabb.max, q1.boundsMin, q1.boundsMax)) {
                    q1.GetOverlap(aabb, results);
                }
                if (Overlap(aabb.min, aabb.max, q2.boundsMin, q2.boundsMax)) {
                    q2.GetOverlap(aabb, results);
                }
                if (Overlap(aabb.min, aabb.max, q3.boundsMin, q3.boundsMax)) {
                    q3.GetOverlap(aabb, results);
                }
                if (Overlap(aabb.min, aabb.max, q4.boundsMin, q4.boundsMax)) {
                    q4.GetOverlap(aabb, results);
                }
            }
        }

        void SubPartition() {
            q1 = partitionPool.Get();
            q2 = partitionPool.Get();
            q3 = partitionPool.Get();
            q4 = partitionPool.Get();

            float midY = (min.y + max.y) / 2;
            float midX = (min.x + max.x) / 2;

            Vector2 center = (min + max) / 2;
            q1.Initialize(center, max, maxDepth - 1, maxOccupants);
            q2.Initialize(
                    new Vector2(min.x, midY),
                    new Vector2(midX, max.y), maxDepth - 1, maxOccupants);
            q3.Initialize(min, center, maxDepth - 1, maxOccupants);
            q4.Initialize(
                    new Vector2(midX, min.y),
                    new Vector2(max.x, midY), maxDepth - 1, maxOccupants);


            foreach (var kv in _shapes) {
                var t = kv.Key;
                var aabb = kv.Value;

                var p = (aabb.min + aabb.max) / 2;

                if (Overlap(q1.min, q1.max, p)) {
                    q1.AddShape(t, aabb);
                }
                if (Overlap(q2.min, q2.max, p)) {
                    q2.AddShape(t, aabb);
                }
                if (Overlap(q3.min, q3.max, p)) {
                    q3.AddShape(t, aabb);
                }
                if (Overlap(q4.min, q4.max, p)) {
                    q4.AddShape(t, aabb);
                }
            }
        }

        public void OnDrawGizmos() {
            if (leaf && _shapes.Count == 0) {
                return;
            }
            q1?.OnDrawGizmos();
            q2?.OnDrawGizmos();
            q3?.OnDrawGizmos();
            q4?.OnDrawGizmos();

            Gizmos.color = Color.blue;

            Vector2 br = new Vector2(boundsMax.x, boundsMin.y);
            Vector2 tl = new Vector2(boundsMin.x, boundsMax.y);

            Gizmos.DrawLine(boundsMax, br);
            Gizmos.DrawLine(br, boundsMin);
            Gizmos.DrawLine(boundsMin, tl);
            Gizmos.DrawLine(tl, boundsMax);
        }

        static bool Overlap(Vector2 mina, Vector2 maxa, Vector2 minb, Vector2 maxb) {
            bool result =
                mina.x <= maxb.x &&
                mina.y <= maxb.y &&
                maxa.x >= minb.x &&
                maxa.y >= minb.y;
            return result;
        }

        static bool Overlap(Vector2 min, Vector2 max, Vector2 p) {
            //pm6.Begin();
            bool result =
                min.x <= p.x &&
                min.y <= p.y &&
                max.x >= p.x &&
                max.y >= p.y;
            //pm6.End();
            return result;
        }
    }


}
