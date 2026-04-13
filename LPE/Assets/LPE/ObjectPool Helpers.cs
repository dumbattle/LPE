using System;
using System.Collections.Generic;

namespace LPE {

    public class ObjectPool {
        //*********************************************************************************************
        // Singleton Helper
        //*********************************************************************************************
        static class Singleton<T> where T : class, new() {
            public static ObjectPool<T> pool = new ObjectPool<T>(() => new T());
        }

        static class CollectionClear<T> {
            public static Action<ICollection<T>> clear = (t) => t.Clear();
        }
        static class StackClear<T> {
            public static Action<Stack<T>> clear = (t) => t.Clear();
        }

        public static PooledItemScope<List<T>> GetScopedList<T>() {
            return new PooledItemScope<List<T>>(Singleton<List<T>>.pool, CollectionClear<T>.clear, CollectionClear<T>.clear);
        }

        public static PooledItemScope<HashSet<T>> GetScopedHashSet<T>() {
            return new PooledItemScope<HashSet<T>>(Singleton<HashSet<T>>.pool, CollectionClear<T>.clear, CollectionClear<T>.clear);
        }

        public static PooledItemScope<Stack<T>> GetScopedStack<T>() {
            return new PooledItemScope<Stack<T>>(Singleton<Stack<T>>.pool, StackClear<T>.clear, StackClear<T>.clear);
        }

        public static PooledItemScope<Dictionary<T, U>> GetScopedDictionary<T, U>() {
            return new PooledItemScope<Dictionary<T, U>>(Singleton<Dictionary<T, U>>.pool, CollectionClear<KeyValuePair<T, U>>.clear, CollectionClear<KeyValuePair<T, U>>.clear);
        }


        public static T Get<T>() where T : class, new() {
            return Singleton<T>.pool.Get();
        }

        public static void Return<T>(T item) where T : class, new() {
            Singleton<T>.pool.Return(item);
        }



        public static List<T> GetList<T>() {
            var result = Singleton<List<T>>.pool.Get();
            result.Clear();
            return result;
        }

        public static void ReturnList<T>(List<T> item) {
            item.Clear();
            Singleton<List<T>>.pool.Return(item);
        }

        

        public static Dictionary<TKey, TVal> GetDictionary<TKey, TVal>() {
            var result = Singleton<Dictionary<TKey, TVal>>.pool.Get();
            result.Clear();
            return result;
        }

        public static void ReturnDictionary<TKey, TVal>(Dictionary<TKey, TVal> item) {
            item.Clear();
            Singleton<Dictionary<TKey, TVal>>.pool.Return(item);
        }



        public static HashSet<T> GetHashSet<T>() {
            var result = Singleton<HashSet<T>>.pool.Get();
            result.Clear();
            return result;
        }

        public static void ReturnHashSet<T>(HashSet<T> item) {
            item.Clear();
            Singleton<HashSet<T>>.pool.Return(item);
        }



        public static Stack<T> GetStack<T>() {
            var result = Singleton<Stack<T>>.pool.Get();
            result.Clear();
            return result;
        }

        public static void ReturnStack<T>(Stack<T> item) {
            item.Clear();
            Singleton<Stack<T>>.pool.Return(item);
        }


        public static Queue<T> GetQueue<T>() {
            var result = Singleton<Queue<T>>.pool.Get();
            result.Clear();
            return result;
        }

        public static void ReturnQueue<T>(Queue<T> item) {
            item.Clear();
            Singleton<Queue<T>>.pool.Return(item);
        }

        //*********************************************************************************************
        // Reminder Helper
        //*********************************************************************************************

        static HashSet<string> reminders = new HashSet<string>();
        public static void SetReminder(string id) {
            if (reminders.Contains(id)) {
                return;
            }
            reminders.Add(id);

            UnityEngine.Debug.LogWarning($"Rember to implement object pool for '{id}'");

        }
    }

    public struct PooledItemScope<T> : IDisposable where T : class {
        public T item => this._item;

        T _item;
        ObjectPool<T> _pool;
        Action<T> _onReturn;

        public PooledItemScope(ObjectPool<T> pool, Action<T> onGet=null, Action<T> onReturn=null) {
            _pool = pool;
            _item = pool.Get();
            onGet?.Invoke(_item);
            _onReturn = onReturn;
        }

        public void Dispose() {
            if (_pool == null) {
                return;
            }

            _onReturn?.Invoke(_item);
            _pool.Return(_item);
            _item = null;
            _pool = null;
            _onReturn = null;
        }
    }

}