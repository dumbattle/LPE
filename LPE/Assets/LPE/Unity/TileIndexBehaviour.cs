using System;
using UnityEngine;

namespace LPE.Unity {
    [ExecuteAlways]
    public class TileIndexBehaviour : MonoBehaviour {
        [SerializeField] SpecialTile tile;
        [SerializeField] bool disableOnPlay = true;
        [SerializeField] bool disableObjectOnPlay = false;
        [SerializeField] bool local = false;

        private void Awake() {
            if (Application.isPlaying && disableObjectOnPlay) {
                gameObject.SetActive(false);
            }
            if (Application.isPlaying && disableOnPlay) {
                enabled = false;
            }
        }

        private void Update() {
            if (Application.isPlaying) {
                return;
            }
            if (tile == null) {
                tile = new SpecialTile();
            }

            Vector3 world =  transform.position;
            Vector3Int global = new Vector3Int(
                Mathf.FloorToInt(world.x),
                Mathf.FloorToInt(world.y),
                Mathf.FloorToInt(world.z));

            if (local) {
                Vector3 localPos = transform.localPosition;
                Vector3Int local = new Vector3Int(
                    Mathf.FloorToInt(localPos.x + 0.5f),
                    Mathf.FloorToInt(localPos.y + 0.5f),
                    Mathf.FloorToInt(localPos.z));
                transform.localPosition = (Vector3)local;
            }
            else {
                transform.position = (Vector3)global + new Vector3(0.5f, 0.5f);
            }
            tile.x = global.x;
            tile.y = global.y;
        }

        /// <summary>
        /// Should not change during gameplay
        /// </summary>
        public Vector2Int Location() {
            return tile.AsVector2Int();
        }
        public void SetLocation(Vector2Int l) {
            if (tile == null) {
                tile = new SpecialTile();
            }

            tile.x = l.x;
            tile.y = l.y;

            // set position
            transform.position = new Vector3(tile.x + .5f, tile.y + .5f, transform.position.z);
        }
        public static implicit operator Vector2Int (TileIndexBehaviour t) {
            return t.tile.AsVector2Int();
        }
        [Serializable]
        class SpecialTile {
            public int x;
            public int y;

            public Vector2Int AsVector2Int() {
                return new Vector2Int(x, y);
            }
        }
    }
}
