using System.Collections.Generic;

namespace LPE {
    public static partial class LPEExtensions {
        public static bool IsInRange<T>(this T[] arr, int i) {
            return i >= 0 && i < arr.Length;
        }
        public static bool IsInRange<T>(this List<T> arr, int i) {
            return i >= 0 && i < arr.Count;
        }

        public static float Max(this float f, float maxValue) {
            return f > maxValue ? maxValue : f;
        }
        public static int Max(this int f, int maxValue) {
            return f > maxValue ? maxValue : f;
        }
        public static int Min(this int f, int maxValue) {
            return f < maxValue ? maxValue : f;
        }
    }

}