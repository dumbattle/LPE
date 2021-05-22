using UnityEngine;

namespace LPE.Hex {
    public struct HexCoord {
        public const float ROOT3 = 1.7320508f;
        public static readonly HexCoord[] Directions =  {
            new HexCoord(0, 1), new HexCoord(1, 0), new HexCoord(1, -1),
            new HexCoord(0, -1) ,new HexCoord(-1, 0) , new HexCoord(-1, 1)
        };

        public int x { get; private set; }
        public int y { get; private set; }
        public int z { get; private set; }

        public int Arm {
            get {
                if (x == 0 && y == 0) {
                    return 0;
                }
                int a, b, c;
                a = x > 0 ? 1 : x < 0 ? -1 : 0;
                b = y > 0 ? 2 : y < 0 ? -1 : 0;
                c = z > 0 ? 3 : z < 0 ? -1 : 0;

                if (a == 0) { a = b > 0 ? 1 : -1; }
                if (b == 0) { b = c > 0 ? 2 : -1; }
                if (c == 0) { c = a > 0 ? 3 : -1; }

                switch (a + b + c) {
                    case 4:
                        return 0;
                    case 0:
                        return 1;
                    case 2:
                        return 2;
                    case -1:
                        return 3;
                    case 3:
                        return 4;
                    case 1:
                        return 5;
                }
                throw new System.InvalidOperationException("Can't get arm");
            }
        }

        public HexCoord(int x, int y) {
            this.x = x;
            this.y = y;
            z = -x - y;
        }

        public int ManhattanDist(HexCoord hex) {
            return (Mathf.Abs(x - hex.x) + Mathf.Abs(y - hex.y) + Mathf.Abs(z - hex.z)) / 2;
        }
        public int ManhattanDist() {
            return (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z)) / 2;
        }
        public float SqrDist(HexCoord hex) {
            int dx = x - hex.x;
            int dy = y - hex.y;
            return dx * dx + dy * dy + dx * dy;
        }
        public float Dist(HexCoord hex) {
            return Mathf.Sqrt(SqrDist(hex));
        }

        public HexCoord[] GetNeighbors() {
            return new[] {
            this + Directions[0],
            this + Directions[1],
            this + Directions[2],
            this + Directions[3],
            this + Directions[4],
            this + Directions[5]
        };
        }



        public Vector2 ToCartesian(Orientation orientation = Orientation.horizontal, float radius = 1) {
            float x = this.x * radius;
            float y = this.y * radius;
            switch (orientation) {
                case Orientation.vertical:
                    return Vertical();
                case Orientation.horizontal:
                    return Horizontal();
            }
            return Vector2.zero;

            Vector2 Vertical() {
                return new Vector2(ROOT3 / 2, .5f) * x + new Vector2(0, 1) * y;
            }
            Vector2 Horizontal() {
                return new Vector2(1, 0) * x + new Vector2(.5f, ROOT3 / 2) * y;
            }
        }

        public static HexCoord FromCartesian(Vector2 pos, float hexRadius = 1, Orientation orientation = Orientation.horizontal) {
            pos /= hexRadius;
            float x = 0, y = 0;
            switch (orientation) {
                case Orientation.vertical:
                    x = 2f / ROOT3 * pos.x;
                    y = pos.y - pos.x / ROOT3;
                    break;
                case Orientation.horizontal:
                    y = 2 * pos.y / ROOT3;
                    x = pos.x - y / 2;
                    break;
            }
            return Round(x, y);
        }
        public static HexCoord Round(float x, float y) {
            float z = -x - y;
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            if (Mathf.Abs(rx - x) > Mathf.Abs(ry - y) && Mathf.Abs(rx - x) > Mathf.Abs(rz - z)) {
                rx = -ry - rz;
            }
            else if (Mathf.Abs(ry - y) > Mathf.Abs(rz - z)) {
                ry = -rz - rx;
            }

            return new HexCoord(rx, ry);
        }

        public static HexCoord operator +(HexCoord l, HexCoord r) {
            return new HexCoord(l.x + r.x, l.y + r.y);
        }
        public static HexCoord operator -(HexCoord l, HexCoord r) {
            return new HexCoord(l.x - r.x, l.y - r.y);
        }
        public static HexCoord operator *(HexCoord h, int i) {
            return new HexCoord(h.x * i, h.y * i);
        }
        public static bool operator ==(HexCoord l, HexCoord r) {
            return l.x == r.x && l.y == r.y;
        }
        public static bool operator !=(HexCoord l, HexCoord r) {
            return l.x != r.x || l.y != r.y;
        }

        public static implicit operator HexCoord((int x, int y) t) {
            return new HexCoord(t.x, t.y);
        }

        public override bool Equals(object other) {
            if (!(other is HexCoord h)) return false;

            return x == h.x && y == h.y;
        }
        public override int GetHashCode() {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        public override string ToString() {
            return $"Hex({x}, {y})";
        }
    }
}