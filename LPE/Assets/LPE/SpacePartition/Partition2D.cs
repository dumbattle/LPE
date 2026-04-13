using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
namespace LPE.SpacePartition {
    public abstract class Partition2D<T> {
        //*****************************************************************************
        // Abstract
        //*****************************************************************************
        public abstract void Add(T item, Vector2 min, Vector2 max);

        public abstract void Remove(T item);

        public abstract void UpdateItem(T item, Vector2 min, Vector2 max);

        public abstract void QueryItems(Vector2 min, Vector2 max, HashSet<T> result);
        public virtual void QueryItems(Vector2 min, Vector2 max, List<T> result) { }

        public virtual void CleanUp() { }

        //*****************************************************************************
        // Overloads
        //*****************************************************************************
        public void QueryItems((Vector2 min, Vector2 max) aabb, HashSet<T> result) {
            QueryItems(aabb.min, aabb.max, result);
        }
        public void QueryItems((Vector2 min, Vector2 max) aabb, List<T> result) {
            QueryItems(aabb.min, aabb.max, result);
        }

        public void Add(T item, (Vector2 min, Vector2 max) aabb) {
            Add(item, aabb.min, aabb.max);
        }
        public void UpdateItem(T item, (Vector2 min, Vector2 max) aabb) {
            UpdateItem(item, aabb.min, aabb.max);
        }
    }
}
