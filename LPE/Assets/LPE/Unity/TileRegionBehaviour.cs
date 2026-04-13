using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LPE.Unity {
    public class TileRegionBehaviour : MonoBehaviour {
        public Color gizmoColor = Color.white;
        public TileIndexBehaviour rootIndex;
        public Vector2Int offset => rootIndex?.Location() ?? Vector2Int.zero;

        [SerializeField, FormerlySerializedAs("positions")] 
        List<Vector2Int> _positions = new List<Vector2Int>();



        public List<Vector2Int> GetPositions() {
            var result = new List<Vector2Int>();
            foreach (var p in _positions) {
                result.Add(p + offset);
            }
            return result;
        }

        public void GetPositions(HashSet<Vector2Int> results) {
            foreach (var p in _positions) {
                results.Add(p + offset);
            }
        }

        public bool ContainsPosition(Vector2Int pos) {
            return _positions.Contains(pos - offset);
        }
        public void AddWorldPosition(Vector2Int w) {
            _positions.Add(w-offset);
        }
        public void RemoveWorldPosition(Vector2Int w) {
            _positions.Remove(w-offset);
        }
        void OnDrawGizmosSelected() {
            Color c = gizmoColor;
            if (Event.current.alt) {
                c.a /= 2;
            }
            Gizmos.color = c;
            foreach (var p in _positions) {
                var t = p + offset;
                Gizmos.DrawCube(t + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }
}
