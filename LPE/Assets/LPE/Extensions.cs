using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LPE {
    public static partial class LPEExtensions {
        public static bool IsWithinBounds(this Vector2Int p, Vector2Int min, Vector2Int max, bool maxinclusive=true) {
            return
                maxinclusive
                ? p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y
                : p.x >= min.x && p.x < max.x && p.y >= min.y && p.y <=max.y;
        }
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

        public static Vector3 SetZ(this Vector2 v, float z) {
            return new Vector3(v.x, v.y, z);
        }

        public static string ContentString<T>(this IEnumerable<T> list, System.Func<T, string> formatter = null) {
            var sb = ObjectPool.Get<StringBuilder>();
            sb.Append('[');

            bool first = true;
            foreach (var item in list) {
                if (!first)
                    sb.Append(", ");
                first = false;

                if (formatter != null) {
                    sb.Append(formatter(item));
                }
                else if (item is string s) {
                    sb.Append('\'').Append(s).Append('\'');
                }
                else if (item == null) {
                    sb.Append("None");
                }
                else {
                    sb.Append(item);
                }
            }

            sb.Append(']');

            var result = sb.ToString();
            sb.Clear();
            ObjectPool.Return(sb);

            return result;
        }
        public static bool IntersectsXY(this BoundsInt a, BoundsInt b, bool allowTangent = false) {
            if (allowTangent) {
                return
                    (a.xMin <= b.xMax) && (a.xMax >= b.xMin) &&
                    (a.yMin <= b.yMax) && (a.yMax >= b.yMin);
            }
            else {
                return
                    (a.xMin < b.xMax) && (a.xMax > b.xMin) &&
                    (a.yMin < b.yMax) && (a.yMax > b.yMin);
            }
        }
        public static Bounds ToBounds(this BoundsInt boundsInt) {
            Vector3 center = boundsInt.min + (Vector3)boundsInt.size / 2f;
            Vector3 size = boundsInt.size;
            return new Bounds(center, size);
        }
        public static bool Contains(this BoundsInt bounds, Vector2 point) {
            // Convert Vector2 to Vector3Int (z defaults to bounds.min.z)
            Vector3Int point3D = new Vector3Int(
                Mathf.FloorToInt(point.x),
                Mathf.FloorToInt(point.y),
                bounds.min.z
            );

            return bounds.Contains(point3D);
        }

        public static bool Contains(this BoundsInt bounds, Vector2Int point) {
            Vector3Int point3D = new Vector3Int(point.x, point.y, bounds.min.z);
            return bounds.Contains(point3D);
        }
    }
}