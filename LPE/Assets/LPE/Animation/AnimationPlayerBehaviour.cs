using UnityEngine;

namespace LPE.Animation {
    public class AnimationPlayerBehaviour : MonoBehaviour {
        public SpriteRenderer sr;
        public LpeAnimationDefinitionSO anim;

        public bool autoStart = true;

        public AnimationPlayer<SpriteRenderer> animPlayer;

        void Awake() {
            animPlayer = new AnimationPlayer<SpriteRenderer>();
            if (autoStart) {
                StartAnimation(anim, true);
                animPlayer.Update();
            }
        }

        private void Update() {
            animPlayer.Update();
        }

        public void SetAnimation(IAnimationDefinition<SpriteRenderer> anim) {
            StartAnimation(anim, true);
            animPlayer.Update();
        }

        public void StartAnimation(IAnimationDefinition<SpriteRenderer> anim, bool loop) {
            animPlayer.Set(sr, anim, loop);
        }

        public void StartAnimation(bool loop) {
            animPlayer.Set(sr, anim, loop);
        }
    }
}
