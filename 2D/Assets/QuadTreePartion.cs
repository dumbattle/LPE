using System.Collections.Generic;
using UnityEngine;


namespace LPE2D {
    public class QuadTreePartion {
        public QuadTreePartion parent { get; private set; }
        public QuadTreePartion q1 { get; private set; }
        public QuadTreePartion q2 { get; private set; }
        public QuadTreePartion q3 { get; private set; }
        public QuadTreePartion q4 { get; private set; }

        public bool leaf => q1 == null;

        public Vector2 min { get; private set; }
        public Vector2 max { get; private set; }

        int maxDepth;
        int maxOccupants = 1;

        HashSet<Shape2D> _shapes = new HashSet<Shape2D>();


        public QuadTreePartion (Vector2 regionMin, Vector2 regionMax, int maxDepth = 5, int maxOccupants = 1) {
            min = regionMin;
            max = regionMax;
            this.maxDepth = maxDepth;
            this.maxOccupants = maxOccupants;       
        }

        
        public void AddShape(Shape2D s) {
            _shapes.Add(s);

            if (leaf) {
                if (_shapes.Count > maxOccupants && maxDepth > 0) {
                    SubPartition();
                    RePartition();
                }
            }
            else {
                if (Overlap(s.AABB(), (q1.min, q1.max))) {
                    q1.AddShape(s);
                }
                if (Overlap(s.AABB(), (q2.min, q2.max))) {
                    q2.AddShape(s);
                }
                if (Overlap(s.AABB(), (q3.min, q3.max))) {
                    q3.AddShape(s);
                }
                if (Overlap(s.AABB(), (q4.min, q4.max))) {
                    q4.AddShape(s);
                }
            }
        }

        public void RemoveShape(Shape2D s) {
            _shapes.Remove(s);
            if (!leaf) {
                q1.RemoveShape(s);
                q2.RemoveShape(s);
                q3.RemoveShape(s);
                q4.RemoveShape(s);
            }
        }

        public void CleanUp() {
            if (leaf) {
                return;
            }
            
            if (_shapes.Count <= maxOccupants) {
                q1 = null;
                q2 = null;
                q3 = null;
                q4 = null;
            }
            else {
                q1.CleanUp();
                q2.CleanUp();
                q3.CleanUp();
                q4.CleanUp();
            }
        }

        public bool IsColliding(Shape2D s) {
            if (!_shapes.Contains(s)) {
                return false; ; 
            }


            if (leaf) {
                foreach (var s2 in _shapes) {
                    if (s2 == s) {
                        continue;
                    }

                    if (s.CheckCollision(s2)) {
                        return true;
                    }
                }
            }
            else {
                return q1.IsColliding(s) || q2.IsColliding(s) || q3.IsColliding(s) || q4.IsColliding(s);
            }

            return false;
        }
        public IEnumerable<Shape2D> GetOverlap(Shape2D s) {
            if (!_shapes.Contains(s)) {
                yield break;
            }


            if (leaf) {
                foreach (var s2 in _shapes) {
                    if (s2 == s) {
                        continue;
                    }

                    if (s.CheckCollision(s2)) {
                        yield return s2;
                    }
                }
            }
            else {
                foreach (var s2 in q1.GetOverlap(s)) {
                    yield return s2;
                }
                foreach (var s2 in q2.GetOverlap(s)) {
                    yield return s2;
                }
                foreach (var s2 in q3.GetOverlap(s)) {
                    yield return s2;
                }
                foreach (var s2 in q4.GetOverlap(s)) {
                    yield return s2;
                }
            }

        }


        void SubPartition() {
            q1 = new QuadTreePartion((min + max) / 2, max, maxDepth - 1, maxOccupants);
            q2 = new QuadTreePartion(
                    new Vector2(min.x, (min.y + max.y) / 2),
                    new Vector2((min.x + max.x) / 2, max.y), maxDepth - 1, maxOccupants);
            q3 = new QuadTreePartion(min, (min + max) / 2, maxDepth - 1, maxOccupants);
            q4 = new QuadTreePartion(
                    new Vector2((min.x + max.x) / 2, min.y),
                    new Vector2(max.x, (min.y + max.y) / 2), maxDepth - 1, maxOccupants);

            q1.parent = this;
            q2.parent = this;
            q3.parent = this;
            q4.parent = this;
        }

        void RePartition() {
            foreach (var s in _shapes) {
                if (Overlap(s.AABB(), (q1.min, q1.max))) {
                    q1.AddShape(s);
                }
                if (Overlap(s.AABB(), (q2.min, q2.max))) {
                    q2.AddShape(s);
                }
                if (Overlap(s.AABB(), (q3.min, q3.max))) {
                    q3.AddShape(s);
                }
                if (Overlap(s.AABB(), (q4.min, q4.max))) {
                    q4.AddShape(s);
                }
            }
        }
       
        public void OnDrawGizmos() {
            if (leaf) {
                return;
            }
            Gizmos.DrawLine(
                new Vector2((min.x + max.x) / 2, min.y),
                new Vector2((min.x + max.x) / 2, max.y));

            Gizmos.DrawLine(
                new Vector2(min.x, (min.y + max.y) / 2),
                new Vector2(max.x, (min.y + max.y) / 2));
            q1.OnDrawGizmos();
            q2.OnDrawGizmos();
            q3.OnDrawGizmos();
            q4.OnDrawGizmos();
        }


        static bool Overlap((Vector2 min, Vector2 max) a, (Vector2 min, Vector2 max) b) {
            return
                !(a.min.x > b.max.x ||
                a.min.y > b.max.y ||
                a.max.x < b.min.x ||
                a.max.y < b.min.y);
        }
    }
}