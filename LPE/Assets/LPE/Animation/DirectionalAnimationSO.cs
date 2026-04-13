using UnityEngine;

namespace LPE.Animation {
    public abstract class DirectionalAnimationSO : ScriptableObject {
        public abstract IAnimationDefinition<SpriteRenderer> GetAnim(Vector2 direction);
    }
}
