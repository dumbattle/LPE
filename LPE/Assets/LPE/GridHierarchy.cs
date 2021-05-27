using System;
using System.Collections.Generic;
using UnityEngine;

namespace LPE.SpacePartition {
    /// <summary>
    /// Not performant
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridHierarchy<T> {

        bool[][,] _coarse;

        int[,] _fine;

        FreeList<(int next, T value)> _elements = new FreeList<(int next, T value)>();

        Vector2 _min;
        Vector2 _max;

        public GridHierarchy(Vector2 min, Vector2 max, int levels) {
            _min = min;
            _max = max;

            int cres = 2;

            _coarse = new bool[levels][,];

            for (int i = 0; i < levels; i++) {
                _coarse[i] = new bool[cres, cres];
                cres *= 2;
            }


            _fine = new int[cres, cres];


            for (int x = 0; x < cres; x++) {
                for (int y = 0; y < cres; y++) {
                    _fine[x, y] = -1;
                }
            }
        }


        public void Add(T item, Vector2 min, Vector2 max) {
            Query(min, max, 0, 0, 0, true, null, (x, y) => AddItemToGrid(x, y));
            Query(min, max, 0, 1, 0, true, null, (x, y) => AddItemToGrid(x, y));
            Query(min, max, 1, 0, 0, true, null, (x, y) => AddItemToGrid(x, y));
            Query(min, max, 1, 1, 0, true, null, (x, y) => AddItemToGrid(x, y));


            void AddItemToGrid(int x, int y) {
                var i = _fine[x, y];
                var r = _elements.Add((i, item));

                _fine[x, y] = r;

                (x, y) = GetParent(x, y);
                SetGridTrue(x, y, _coarse.GetLength(0) - 1);
            }
            
            void SetGridTrue(int x, int y, int level) {
                _coarse[level][x, y] = true;
                if (level > 0) {
                    (x, y) = GetParent(x, y);
                    SetGridTrue(x, y, level - 1);
                }
            }
        }
        public void Remove(T item, Vector2 min, Vector2 max) {
            Query(min, max, 0, 0, 0, false, null, (x, y) => RemoveFromGrid(x, y));
            Query(min, max, 0, 1, 0, false, null, (x, y) => RemoveFromGrid(x, y));
            Query(min, max, 1, 0, 0, false, null, (x, y) => RemoveFromGrid(x, y));
            Query(min, max, 1, 1, 0, false, null, (x, y) => RemoveFromGrid(x, y));

            void RemoveFromGrid(int x, int y) {
                int p = -1;
                var i = _fine[x, y];

                while (i != -1) {
                    T t = _elements[i].value;
                    if (EqualityComparer<T>.Default.Equals(t, item)) {
                        break;
                    }

                    p = i;
                    i = _elements[i].next;
                }
                if (i == -1) {
                    return;

                }
                if (p >= 0) {
                    _elements[p] = (_elements[i].next, _elements[p].value);
                }
                else {
                    _fine[x, y] = _elements[i].next;
                }

                _elements.RemoveAt(i);
                (x, y) = GetParent(x, y);
                SetGridFalse(x, y, _coarse.GetLength(0) - 1);
            }

            void SetGridFalse(int x, int y, int level) {

                _coarse[level][x, y] = false;
                if (level > 0) {
                    var (cx, cy) = GetFirstChild(x, y);
                    bool empty = true;

                    if (level == _coarse.GetLength(0) - 1) {
                        empty =
                            _fine[cx, cy] == -1 &&
                            _fine[cx, cy + 1] == -1 &&
                            _fine[cx + 1, cy] == -1 &&
                            _fine[cx + 1, cy + 1] == -1;
                    }
                    else {
                        empty =
                            _coarse[level + 1][cx, cy] &&
                            _coarse[level + 1][cx, cy + 1] &&
                            _coarse[level + 1][cx + 1, cy] &&
                            _coarse[level + 1][cx + 1, cy + 1];
                    }

                    _coarse[level][x, y] = !empty;

                    if (!empty && level > 0) {
                        (x, y) = GetParent(x, y);
                        SetGridFalse(x, y, level - 1);

                    }
                }
            }
        }

        public void Query(Vector2 min, Vector2 max, Action<T> q) {
            Query(min, max, 0, 0, 0, false, q, null);
            Query(min, max, 1, 0, 0, false, q, null);
            Query(min, max, 0, 1, 0, false, q, null);
            Query(min, max, 1, 1, 0, false, q, null);

        }
        void Query(Vector2 min, Vector2 max, int x, int y, int level, bool queryEmpty, Action<T> qt, Action<int,int> qc) {
            var (a, b) = AABB(x, y, level);
            if  (!Math.Geometry.AABBIntersection(a, b, min, max)) {
                return;
            }

            if (level < _coarse.Length) {
                if (!queryEmpty && !_coarse[level][x, y]) {
                    return;
                }
                var (x1, y1) = GetFirstChild(x, y);

                Query(min, max, x1, y1, level + 1, queryEmpty, qt, qc);
                Query(min, max, x1 + 1, y1, level + 1, queryEmpty, qt, qc);
                Query(min, max, x1, y1 + 1, level + 1, queryEmpty, qt, qc);
                Query(min, max, x1 + 1, y1 + 1, level + 1, queryEmpty, qt, qc);
            }
            else {
                qc?.Invoke(x, y);
                int i = _fine[x, y];
                while (i != -1) {
                    qt?.Invoke(_elements[i].value);
                    i = _elements[i].next;
                }
            }
        }

        (Vector2 min, Vector2 max) AABB(int x, int y, int level) {
            var res = 2 << level;
            var width = (_max.x - _min.x) / res;
            var height = (_max.y - _min.y) / res;

            return (
                _min + new Vector2(x * width, y * height),
                _min + new Vector2((x + 1) * width, (y + 1) * height)
            );
        }
    
        (int, int) GetFirstChild(int x, int y) {
            return (x * 2, y * 2);
        }
        (int, int) GetParent(int x, int y) {
            return (x / 2, y / 2);
        }
    
    }    
}
