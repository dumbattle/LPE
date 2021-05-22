using System.Collections.Generic;
using System;
using System.Collections;

namespace LPE {
    public class ObjectPool<T> where T : class {
        Dictionary<T, Item> returnDict = new Dictionary<T, Item>();
        Func<T> _constructor;
        LinkedList<Item> freeItems = new LinkedList<Item>();

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
            if (freeItems.First != null) {
                var n = freeItems.First;

                freeItems.RemoveFirst();

                return n.Value.obj;
            }

            var newItem = CreateItem();

            return Get();
        }

        public void Return(T t) {
            var n = returnDict[t].node;
            freeItems.AddLast(n);
        }

        Item CreateItem() {
            T t = _constructor();
            Item i = new Item(t);

            returnDict.Add(t, i);
            freeItems.AddLast(i.node);
            return i;
        }


        class Item {
            public T obj;
            public LinkedListNode<Item> node;

            public Item(T t) {
                obj = t;
                node = new LinkedListNode<Item>(this);
            }
        }
    }
}