using UnityEngine;

namespace LPE {
    public static class RandomUtility {
        public static T Uniform<T>(T t1, T t2) {
            if (Random.value < 0.5f) {
                return t1;
            }
            return t2;
        }
        public static T Uniform<T>(T t1, T t2, T t3) {
            if (Random.value < 0.3333f) {
                return t1;
            }
            return Uniform(t2, t3);
        }
        public static T Uniform<T>(T t1, T t2, T t3, T t4) {
            if (Random.value < .25f) {
                return t1;
            }
            return Uniform(t2, t3, t4);
        }
    }
}