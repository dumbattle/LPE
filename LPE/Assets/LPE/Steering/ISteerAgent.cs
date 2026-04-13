using UnityEngine;


namespace LPE.Steering {
    public interface ISteerAgent {
        Vector2 position { get; }
        Vector2 direction { get; }
        float radius { get; }
    }


}
