using System;
using System.Collections.Generic;

namespace LPE {

    public class ObjectPool<T> where T : class {
        public int createdCount => availableCount + inUseCount;
        public int availableCount => availableItems.Count;
        public int inUseCount => usedItems.Count;


   
        HashSet<T> usedItems = new HashSet<T>();
        Stack<T> availableItems = new Stack<T>();

        Func<T> _constructor;
        int warningCount;


        public ObjectPool(Func<T> objCreater, int warningCount = 1000) {
            _constructor = objCreater ?? throw new ArgumentNullException(nameof(objCreater));
            this.warningCount = warningCount;
        }

        public T Get() {
            if (availableCount == 0) {
                CreateItem();
            }

            var item = availableItems.Pop();
            usedItems.Add(item);
            return item;
        }

        /// <summary>
        /// Will throw if you double return an item or return an item that was not created by this pool
        /// </summary>
        public void Return(T t) {
            if (!usedItems.Contains(t)) {

                if (availableItems.Contains(t)) {
                    throw new InvalidOperationException($"ObjectPool.Return: Item has already been returned");
                }
                else {
                    throw new InvalidOperationException($"ObjectPool.Return: Item does not belong to this pool");
                }
            }

            usedItems.Remove(t);
            availableItems.Push(t);
        }

        public PooledItemScope<T> GetScoped() {
            return new PooledItemScope<T>(this);
        }
        void CreateItem() {
            T t = _constructor();
            availableItems.Push(t);

            if (createdCount % warningCount == 0) {
                UnityEngine.Debug.LogWarning($"ObjectPool<{typeof(T).Name}> capacity reached {createdCount}");
            }
        }
    }

}