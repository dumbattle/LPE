using System.Collections.Generic;
using UnityEngine;
using LPE;
using System.Threading.Tasks;
using System;

namespace LPE2D {
    public class QuadTreePartion<T> where T : class, IShape2D {
        static ObjectPool<QuadTreePartion<T>> partitionPool;
        static QuadTreePartion() {
            partitionPool = new ObjectPool<QuadTreePartion<T>>(() => new QuadTreePartion<T>());
        }
        
        public QuadTreePartion<T> q1 { get; private set; }
        public QuadTreePartion<T> q2 { get; private set; }
        public QuadTreePartion<T> q3 { get; private set; }
        public QuadTreePartion<T> q4 { get; private set; }

        //public QuadTreePartion<T> bottom { get; private set; }
        //public QuadTreePartion<T> top { get; private set; }
        //public QuadTreePartion<T> right { get; private set; }
        //public QuadTreePartion<T> left { get; private set; }

        public bool leaf => q1 == null;

        public Vector2 min { get; private set; }
        public Vector2 max { get; private set; }

        int maxDepth;
        int maxOccupants = 1;

        HashSet<T> _shapes = new HashSet<T>();

        public int ShapeCount() {
            return _shapes.Count;
        }

        public QuadTreePartion() { }

        public void Initialize(Vector2 regionMin, Vector2 regionMax, int maxDepth, int maxOccupants) {
            min = regionMin;
            max = regionMax;

            this.maxDepth = maxDepth;
            this.maxOccupants = maxOccupants;

            //bottom = null;
            //top = null;
            //right = null;
            //left = null;

            _shapes.Clear();
        }

