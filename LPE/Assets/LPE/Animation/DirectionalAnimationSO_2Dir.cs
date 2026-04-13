using UnityEngine;

namespace LPE.Animation {
    [CreateAssetMenu(menuName = "LPE/Animation/Directional 2Dir")]
    public class DirectionalAnimationSO_2Dir : DirectionalAnimationSO {
        public LpeAnimationDefinitionSO right;
        public LpeAnimationDefinitionSO left;

        public override IAnimationDefinition<SpriteRenderer> GetAnim(Vector2 direction) {
            return direction.x != 0f ?
                   direction.x >= 0f ? right : left :
                   direction.y >= 0f ? right : left;
        }
    }
}
