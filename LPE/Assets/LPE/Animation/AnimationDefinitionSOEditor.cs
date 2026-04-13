#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LPE.Animation {
    [CustomEditor(typeof(LpeAnimationDefinitionSO), true)]
    public class AnimationDefinitionSOEditor : Editor {
        public override bool HasPreviewGUI() => true;
        public override bool RequiresConstantRepaint() => true;


        public override void OnPreviewSettings() {
            base.OnPreviewSettings();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            LpeAnimationDefinitionSO src = target as LpeAnimationDefinitionSO;
            //GUI.Label(r, "test");

            if (src.numFrames == 0) {
                return;
            }
            var t = GetFrame(src.TotalDuration());
            int f = -1;

            for (int i = 0; i < src.numFrames; i++) {
                t -= src.GetFrameDuration(i);
                if (t < 0) {
                    f = i;
                    break;
                }
            }
            if (f == -1) {
                return;
            }

            var s = src.frames[f].sprite;
            if (s == null) {
                return;
            }
            var sRect = s.rect;
            var tex = s.texture;
            sRect = new Rect(sRect.x / tex.width, sRect.y / tex.height, sRect.width / tex.width, sRect.height / tex.height);
            GUI.DrawTextureWithTexCoords(r, s.texture, sRect);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            LpeAnimationDefinitionSO src = target as LpeAnimationDefinitionSO;
            int totalFrames = src.TotalDuration();
            EditorGUILayout.LabelField($"Total Duration: {totalFrames} Frames");
        }

        static int GetFrame(int numFrames) {
            double maxTime = (float)numFrames / 30;
            var tSec = EditorApplication.timeSinceStartup % maxTime;
            var result = (int)(tSec * 30);
            return result;
        }
    }
}
#endif
