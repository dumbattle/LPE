using UnityEngine;
using UnityEngine.Tilemaps;

namespace LPE.Unity {
    [CreateAssetMenu(fileName = "Color Drift Tile", menuName = "LPE/Tiles/Color Drift Tile")]
    public class ColorDriftTile : TileBase {
        public Sprite sprite;
        public Color color1;
        public Color color2;
        public Color color3;
        [Min(0.001f)]
        public float bias1 = 1;
        [Min(0.001f)]
        public float bias2 = 1;
        [Min(0.001f)]
        public float bias3 = 1;

        public int cycleTime = 10;
        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
            var f = Time.frameCount + Mathf.Abs(position.x) * 1312 + Mathf.Abs(position.y) * 1254;
            tileData.sprite = sprite;
            var cycle = Mathf.Abs(0.123412f * position.x + 0.32451f * position.y + 0.86753f * position.z + 0.1324f);
            cycle = (f / cycleTime) + (cycle % 1) * 1000;

            var startColor = GetTarget(position, (int)cycle);
            var targetColor = GetTarget(position, (int)cycle + 1);
            tileData.flags = TileFlags.LockColor;
            tileData.color = Color.Lerp(startColor, targetColor, (f % cycleTime) / (float)cycleTime);
        }

        Color GetTarget(Vector3Int position, int cycle) { 
            var PA = 4.123343f;
            var PB = 3.1423123f;
            var PC = 4.152134f;
            float a = 2.1234f * (cycle * PC  + position.x * PA + position.y * PB);
            float b = 1.131f * (cycle * PB + position.x * PC + position.y * PA);
            float c = 2.13f * (cycle * PA + position.x * PB + position.y * PC);
            
            a %= 1;
            b %= 1;
            c %= 1;
            a *= bias1;
            b *= bias2;
            c *= bias3;
            a /= a + b + c;
            b /= a + b + c;
            c /= a + b + c;
            return color1 * a + color2 * b + color3 * c;
        }

    }
}
