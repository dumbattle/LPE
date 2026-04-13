using UnityEngine;

namespace LPE {
    public static class Direction4Extensions {
        public static Vector2Int ToVector2Int(this Direction4.Enum d) {
            return Direction4.FromEnum(d).ToVector2Int();
        }
        public static Direction4 Reverse(this Direction4 d) {
            return Direction4.Reverse(d);
        }
    }
}