using System.Collections.Generic;
using UnityEngine;

namespace LPE {
    public class ArbitraryDataStorage<TKey> {
        Dictionary<TKey, Entry> data = new Dictionary<TKey, Entry>();


        public void Set<T>(TKey key, T value) {
            if (!data.ContainsKey(key)) {
                data.Add(key, Entry<T>.pool.Get());
            }
            var e = data[key];

            // found
            if (e is Entry<T> entry) {
                entry.value = value;
            }
            else {
                data[key].Return();
                var e2 = Entry<T>.pool.Get();
                e2.value = value;
                data[key] = e2;
            }
        }

        public T GetOrDefault<T>(TKey key, T defaultVal) {
            if (TryGet<T>(key, out var result)) {
                return result;
            }
            return defaultVal;
        }

        public bool TryGet<T>(TKey key, out T value) {
            // does not exist
            if (!data.ContainsKey(key)) {
                value = default(T);
                return false;
            }

            var e = data[key];

            // found
            if (e is Entry<T> entry) {
                value = entry.value;
                return true;
            }

            // wrong type
            Debug.LogWarning($"ArbitraryDataStorage: Tried to get key '{key}' with wrong type. \nExpected '{e.GetType().GetGenericArguments()[0]}', \nGot '{typeof(T)}'");
            value = default(T);
            return false;
        }

        public void Remove(TKey key) {
            if (data.ContainsKey(key)) {
                data[key].Return();
                data.Remove(key);
            }
        }

        public void Clear() {
            foreach (var kv in data) {
                kv.Value.Return();
            }

            data.Clear();
        }

        class Entry {
            public virtual void Return() { }
        }

        class Entry<T> : Entry {
            public static ObjectPool<Entry<T>> pool = new ObjectPool<Entry<T>>(() => new Entry<T>());
            public T value;

            public override void Return() {
                pool.Return(this);
                value = default(T);
            }
        }
    }
}