using UnityEngine;

namespace LPE.Animation {
    [CreateAssetMenu(fileName = "new LPE Animation Definition", menuName = "LPE/Animation/Animation Def")]
    public class LpeAnimationDefinitionSO : ScriptableObject, IAnimationDefinition<SpriteRenderer> {
        public int numFrames => frames.Length;

        public AnimationFrameDefinition[] frames;


        public int GetFrameDuration(int frame) {
            return frames[frame].duration;
        }

        public AnimationFrameTags GetFrameTags(int frame) {
            return frames[frame].tags;
        }

        public void DrawFrame(int frame, SpriteRenderer sr) {
            sr.sprite = frames[frame].sprite;
        }
        public int TotalDuration() {
            int result = 0;
            if (frames == null) {
                return result;
            }
            foreach (var f in frames) {
                result += f.duration;
            }
            return result;
        }
    }
}
