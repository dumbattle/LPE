using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Profiling;
namespace LPE.SpacePartition {
    public abstract class Partition2D<T> {
        public abstract void Add(T item, Vector2 min, Vector2 max);

        public abstract void Remove(T item);

        public abstract void UpdateItem(T item, Vector2 min, Vector2 max);

        public abstract void QueryItems(Vector2 min, Vector2 max, HashSet<T> result);
        public abstract void QueryItems(Vector2 min, Vector2 max, List<T> result);

        public abstract void CleanUp();

        //*****************************************************************************
        // Overloads
        //*****************************************************************************
        public void QueryItems((Vector2 min, Vector2 max) aabb, HashSet<T> result) {
            QueryItems(aabb.min, aabb.max, result);
        }
        public void QueryItems((Vector2 min, Vector2 max) aabb, List<T> result) {
            QueryItems(aabb.min, aabb.max, result);
        }

        public void Add(T item, (Vector2 min, Vector2 max) aabb) {
            Add(item, aabb.min, aabb.max);
        }
        public void UpdateItem(T item, (Vector2 min, Vector2 max) aabb) {
            UpdateItem(item, aabb.min, aabb.max);
        }
    }

    public class Grid2D<T> : Partition2D<T> {
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

        public Grid2D(Vector2 min, Vector2 max, Vector2Int resolution) {
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


        //***********************************************************************************************
        // Helper methods
        //***********************************************************************************************


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


}
