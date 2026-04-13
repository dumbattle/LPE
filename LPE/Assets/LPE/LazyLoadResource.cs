using UnityEngine;
namespace LPE {
    public class LazyLoadResource<T> where T : Object {
        public T obj => this;

        T _obj;
        string path;

        public LazyLoadResource(string path) {
            this.path = path;
        }


        public static implicit operator T(LazyLoadResource<T> src) {
            if (src._obj == null) {
                src._obj = Resources.Load<T>(src.path);
            }
            if (src._obj == null) {
                throw new System.ArgumentException($"Cannot find resource of type '{typeof(T)}' at path '{src.path}'");
            }
            return src._obj;
        }
    }

}