using System;
using System.Collections.Generic;
using UnityEngine;

namespace LPE.SpacePartition {
    public class Grid2D<T> : Partition2D<T> {
        /// <summary>
        /// index(x,y) = x * row + y
        /// </summary>
        List<T>[] _grid;
        Vector2 min;
        Vector2 max;
        int colCount;
        int rowCount;
        float cellWidth_inv;
        float cellHeight_inv;

        Dictionary<T, (int minX, int minY, int maxX, int maxY)> aabb = new();



        public Grid2D(Vector2 min, Vector2 max, Vector2Int resolution) {
            this.min = min;
            this.max = max;
            colCount = resolution.x;
            rowCount = resolution.y;
            _grid = new List<T>[colCount * rowCount];

            for (int i = 0; i < _grid.Length; i++) {
                _grid[i] = new List<T>(4);
            }
            cellWidth_inv = colCount / (max.x - min.x);
            cellHeight_inv = rowCount / (max.y - min.y);
        }

        public override void Add(T item, Vector2 min, Vector2 max) {
            if (aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid already contains item");
            }


            var (ax, ay) = ToIndex(min);
            var (bx, by) = ToIndex(max);
            aabb.Add(item, (ax, ay, bx, by));

            TraverseCells(ax, bx, ay, by, out var t);
            foreach (var i in t) {
                _grid[i].Add(item);
            }
        }

        public override void Remove(T item) {
            if (!aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            var (ax, ay, bx, by) = aabb[item];
            aabb.Remove(item);
            TraverseCells(ax, bx, ay, by, out var t);
            foreach (var c in t) {
                // TODO - Swap and Pop for faster removal if needed
                _grid[c].Remove(item);
            }
        }

        public override void UpdateItem(T item, Vector2 min, Vector2 max) {
            if (!aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            var (oax, oay, obx, oby) = aabb[item];

            var (nax, nay) = ToIndexMin(min);
            var (nbx, nby) = ToIndexMax(max);

            // no change to cells
            if (oax == nax &&
                oay == nay &&
                obx == nbx &&
                oby == nby)
                return;


            aabb[item] = (nax, nay, nbx, nby);


            // remove
            TraverseCells(oax, obx, oay, oby, out var t);
            foreach (var s in t) {
                // TODO - Swap and Pop for faster removal if needed
                _grid[s].Remove(item);
            }

            // add
            TraverseCells(nax, nbx, nay, nby, out t);
            foreach (var s in t) {
                _grid[s].Add(item);
            }
        }

        public override void QueryItems(Vector2 min, Vector2 max, HashSet<T> result) {
            var (ax, ay) = ToIndexMin(min);
            var (bx, by) = ToIndexMax(max);

            TraverseCells(ax, bx, ay, by, out var t);
            foreach (var s in t) {
                foreach (var item in _grid[s]) {
                    result.Add(item);
                }
            }
        }


        public override void QueryItems(Vector2 min, Vector2 max, List<T> result) {
            var (ax, ay) = ToIndexMin(min);
            var (bx, by) = ToIndexMax(max);

            TraverseCells(ax, bx, ay, by, out var t);
            foreach (var s in t) {
                foreach (var item in _grid[s]) {
                    if (!result.Contains(item)) {
                        result.Add(item);
                    }
                }
            }
        }


        public bool Contains(T item) {
            return aabb.ContainsKey(item);
        }

        //***********************************************************************************************
        // Helper methods
        //***********************************************************************************************


        (int x, int y) ToIndex(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Clamp(Mathf.FloorToInt(x), 0, colCount - 1),
                Mathf.Clamp(Mathf.FloorToInt(y), 0, rowCount - 1)
            );
        }
        (int x, int y) ToIndexMin(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Max(Mathf.FloorToInt(x), 0),
                Mathf.Max(Mathf.FloorToInt(y), 0)
            );
        }
        (int x, int y) ToIndexMax(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Min(Mathf.FloorToInt(x), colCount - 1),
                Mathf.Min(Mathf.FloorToInt(y), rowCount - 1)
            );
        }


        //***********************************************************************************************
        // Traversal
        //***********************************************************************************************

        void TraverseCells(int startX, int endX, int startY, int endY, out CellTraversal result) {
            result = new CellTraversal(rowCount, startX, endX, startY, endY);
        }


