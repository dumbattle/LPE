using System.Runtime.CompilerServices;
using UnityEngine;
namespace LPE.Math {
    public static partial class Geometry {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector2 min, Vector2 max) CircleAABB(Vector2 pos, float r) {
            return (
                pos - new Vector2(r, r),
                pos + new Vector2(r, r)
                );
        }

        public static (Vector2 min, Vector2 max) ExpandAABB((Vector2 min, Vector2 max) src, Vector2 dir) {
            (Vector2 min, Vector2 max) = src;
            if (dir.x > 0) {
                max.x += dir.x;
            }
            else {
                min.x += dir.x;
            }

            if (dir.y > 0) {
                max.y += dir.y;
            }
            else {
                min.y += dir.y;
            }
            return (min, max);
        }


    }
}