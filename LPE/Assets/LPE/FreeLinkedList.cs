using System.Collections;
using System.Collections.Generic;
namespace LPE {
    public class FreeLinkedList<T> : IEnumerable<T>{
        static FreeList<Node> _list = new FreeList<Node>();
        
        public int Count { get; private set; }
        int _first = -1;
        int _last = -1;


        public Node AddFirst(T item) {
            Count++;
            int f = _first;
            // add node
            int ind = _list.Add(new Node(item, f, -1));
            // update first
            _first = ind;
            // update second
            if (f >= 0) {
                var s = _list[f];
                _list[f] = new Node(s.value, s.next, ind);
            }
            // update last
            if (_last == -1) {
                _last = ind;
            }

            return _list[ind];
        }

        public Node AddLast(T item) {
            Count++;
            int l = _last;
            // add node
            int ind = _list.Add(new Node(item, -1, l));
            // update last
            _last = ind;
            // update second last
            if (l >= 0) {
                var s = _list[l];
                _list[l] = new Node(s.value, ind, s.prev);
            }
            // update first
            if (_first == -1) {
                _first = ind;
            }

            return _list[ind];
        }

        public IEnumerator<T> GetEnumerator() {
            int f = _first;
            
            while (f >= 0) {
                yield return _list[f].value;
                f = _list[f].next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        internal static Node Next(Node n) {
            if (!n.Valid || n.next == -1) {
                return Node.GetInvalid();
            }

            return _list[n.next];
        }
        internal static Node Previous(Node n) {
            if (!n.Valid || n.prev == -1) {
                return Node.GetInvalid();
            }

            return _list[n.prev];
        }

        public struct Node {
            public bool Valid { get; private set; }
            public readonly T value;

            public Node Next => Next(this);
            public Node Previous => Previous(this);

            internal readonly int next;
            internal readonly int prev;

            internal Node(T value, int next, int prev) {
                this.value = value;
                this.next = next;
                this.prev = prev;
                Valid = true;
            }

            internal static Node GetInvalid() {
                return new Node(default(T), -1, -1) { Valid = false }; 
            }
        }
    }
}