        struct CellTraversal {
            // Only store the actual values we need 
            public int rangeY;
            public int rangeX;
            public int delta;
            public int start;

            public CellTraversal(int rh, int startX, int endX, int startY, int endY) {
                rangeY = endY - startY + 1;
                rangeX = endX - startX + 1;

                delta = rh - rangeY;
                start = startX * rh + startY;

            }

            public CellEnumerator GetEnumerator() {
                return new CellEnumerator(this);
            }
        }

        struct CellEnumerator {
            public int Current { get; private set; }

            CellTraversal t;
            int dy;
            int dx;


            public CellEnumerator(CellTraversal t) {
                this.t = t;
                Current = t.start - 1; // Start 1 step before
                dy = -1;
                dx = 0;
            }

            public bool MoveNext() {
                dy++;
                Current += 1; // move forward

                if (dy >= t.rangeY) {
                    // jump to next row
                    dy = 0;
                    dx++;
                    Current += t.delta;
                }

                // all rows done
                return dx < t.rangeX;
            }
        }

    }

    public class Grid2DOld<T> : Partition2D<T> {
        FreeList<(T item, int next)> _data = new FreeList<(T item, int next)>();
        /// <summary>
        /// index(x,y) = x * rh + y
        /// </summary>
        int[] _grid;
        Vector2 min;
        Vector2 max;
        int rw;
        int rh;
        float cellWidth_inv;
        float cellHeight_inv;

        Dictionary<T, (Vector2 min, Vector2 max)> aabb = new Dictionary<T, (Vector2 min, Vector2 max)>();

        public Grid2DOld(Vector2 min, Vector2 max, Vector2Int resolution) {
            this.min = min;
            this.max = max;
            rw = resolution.x;
            rh = resolution.y;
            _grid = new int[rw * rh];

            for (int i = 0; i < _grid.Length; i++) {
                _grid[i] = -1;
            }
            cellWidth_inv = rw / (max.x - min.x);
            cellHeight_inv = rh / (max.y - min.y);
        }

        public override void Add(T item, Vector2 min, Vector2 max) {
            if (aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid already contains item");
            }

            aabb.Add(item, (min, max));

            var (ax, ay) = ToIndex(min);
            var (bx, by) = ToIndex(max);

            int i = ax * rh + ay;
            int delta = rh - by + ay - 1;

            for (int x = ax; x <= bx; x++) {
                for (int y = ay; y <= by; y++) {
                    int f = _grid[i];
                    int j = _data.Add((item, f));
                    _grid[i] = j;
                    i++;
                }

                i += delta;
            }
        }

        public override void Remove(T item) {
            if (!aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            var (min, max) = aabb[item];
            aabb.Remove(item);


            var (ax, ay) = ToIndex(min);
            var (bx, by) = ToIndex(max);

            int s = ax * rh + ay;
            int delta = rh - by + ay - 1;
            for (int x = ax; x <= bx; x++) {
                for (int y = ay; y <= by; y++) {
                    int p = -1;
                    int i = _grid[s];

                    while (!EqualityComparer<T>.Default.Equals(_data[i].item, item)) {
                        p = i;
                        i = _data[i].next;
                    }

                    int n = _data[i].next;
                    if (p >= 0) {
                        _data[p] = (_data[p].item, n);
                    }
                    else {
                        _grid[s] = n;
                    }

                    _data[i] = (default(T), -1);
                    _data.RemoveAt(i);
                    s++;
                }

                s += delta;
            }
        }

        public override void UpdateItem(T item, Vector2 min, Vector2 max) {
            if (!aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            var bb = aabb[item];
            aabb[item] = (min, max);
            var (oax, oay) = ToIndexMin(bb.min);
            var (obx, oby) = ToIndexMax(bb.max);

            var (nax, nay) = ToIndexMin(min);
            var (nbx, nby) = ToIndexMax(max);


            // remove  
            int s = oax * rh + oay;
            int delta = rh - oby + oay - 1;
            for (int x = oax; x <= obx; x++) {
                for (int y = oay; y <= oby; y++) {
                    // overlap new area  
                    if (x >= nax && x <= nbx && y >= nay && y <= nby) {
                        s++;
                        continue;
                    }
                    int p = -1;
                    int i = _grid[s];
                    while (!EqualityComparer<T>.Default.Equals(_data[i].item, item)) {
                        p = i;
                        i = _data[i].next;
                    }
                    int n = _data[i].next;
                    if (p >= 0) {
                        _data[p] = (_data[p].item, n);
                    }
                    else {
                        _grid[s] = n;
                    }

                    _data[i] = (default(T), -1);
                    _data.RemoveAt(i);
                    s++;
                }

                s += delta;
            }

            // add  
            s = nax * rh + nay;
            delta = rh - nby + nay - 1;
            for (int x = nax; x <= nbx; x++) {
                for (int y = nay; y <= nby; y++) {

                    // overlap old area  
                    if (x >= oax && x <= obx && y >= oay && y <= oby) {
                        s++;
                        continue;
                    }
                    int f = _grid[s];
                    int i = _data.Add((item, f));
                    _grid[s] = i;
                    s++;
                }

                s += delta;
            }
        }

        public override void QueryItems(Vector2 min, Vector2 max, HashSet<T> result) {
            var (ax, ay) = ToIndexMin(min);
            var (bx, by) = ToIndexMax(max);

            int s = ax * rh + ay;
            int delta = rh - by + ay - 1;

            for (int x = ax; x <= bx; x++) {
                for (int y = ay; y <= by; y++) {
                    int i = _grid[s];

                    while (i >= 0) {
                        T item = _data[i].item;
                        result.Add(item);
                        i = _data[i].next;
                    }
                    s++;
                }
                s += delta;
            }
        }


        public override void QueryItems(Vector2 min, Vector2 max, List<T> result) {
            var (ax, ay) = ToIndexMin(min);
            var (bx, by) = ToIndexMax(max);

            int s = ax * rh + ay;
            int delta = rh - by + ay - 1;

            for (int x = ax; x <= bx; x++) {
                for (int y = ay; y <= by; y++) {
                    int i = _grid[s];

                    while (i >= 0) {
                        T item = _data[i].item;
                        if (!result.Contains(item)) {
                            result.Add(item);
                        }
                        i = _data[i].next;
                    }
                    s++;
                }
                s += delta;
            }
        }

        public override void CleanUp() { }

        public bool Contains(T item) {
            return aabb.ContainsKey(item);
        }

        //***********************************************************************************************  
        // Helper methods  
        //***********************************************************************************************  
        public void DrawGizmos(Vector2 offset) {
            foreach (var kv in aabb) {
                Gizmos.DrawCube((kv.Value.min + kv.Value.max) / 2 + offset, kv.Value.max - kv.Value.min);
            }
        }

        (int x, int y) ToIndex(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Clamp(Mathf.FloorToInt(x), 0, rw - 1),
                Mathf.Clamp(Mathf.FloorToInt(y), 0, rh - 1)
            );
        }
        (int x, int y) ToIndexMin(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Max(Mathf.FloorToInt(x), 0),
                Mathf.Max(Mathf.FloorToInt(y), 0)
            );
        }
        (int x, int y) ToIndexMax(Vector2 v) {
            v = v - min;

            var x = v.x * cellWidth_inv;
            var y = v.y * cellHeight_inv;

            return (
                Mathf.Min(Mathf.FloorToInt(x), rw - 1),
                Mathf.Min(Mathf.FloorToInt(y), rh - 1)
            );
        }

    }

    public class FreeList<T> {
        public T this[int i] {
            get {
                return _data[i].value;
            }
            set {
                _data[i] = (-1, value);
            }
        }

        List<(int next, T value)> _data = new List<(int next, T value)>();
        int _firstFree = -1;

        public int Range => _data.Count;

        public int Add(T item) {
            if (_firstFree >= 0) {
                int index = _firstFree;
                _firstFree = _data[_firstFree].next;
                _data[index] = (-1, item);
                return index;
            }
            else {

                _data.Add((-1, item));
                return _data.Count - 1;
            }
        }

        public void RemoveAt(int i) {
            _data[i] = (_firstFree, default(T));
            _firstFree = i;
        }

        public void Clear() {
            _data.Clear();
            _firstFree = -1;
        }

    }
}