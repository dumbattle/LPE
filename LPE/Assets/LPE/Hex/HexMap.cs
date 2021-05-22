using System.Collections;
using System.Collections.Generic;


namespace LPE.Hex {
    public class HexMap<T> : IEnumerable<T> {
        public HexGrid grid { get; private set; }
        T[] value;

        public T this[HexCoord h] {
            get {
                int i = MapIndex(h);
                return value[i];
            }
            set {
                int i = MapIndex(h);
                this.value[i] = value;
            }
        }

        public int Size => grid.area;
        public int Radius { get; private set; }

        public HexMap(int radius) {
            Radius = radius;
            grid = new HexGrid(radius);
            value = new T[grid.area];
        }

        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)value).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<T>)value).GetEnumerator();
        }

        public bool IsInRange(HexCoord hex) {
            return hex.ManhattanDist() <= Radius;
        }


        static int MapIndex(HexCoord h) {
            int r = h.ManhattanDist();
            int innerArea = r == 0 ? 0 : new HexGrid(r - 1).area;
            int arm = h.Arm * r;

            switch (h.Arm) {
                case 0:
                    arm += h.y;
                    break;
                case 1:
                    arm -= h.z;
                    break;
                case 2:
                    arm += h.x;
                    break;
                case 3:
                    arm -= h.y;
                    break;
                case 4:
                    arm += h.z;
                    break;
                case 5:
                    arm -= h.x;
                    break;
            }
            return innerArea + arm;
        }
    }
}