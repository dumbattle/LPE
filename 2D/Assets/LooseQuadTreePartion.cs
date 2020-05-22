using System.Collections.Generic;
using UnityEngine;
using LPE;

namespace LPE2D {
    public class LooseQuadTreePartion<T> where T : class, IShape2D {
        static ObjectPool<LooseQuadTreePartion<T>> partitionPool;
        static LooseQuadTreePartion() {
            partitionPool = new ObjectPool<LooseQuadTreePartion<T>>(() => new LooseQuadTreePartion<T>());
        }


        public LooseQuadTreePartion<T> q1 { get; private set; }
        public LooseQuadTreePartion<T> q2 { get; private set; }
        public LooseQuadTreePartion<T> q3 { get; private set; }
        public LooseQuadTreePartion<T> q4 { get; private set; }


        public bool leaf => q1 == null;

        public Vector2 min { get; private set; }
        public Vector2 max { get; private set; }

        Vector2 boundsMin;
        Vector2 boundsMax;

        int maxDepth;
        int maxOccupants = 1;

        HashSet<T> _shapes = new HashSet<T>();


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
                        (Vector2 smin, Vector2 smax) = s.shape.AABB();

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

        public void UpdateShape(T s) {
            Vector2 p = s.shape.position;

            (Vector2 min, Vector2 max) aabb = s.shape.AABB();
            if (_shapes.Contains(s)) {
                if (Overlap(min, max, p)) {
                    if (!leaf) {
                        q1.UpdateShape(s);
                        q2.UpdateShape(s);
                        q3.UpdateShape(s);
                        q4.UpdateShape(s);
                    }
                }
                else {
                    RemoveShape(s);
                }
                CalculateBounds();
            }
            else {
                if (Overlap(min, max, p)) {
                    AddShape(s);
                }
            }
        }

        public void AddShape(T s) {
            _shapes.Add(s);

            if (leaf) {
                if (_shapes.Count > maxOccupants && maxDepth > 0) {
                    SubPartition();
                }
            }
            else {
                var p = s.shape.position;
                if (Overlap(q1.min, q1.max, p)) {
                    q1.AddShape(s);
                }
                if (Overlap(q2.min, q2.max, p)) {
                    q2.AddShape(s);
                }
                if (Overlap(q3.min, q3.max, p)) {
                    q3.AddShape(s);
                }
                if (Overlap(q4.min, q4.max, p)) {
                    q4.AddShape(s);
                }
            }

            CalculateBounds();
        }


        public void RemoveShape(T s) {
            if (!_shapes.Contains(s)) {
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

        public bool IsColliding(T s) {
            var aabb = s.shape.AABB();
            if (_shapes.Count == 0 || !Overlap(aabb.min, aabb.max, boundsMin, boundsMax)) {
                return false;
            }


            if (leaf) {
                foreach (var s2 in _shapes) {
                    if (s2 == s) {
                        continue;
                    }

                    if (s.shape.CheckCollision(s2.shape)) {
                        return true;
                    }
                }
                return false;
            }
            else {
                return q1.IsColliding(s) || q2.IsColliding(s) || q3.IsColliding(s) || q4.IsColliding(s);
            }
        }

        public void GetOverlap(IShape2D s, List<T> results) {
            if (_shapes.Count == 0) {
                return;
            }


            (Vector2 aabbmin, Vector2 aabbmax) = s.shape.AABB();
            if (leaf) {
                foreach (var s2 in _shapes) {
                    if (s2 == s) {
                        continue;
                    }

                    if (s.shape.CheckCollision(s2.shape)) {
                        if (!results.Contains(s2)) {
                            results.Add(s2);
                        }
                    }
                }
            }
            else {

                if (Overlap(aabbmin, aabbmax, q1.boundsMin, q1.boundsMax)) {
                    q1.GetOverlap(s, results);
                }
                if (Overlap(aabbmin, aabbmax, q2.boundsMin, q2.boundsMax)) {
                    q2.GetOverlap(s, results);
                }
                if (Overlap(aabbmin, aabbmax, q3.boundsMin, q3.boundsMax)) {
                    q3.GetOverlap(s, results);
                }
                if (Overlap(aabbmin, aabbmax, q4.boundsMin, q4.boundsMax)) {
                    q4.GetOverlap(s, results);
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


            foreach (var s in _shapes) {
                var p = s.shape.position;
                if (Overlap(q1.min, q1.max, p)) {
                    q1.AddShape(s);
                }
                if (Overlap(q2.min, q2.max, p)) {
                    q2.AddShape(s);
                }
                if (Overlap(q3.min, q3.max, p)) {
                    q3.AddShape(s);
                }
                if (Overlap(q4.min, q4.max, p)) {
                    q4.AddShape(s);
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
            //switch (maxDepth % 6) {
            //    case 0:
            //        Gizmos.color = Color.yellow;
            //        break;
            //    case 1:
            //        Gizmos.color = Color.red;
            //        break;
            //    case 2:
            Gizmos.color = Color.blue;
            //        break;
            //    case 3:
            //        Gizmos.color = Color.cyan;
            //        break;
            //    case 4:
            //        Gizmos.color = Color.black;
            //        break;
            //    case 5:
            //        Gizmos.color = Color.white;
            //        break;
            //}
            Vector2 br = new Vector2(boundsMax.x, boundsMin.y);
            Vector2 tl = new Vector2(boundsMin.x, boundsMax.y);

            Gizmos.DrawLine(boundsMax, br);
            Gizmos.DrawLine(br, boundsMin);
            Gizmos.DrawLine(boundsMin, tl);
            Gizmos.DrawLine(tl, boundsMax);
        }

        static bool Overlap(Vector2 mina, Vector2 maxa, Vector2 minb, Vector2 maxb) {
            //bool result = 
            //    !(mina.x > maxb.x ||
            //    mina.y > maxb.y ||
            //    maxa.x < minb.x ||
            //    maxa.y < minb.y);
            bool result =
                mina.x <= maxb.x &&
                mina.y <= maxb.y &&
                maxa.x >= minb.x &&
                maxa.y >= minb.y;
            return result;
        }

        static bool Overlap(Vector2 min, Vector2 max, Vector2 p) {
            //bool result = 
            //    !(mina.x > maxb.x ||
            //    mina.y > maxb.y ||
            //    maxa.x < minb.x ||
            //    maxa.y < minb.y);
            bool result =
                min.x <= p.x &&
                min.y <= p.y &&
                max.x >= p.x &&
                max.y >= p.y;
            return result;
        }
    }
}