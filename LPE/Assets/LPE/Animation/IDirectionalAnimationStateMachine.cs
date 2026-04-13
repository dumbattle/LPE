using UnityEngine;

namespace LPE.Animation {
    public interface IDirectionalAnimationStateMachine<TState> {
        IAnimationDefinition<SpriteRenderer> GetAnim(TState state, Vector2 direction);
        bool TryGetAnim(TState state, Vector2 direction, out IAnimationDefinition<SpriteRenderer> result);
    }
}
