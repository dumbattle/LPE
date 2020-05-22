using System.Collections.Generic;
using System;

namespace LPE {
    public class PriorityQueue<T>  {
        struct Item {
            public T value;
            public float priority;
        }

        List<Item> _items = new List<Item>();

        public bool isEmpty => _items.Count == 0;
        public int size => _items.Count;

        public void Add(T value, float priority) {
            _items.Add(new Item { value = value, priority = priority });
            SiftUp(_items.Count - 1);
        }
        public T Get() {
            if (_items.Count == 0) {
                throw new System.InvalidOperationException("Trying to get item from empty PriorityQueue");
            }

            T result = _items[0].value;
            _items[0] = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);

            SiftDown(0);

            return result;
        }
        public T Peek() {
            return _items[0].value;
        }
        public void Clear() {
            _items.Clear();
        }
        public bool Contains(T value) {
            foreach (var item in _items) {
                if (EqualityComparer<T>.Default.Equals(value, item.value)) {
                    return true;
                }
            }
            return false;
        } 


        void SiftUp(int n) {
            if (n == 0) {
                return;
            }
            var p = GetParent(n);

            if (_items[n].priority > _items[p].priority) {
                SwapNodes(n, p);
                SiftUp(p);
            }
        }

        void SiftDown(int n) {
            if (n >= _items.Count) {
                return;
            }

            int l = GetLeftChild(n);
            int r = l + 1;

            int c = n;
            if ( l < _items.Count) {
                c = l;
                if (r < _items.Count && _items[r].priority > _items[l].priority) {
                    c = r;
                }
            }

            if (_items[n].priority < _items[c].priority) {
                SwapNodes(n, c);
                SiftDown(c);
            }
        }


        private void SwapNodes(int n, int p) {
            var temp = _items[p];

            _items[p] = _items[n];
            _items[n] = temp;
        }

        static int GetParent(int n) {
            return (n - 1) / 2;
        }
        static int GetLeftChild(int n) {
            return 2 * n + 1;
        }
    }

}