using UnityEngine;
using System;

namespace LPE.Animation {
    [Serializable]
    public class AnimationFrameDefinition {
        public Sprite sprite;
        public int duration;
        public AnimationFrameTags tags;
    }
}
