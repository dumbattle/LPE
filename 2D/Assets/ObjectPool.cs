using System.Collections.Generic;
using System;
using System.Collections;

namespace LPE {
    public class ObjectPool<T> where T : class {
        Dictionary<T, Item> returnDict = new Dictionary<T, Item>();
        Func<T> _constructor;
        List<Item> items = new List<Item>();

        public ObjectPool(Func<T> constructor) {
            _constructor = constructor;
        }

        public ObjectPool(Func<T> objCreater, int initialCapacity) {
            _constructor = objCreater;
            for (int i = 0; i < initialCapacity; i++) {
                CreateItem();
            }
        }

        public T Get() {
            for (int i = 0; i < items.Count; i++) {
                Item item = items[i];
                if (!item.active) {
                    item.active = true;
                    return item.obj;
                }
            }

            var newItem = CreateItem();

            newItem.active = true;
            return newItem.obj;
        }

        public void Return(T t) {
            Item item = returnDict[t];

            item.active = false;
        }

        Item CreateItem() {
            T t = _constructor();
            Item i = new Item(t);
            returnDict.Add(t, i);
            items.Add(i);
            return i;
        }


        class Item {
            public T obj;
            public bool active = false;

            public Item(T t) {
                obj = t;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            foreach (var t in items) {
                yield return t.obj;
            }
        }
    }


}
