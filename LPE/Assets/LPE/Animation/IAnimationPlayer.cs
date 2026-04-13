using System;
using UnityEngine;
using UnityEngine.UI;

namespace LPE.Animation {

    public interface IAnimationPlayer {
        void Update();
        bool IsDone();
    }
}
