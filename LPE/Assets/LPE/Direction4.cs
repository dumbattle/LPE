using UnityEngine;

namespace LPE {
    public class Direction4 {
        public static readonly Direction4 up = new Direction4(new Vector2Int(0, 1));
        public static readonly Direction4 down = new Direction4(new Vector2Int(0, -1));
        public static readonly Direction4 right = new Direction4(new Vector2Int(1, 0));
        public static readonly Direction4 left = new Direction4(new Vector2Int(-1, 0));

        public Vector2Int ToVector2Int() => v2;

        Vector2Int v2;

        Direction4(Vector2Int v2) {
            this.v2 = v2;
        }
        public override string ToString() {
            return ((Enum)this).ToString();
        }

        public static Direction4 Reverse(Direction4 d) {
            if (d == up) return down;
            if (d == down) return up;
            if (d == left) return right;
            if (d == right) return left;
            return null;
        }
        public static Direction4 FromVector2Int(Vector2Int v) {
            if (v == up.v2) return up;
            if (v == down.v2) return down;
            if (v == left.v2) return left;
            if (v == right.v2) return right;
            return null;
        }
        public static Direction4 FromEnum(Enum e) {
            switch (e) {
                case Enum.up:
                    return up;
                case Enum.right:
                    return right;
                case Enum.down:
                    return down;
                case Enum.left:
                    return left;
            }
            return null;
        }

        public static implicit operator Direction4(Enum e) {
            return FromEnum(e);
        }
        public static implicit operator Enum(Direction4 e) {
            if (e == Direction4.up) {
                return Enum.up;
            }
            if (e == Direction4.right) {
                return Enum.right;
            }
            if (e == Direction4.left) {
                return Enum.left;
            }
            if (e == Direction4.down) {
                return Enum.down;
            }
            return Enum.none;
        }
        public enum Enum {
            up, right, down, left, none
        }
    }
}