        public void UpdateShape(T s) {
            (Vector2 min, Vector2 max) aabb = s.shape.AABB();
            if (_shapes.Contains(s)) {
                if (Overlap(aabb.min, aabb.max, min, max)) {
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
            }
            else {
                if (Overlap(aabb.min, aabb.max, min, max)) {
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
                (Vector2 min, Vector2 max) aabb = s.shape.AABB();
                if (Overlap(aabb.min, aabb.max, q1.min, q1.max)) {
                    q1.AddShape(s);
                }
                if (Overlap(aabb.min, aabb.max, q2.min, q2.max)) {
                    q2.AddShape(s);
                }
                if (Overlap(aabb.min, aabb.max, q3.min, q3.max)) {
                    q3.AddShape(s);
                }
                if (Overlap(aabb.min, aabb.max, q4.min, q4.max)) {
                    q4.AddShape(s);
                }
            }
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
            if (_shapes.Count == 0 || !Overlap(aabb.min, aabb.max, min, max)) {
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
            }
            else {
                return q1.IsColliding(s) || q2.IsColliding(s) || q3.IsColliding(s) || q4.IsColliding(s);
            }

            return false;
        }
        public void GetOverlap(IShape2D s, List<T> results) {
            results = results ?? new List<T>();

            if (_shapes.Count == 0) {
                return;
            }


            (Vector2 min, Vector2 max) aabb = s.shape.AABB();
            if (leaf || _shapes.Count <= 5) {
                foreach (var s2 in _shapes) {
                    if (s2 == s) {
                        continue;
                    }
                    var a2 = s2.shape.AABB();
                    if (Overlap(a2.min, a2.max, aabb.min, aabb.max) && s.shape.CheckCollision(s2.shape)) {
                        if (!results.Contains(s2)) {
                            results.Add(s2);
                        }
                    }
                }
            }
            else {
                if (Overlap(aabb.min, aabb.max, q1.min, q1.max)) {
                    q1.GetOverlap(s, results);
                }
                if (Overlap(aabb.min, aabb.max, q2.min, q2.max)) {
                    q2.GetOverlap(s, results);
                }
                if (Overlap(aabb.min, aabb.max, q3.min, q3.max)) {
                    q3.GetOverlap(s, results);

                }
                if (Overlap(aabb.min, aabb.max, q4.min, q4.max)) {
                    q4.GetOverlap(s, results);
                }
            }
        }

        //public IEnumerable<QuadTreePartion<T>> TopmostNodes() {
        //    if (leaf) {
        //        yield return this;
        //        yield break;
        //    }

        //    foreach (var p in q1.TopmostNodes()) {
        //        yield return p;
        //    }
        //    foreach (var p in q2.TopmostNodes()) {
        //        yield return p;
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> BottommostNodes() {
        //    if (leaf) {
        //        yield return this;
        //        yield break;
        //    }

        //    foreach (var p in q3.BottommostNodes()) {
        //        yield return p;
        //    }
        //    foreach (var p in q4.BottommostNodes()) {
        //        yield return p;
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> RightmostNodes() {
        //    if (leaf) {
        //        yield return this;
        //        yield break;
        //    }

        //    foreach (var p in q1.RightmostNodes()) {
        //        yield return p;
        //    }
        //    foreach (var p in q4.RightmostNodes()) {
        //        yield return p;
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> LeftmostNodes() {
        //    if (leaf) {
        //        yield return this;
        //        yield break;
        //    }

        //    foreach (var p in q2.LeftmostNodes()) {
        //        yield return p;
        //    }
        //    foreach (var p in q3.LeftmostNodes()) {
        //        yield return p;
        //    }
        //}

        //public IEnumerable<QuadTreePartion<T>> AdjacentTop() {
        //    if (top != null) {
        //        foreach (var t in top.BottommostNodes()) {
        //            if (!(t.min.x >= max.x || t.max.x <= min.x)) {
        //                yield return t;
        //            }
        //        }
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> AdjacentBottom() {
        //    if (bottom != null) {
        //        foreach (var t in bottom.TopmostNodes()) {
        //            if (!(t.min.x >= max.x || t.max.x <= min.x)) {
        //                yield return t;
        //            }
        //        }
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> AdjacentRight() {
        //    if (right != null) {
        //        foreach (var t in right.LeftmostNodes()) {
        //            if (!(t.min.y >= max.y || t.max.y <= min.y)) {
        //                yield return t;
        //            }
        //        }
        //    }
        //}
        //public IEnumerable<QuadTreePartion<T>> Adjacentleft() {

        //    if (left != null) {
        //        foreach (var t in left.RightmostNodes()) {
        //            if (!(t.min.y >= max.y || t.max.y <= min.y)) {
        //                yield return t;
        //            }
        //        }
        //    }
        //}

        //public IEnumerable<QuadTreePartion<T>> AdjacentNodes() {
        //    foreach (var n in AdjacentTop()) {
        //        yield return n;
        //    }
        //    foreach (var n in AdjacentBottom()) {
        //        yield return n;
        //    }
        //    foreach (var n in AdjacentRight()) {
        //        yield return n;
        //    }
        //    foreach (var n in Adjacentleft()) {
        //        yield return n;
        //    }
        //}


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
                var a = s.shape.AABB();
                if (Overlap(a.min, a.max, q1.min, q1.max)) {
                    q1.AddShape(s);
                }
                if (Overlap(a.min, a.max, q2.min, q2.max)) {
                    q2.AddShape(s);
                }
                if (Overlap(a.min, a.max, q3.min, q3.max)) {
                    q3.AddShape(s);
                }
                if (Overlap(a.min, a.max, q4.min, q4.max)) {
                    q4.AddShape(s);
                }
            }
        }

        public void OnDrawGizmos() {
            if (leaf) {
                //Gizmos.color = Color.black;
                //Vector2 center = (min + max) / 2;
                //foreach (var t in AdjacentTop()) {
                //    Gizmos.DrawLine(
                //        center,
                //        (t.min + t.max) / 2);
                //}
                //foreach (var t in AdjacentRight()) {
                //    Gizmos.DrawLine(
                //        center,
                //        (t.min + t.max) / 2);
                //}
                return;
            }

            if (_shapes.Count == 0) {
                return;
            }
            q1?.OnDrawGizmos();
            q2?.OnDrawGizmos();
            q3?.OnDrawGizmos();
            q4?.OnDrawGizmos();

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(
                new Vector2((min.x + max.x) / 2, min.y),
                new Vector2((min.x + max.x) / 2, max.y));

            Gizmos.DrawLine(
                new Vector2(min.x, (min.y + max.y) / 2),
                new Vector2(max.x, (min.y + max.y) / 2));
        }

        static bool Overlap(Vector2 mina, Vector2 maxa, Vector2 minb, Vector2 maxb) {
            bool result =
                mina.x <= maxb.x &&
                mina.y <= maxb.y &&
                maxa.x >= minb.x &&
                maxa.y >= minb.y;
            return result;
        }
    }


    public class QuadTree<T >where T: class{
        struct qtElement {
            public int nextElement;
            public T value;
            public AABB bounds;

            public qtElement(qtElement other) {
                nextElement = other.nextElement;
                value = other.value;
                bounds = new AABB(other.bounds.min, other.bounds.max);
            }
        }
        struct qtNode {
            public int firstChild;
            public int firstElement;

            public int count;
            public int nextFree;

            public bool isLeaf => firstChild < 0;

            
            public qtNode(qtNode copy) {
                firstChild = copy.firstChild;
                firstElement = copy.firstElement;
                count = copy.count;
                nextFree = copy.nextFree;             
            }

            public static qtNode New() {
                return new qtNode { firstChild = -1, firstElement = -1, count = 0, nextFree = -1 };
            }
        }

        public AABB bounds;
        public int maxDepth;
        public int maxElements;

        int freeElement = -1;
        int freeNode = -1;

        List<qtNode> nodes = new List<qtNode>();
        List<qtElement> elements = new List<qtElement>();

        Dictionary<T, AABB> elementDict = new Dictionary<T, AABB>();

        public QuadTree(AABB bounds, int maxDepth, int maxElements) {
            this.bounds = bounds;
            this.maxDepth = maxDepth;
            this.maxElements = maxElements;

            nodes.Add(qtNode.New());
        }

        public void Add(T element, AABB aabb) {
            if (elementDict.ContainsKey(element)) {
                throw new System.InvalidOperationException();
            }
            InsertToNode(0, bounds, 0, element, aabb);
            elementDict.Add(element, aabb);

            void InsertToNode(int n, AABB nAABB, int depth, T item, AABB iAABB) {
                if (depth > maxDepth) {
                    throw new Exception();
                }

                var eIdx = GetFreeElement();
                elements[eIdx] = new qtElement() {
                    bounds = iAABB,
                    nextElement = nodes[n].firstElement,
                    value = item
                };
                nodes[n] = new qtNode(nodes[n]) {
                    count = nodes[n].count + 1,
                    firstElement = eIdx
                };


                if (nodes[n].isLeaf) {
                    if (nodes[n].count + 1 > maxElements && depth < maxDepth) {
                        SubDivideNode(n);

                        int firstChild = nodes[n].firstChild;
                        int e = nodes[n].firstElement;

                        while (e >= 0) {
                            if (elements[e].bounds.Intersects(GetChildAABB(nAABB, 0))) {
                                InsertToNode(firstChild + 0, GetChildAABB(nAABB, 0), depth + 1, elements[e].value, elements[e].bounds);
                            }
                            if (elements[e].bounds.Intersects(GetChildAABB(nAABB, 1))) {
                                InsertToNode(firstChild + 1, GetChildAABB(nAABB, 1), depth + 1, elements[e].value, elements[e].bounds);
                            }
                            if (elements[e].bounds.Intersects(GetChildAABB(nAABB, 2))) {
                                InsertToNode(firstChild + 2, GetChildAABB(nAABB, 2), depth + 1, elements[e].value, elements[e].bounds);
                            }
                            if (elements[e].bounds.Intersects(GetChildAABB(nAABB, 3))) {
                                InsertToNode(firstChild + 3, GetChildAABB(nAABB, 3), depth + 1, elements[e].value, elements[e].bounds);
                            }

                            e = elements[e].nextElement;
                        }
                    }
                }
                else {
                    int firstChild = nodes[n].firstChild;
                    if (iAABB.Intersects(GetChildAABB(nAABB, 0))) {
                        InsertToNode(firstChild + 0, GetChildAABB(nAABB, 0), depth + 1, item, iAABB);
                    }
                    if (iAABB.Intersects(GetChildAABB(nAABB, 1))) {
                        InsertToNode(firstChild + 1, GetChildAABB(nAABB, 1), depth + 1, item, iAABB);
                    }
                    if (iAABB.Intersects(GetChildAABB(nAABB, 2))) {
                        InsertToNode(firstChild + 2, GetChildAABB(nAABB, 2), depth + 1, item, iAABB);
                    }
                    if (iAABB.Intersects(GetChildAABB(nAABB, 3))) {
                        InsertToNode(firstChild + 3, GetChildAABB(nAABB, 3), depth + 1, item, iAABB);
                    }
                }
            }

            void SubDivideNode(int n) {
                int newIdx = GetFreeNode();
                nodes[n] = new qtNode(nodes[n]) {
                    firstChild = newIdx
                };
                nodes[newIdx] = qtNode.New();
                nodes[newIdx + 1] = qtNode.New();
                nodes[newIdx + 2] = qtNode.New();
                nodes[newIdx + 3] = qtNode.New();
            }
        }


        public void Remove(T element) {
            if (!elementDict.ContainsKey(element)) {
                throw new System.InvalidOperationException();
            }

            RemoveFromNode(0, bounds);
            elementDict.Remove(element);

            void RemoveFromNode(int n, AABB nAABB) {
                nodes[n] = new qtNode(nodes[n]) {
                    count = nodes[n].count - 1,
                };


                int e = nodes[n].firstElement;
                int previous = -1;

                while (e >= 0) {
                    if (elements[e].value == element) {
                        if (previous == -1) {
                            nodes[n] = new qtNode(nodes[n]) {
                                firstElement = elements[e].nextElement
                            };
                        }
                        else {
                            elements[previous] = new qtElement(elements[previous]) {
                                nextElement = elements[e].nextElement
                            };
                        }
                        ReturnElement(e);
                        break;
                    }

                    previous = e;
                    e = elements[e].nextElement;
                }


                if (!nodes[n].isLeaf) {
                    var aabb = elementDict[element];
                    int firstChild = nodes[n].firstChild;
                    if (aabb.Intersects(GetChildAABB(nAABB, 0))) {
                        RemoveFromNode(firstChild + 0, GetChildAABB(nAABB, 0));
                    }
                    if (aabb.Intersects(GetChildAABB(nAABB, 1))) {
                        RemoveFromNode(firstChild + 1, GetChildAABB(nAABB, 1));
                    }
                    if (aabb.Intersects(GetChildAABB(nAABB, 2))) {
                        RemoveFromNode(firstChild + 2, GetChildAABB(nAABB, 2));
                    }
                    if (aabb.Intersects(GetChildAABB(nAABB, 3))) {
                        RemoveFromNode(firstChild + 3, GetChildAABB(nAABB, 3));
                    }
                }
            }
        }

        public void Prune() {
            PruneNode(0);
            void PruneNode(int n) {
                if (nodes[n].isLeaf) {
                    return;
                }

                if (nodes[n].count <= maxElements) {
                    //remove children
                    ReturnChildren(n);
                    nodes[n] = new qtNode(nodes[n]) {
                        firstChild = -1
                    };
                }
                else {
                    //prune children
                    var fc = nodes[n].firstChild;
                    PruneNode(fc + 0);
                    PruneNode(fc + 1);
                    PruneNode(fc + 2);
                    PruneNode(fc + 3);
                }

            }

            void ReturnChildren(int n) {
                if (nodes[n].isLeaf) {
                    return;
                }
                else {
                    var fc = nodes[n].firstChild;
                    ReturnChildren(fc + 0);
                    ReturnChildren(fc + 1);
                    ReturnChildren(fc + 2);
                    ReturnChildren(fc + 3);
                    ReturnNode(fc);
                }
            }
        }

        void ReturnNode(int n) {
            nodes[n] = new qtNode {
                firstChild = -1,
                firstElement = -1,
                count = 0,
                nextFree = freeNode
            };

            freeNode = n;
        }
        void ReturnElement(int e) {
            elements[e] = new qtElement() {
                nextElement = freeElement,
                bounds = default,
                value = default
            };

            freeElement = e;
        }


        int GetFreeElement() {
            if (freeElement < 0) {
                int i = elements.Count;
                elements.Add(new qtElement { nextElement = -1, value = default });
                return i;
            }

            int result = freeElement;
            freeElement = elements[freeElement].nextElement;
            return result;
        }

        int GetFreeNode() {
            if (freeNode < 0) {
                int i = nodes.Count;
                nodes.Add(qtNode.New());
                nodes.Add(qtNode.New());
                nodes.Add(qtNode.New());
                nodes.Add(qtNode.New());
                return i;
            }

            int result = freeNode;
            freeNode = nodes[freeNode].nextFree;
            return result;

        }




        public void OnDrawGizmos() {
            DrawNode(0, bounds);
            void DrawNode(int n, AABB bounds) {
                if (nodes[n].isLeaf) {
                    return;
                }

                DrawNode(nodes[n].firstChild + 0, GetChildAABB(bounds, 0));
                DrawNode(nodes[n].firstChild + 1, GetChildAABB(bounds, 1));
                DrawNode(nodes[n].firstChild + 2, GetChildAABB(bounds, 2));
                DrawNode(nodes[n].firstChild + 3, GetChildAABB(bounds, 3));
                Gizmos.color = Color.blue;

                Gizmos.DrawLine(
                    new Vector2((bounds.min.x + bounds.max.x) / 2, bounds.min.y),
                    new Vector2((bounds.min.x + bounds.max.x) / 2, bounds.max.y));

                Gizmos.DrawLine(
                    new Vector2(bounds.min.x, (bounds.min.y + bounds.max.y) / 2),
                    new Vector2(bounds.max.x, (bounds.min.y + bounds.max.y) / 2));

            }
        }


        static AABB GetChildAABB(AABB parent, int childInd) {
            float midY = (parent.min.y + parent.max.y) / 2;
            float midX = (parent.min.x + parent.max.x) / 2;

            Vector2 center = new Vector2(midX, midY);

            switch (childInd) {
                case 0:
                    return new AABB(center, parent.max);
                case 1:
                    return new AABB(new Vector2(parent.min.x, midY), new Vector2(midX, parent.max.y));
                case 2:
                    return new AABB(parent.min, center);
                case 3:
                    return new AABB(new Vector2(midX, parent.min.y), new Vector2(parent.max.x, midY));
                default:
                    throw new System.Exception();
            }
        }
    }

    public struct AABB {
        public Vector2 min;
        public Vector2 max;

        public AABB(Vector2 min, Vector2 max) {
            this.min = min;
            this.max = max;
        }

        public  bool Intersects(AABB other) {
            bool result =
                min.x <= other.max.x &&
                min.y <= other.max.y &&
                max.x >= other.min.x &&
                max.y >= other.min.y;
            return result;
        }
    }
}