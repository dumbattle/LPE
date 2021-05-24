using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LPE {
    public class FreeList<T>  {
        public T this[int i] => _data[i].value;

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