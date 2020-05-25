using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;


namespace LPE.Hex {
    public struct HexGrid : IEnumerable<HexCoord> {
        public int radius;
        public int area;
        public int circumference;
        public HexCoord center;


        public HexGrid(int radius, HexCoord center) : this(radius) {
            this.center = center;
        }
        public HexGrid(int radius) {
            this.radius = radius;
            circumference = 6 * radius;
            area = 1 + 3 * radius * (radius + 1);
            center = new HexCoord(0, 0);
        }



        public void ParallelForeach(Action<HexCoord> function) {
            Thread[] threads = new Thread[area];
            int ind = 0;

            foreach (var h in this) {
                var t = new Thread(() => function(h));
                threads[ind] = t;

                t.Start();
                ind++;
            }
            foreach (var t in threads) {
                t.Join();
            }
        }
        public IEnumerator<HexCoord> GetEnumerator() {
            yield return new HexCoord(0, 0) + center;

            for (int r = 1; r <= radius; r++) {
                var hex = new HexCoord(0, 0) + HexCoord.Directions[4] * r;

                for (int d = 0; d < 6; d++) {
                    for (int i = 0; i < r; i++) {
                        yield return hex + center;
                        hex += HexCoord.Directions[d];
                    }
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public HexCoord RandomCoord() {
            //choose ring
            //choose arm
            //choose hex
            int r = UnityEngine.Random.Range(0, (radius + 1) * radius * 3 + 1);
            if (r == 0) {
                return new HexCoord(0, 0) + center;
            }
            r--;

            for (int i = 1; i <= radius; i++) {
                r -= i * 6;
                if (r < 0) {
                    r = i;
                    break;
                }
            }


            int arm = UnityEngine.Random.Range(0, 6);

            HexCoord result = HexCoord.Directions[arm] * r;
            int step = UnityEngine.Random.Range(0, r);
            return result + HexCoord.Directions[(arm + 2) % 6] * step + center;
        }

        public bool IsInRange(HexCoord hex) {
            return (hex - center).ManhattanDist() <= radius;
        }
    }

    public struct HexRectGrid : IEnumerable<HexCoord> {

        public float width { get; private set; }
        public float height { get; private set; }

        public float spacing { get; private set; }
        public Orientation orientation { get; private set; }

        public HexRectGrid(float width, float height, float spacing, Orientation orientation) {
            if (width <= 0) {
                throw new ArgumentException("Width must be greater than 0");
            }
            if (height <= 0) {
                throw new ArgumentException("Seight must be greater than 0");
            }
            if (spacing <= 0) {
                throw new ArgumentException("Spacing must be greater than 0");
            }
            this.width = width;
            this.height = height;
            this.spacing = spacing;
            this.orientation = orientation;
        }


        public Vector2 CenterOffset() {
            int w = (int)(width / (spacing / 2));
            int h = (int)(width / (spacing * HexCoord.ROOT3 / 2));
            return new Vector2(width - w * (spacing / 2), height - h * (spacing * HexCoord.ROOT3 / 2)) / 2;
        }

        public IEnumerator<HexCoord> GetEnumerator() {
            HexRectGrid copy = this;
            return Horizontal();

            IEnumerator<HexCoord> Horizontal () {
                int width = (int)(copy.width / (copy.spacing / 2)) + 1;
                int height = (int)(copy.width / (copy.spacing * HexCoord.ROOT3 / 2)) + 1;

                if (width > 100_000 || height > 100_000) {
                    throw new InvalidOperationException("Spacing is too small");
                }
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < (width + (y + 1) % 2) / 2; x++) {
                        yield return new HexCoord(x - y / 2, y);

                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}