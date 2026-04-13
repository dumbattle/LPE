#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LPE.Unity {
    [CustomEditor(typeof(TileRegionBehaviour))]
    public class TileRegionBehaviourEditor : Editor {
        const float TEXT_FIELD_HEIGHT = 20;
        const float LABEL_WIDTH = 250;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        void OnSceneGUI() {
            TileRegionBehaviour src = (TileRegionBehaviour)target;
            if (Selection.activeGameObject != src.gameObject) {
                return;
            }

            var svRect = SceneView.currentDrawingSceneView.position;
            var cam = SceneView.currentDrawingSceneView.camera;

            Vector3 pos = Event.current.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            pos.y = cam.pixelHeight - pos.y * ppp;
            pos.x *= ppp;
            pos = cam.ScreenToWorldPoint(pos);

            Vector2Int mouseT = Vector2Int.RoundToInt(pos - new Vector3(.5f, .5f));

            Handles.BeginGUI();
            GUI.Button(
                new Rect(
                new Vector2(svRect.xMax / 2 - LABEL_WIDTH / 2, 0),
                new Vector2(LABEL_WIDTH, TEXT_FIELD_HEIGHT)),
                "Right-click to add tiles");
            GUI.Button(
                new Rect(
                new Vector2(svRect.xMax / 2 - LABEL_WIDTH / 2, TEXT_FIELD_HEIGHT),
                new Vector2(LABEL_WIDTH, TEXT_FIELD_HEIGHT)),
                "Right-click + CTRL to remove tiles");
            Handles.EndGUI();


            if (Event.current.type == EventType.MouseDown) {
                if (Event.current.button == 1) {
                    visited.Clear();
                    visited.Add(mouseT);

                    if (Event.current.control) {
                        TryRemove(src, mouseT);
                    }
                    else {
                        TryAdd(src, mouseT);
                    }

                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.MouseDrag) {
                if (Event.current.button == 1) {

                    if (!visited.Contains(mouseT)) {
                        visited.Add(mouseT);
                        if (Event.current.control) {
                            TryRemove(src, mouseT);
                        }
                        else {
                            TryAdd(src, mouseT);
                        }
                    }

                    Event.current.Use();

                }
            }
        }

        void TryAdd(TileRegionBehaviour src, Vector2Int pos) {
            var offset = pos;
            Undo.RecordObject(target, "Edit TileGroupBehaviour");
            if (!src.ContainsPosition(offset)) {
                src.AddWorldPosition(offset);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
        void TryRemove(TileRegionBehaviour src, Vector2Int pos) {
            var offset = pos;
            Undo.RecordObject(target, "Edit TileGroupBehaviour");
            if (src.ContainsPosition(offset)) {
                src.RemoveWorldPosition(offset);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}
#endif