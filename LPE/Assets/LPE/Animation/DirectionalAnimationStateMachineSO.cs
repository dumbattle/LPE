using System;
using System.Collections.Generic;
using UnityEngine;

namespace LPE.Animation {

    /// <summary>
    /// TState should implement ISerializable
    /// If TState is a struct, it should implement IEquatable<T> to avoid structs
    /// </summary>
    public abstract class DirectionalAnimationStateMachineSO<TState> : ScriptableObject, IDirectionalAnimationStateMachine<TState> {
        [Serializable]
        struct Entry {
            public TState state;
            public DirectionalAnimationSO anim;
        }

        [SerializeField] Entry[] _entries = new Entry[0];

        public IAnimationDefinition<SpriteRenderer> GetAnim(TState state, Vector2 direction) {
            foreach (var entry in _entries) {
                if (EqualityComparer<TState>.Default.Equals(entry.state, state)) {
                    return entry.anim.GetAnim(direction);
                }
            }
            throw new InvalidOperationException($"AnimationStateMachineSO: no animation defined for state {state}");
        }
        public bool TryGetAnim(TState state, Vector2 direction, out IAnimationDefinition<SpriteRenderer> result) {

            foreach (var entry in _entries) {
                if (EqualityComparer<TState>.Default.Equals(entry.state, state)) {
                    result = entry.anim.GetAnim(direction);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
