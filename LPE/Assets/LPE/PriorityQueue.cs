using System.Collections;
using System.Collections.Generic;

namespace LPE {
    /// <summary>
    /// Max Priority Queue
    /// </summary>
    public class PriorityQueue<T> {
        struct Item {
            public T item;
            public float priority;
        }

        List<Item> _items = new List<Item>();

        Item best;
        bool bestExists;


        //*************************************************************************************************************************
        // Public
        //*************************************************************************************************************************

        public bool isEmpty => !bestExists && (_items.Count == 0);
        public int size => _items.Count + (bestExists ? 1 : 0);

        public void Add(T value, float priority) {

            if (bestExists) {
                // push existing best into queue
                // next step will place incoming into best
                if (priority > best.priority) {
                    _items.Add(best);
                    SiftUp(_items.Count - 1);
                    bestExists = false;
                }
            }

            // set incoming as best
            if ((_items.Count == 0 || priority >= _items[0].priority) && !bestExists) {
                best = new Item { item = value, priority = priority };
                bestExists = true;
            }
            // push incoming into queue
            else {
                _items.Add(new Item { item = value, priority = priority });
                SiftUp(_items.Count - 1);
            }
        }

        public T Get() {
            if (bestExists) {
                var r = best;
                best = new Item();
                bestExists = false;
                return r.item;
            }
            if (_items.Count == 0) {
                throw new System.InvalidOperationException("Trying to get item from empty PriorityQueue");
            }

            T result = _items[0].item;
            _items[0] = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);

            SiftDown(0);

            return result;
        }
        
        public T Peek() {
            if (bestExists) {
                return best.item;
            }
            return _items[0].item;
        }
       
        public float PeekPriority() {
            if (bestExists) {
                return best.priority;
            }
            return _items[0].priority;
        }

        public void Clear() {
            best = new Item();
            bestExists = false;
            _items.Clear();
        }
       
        //*************************************************************************************************************************
        // Helpers
        //*************************************************************************************************************************


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
            if (l < _items.Count) {
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


        void SwapNodes(int n, int p) {
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