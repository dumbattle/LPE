using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;


namespace LPE.Steering {


    public static class Steering {
        /// <summary>
        /// Useful for idle agents that are clumped and need to seperate
        /// </summary>
        public static Vector2 Seperate<T>(T agent, List<T> nearby) where T : ISteerAgent {

            var dir =
                Seperation(agent, nearby, 2f) +
                Seperation(agent, nearby, .8f) * 3;

            return dir.normalized;
        }

        public static Vector2 Better<T>(Vector2 target, T agent, List<T> nearby, float momentum = 0.6f) where T : ISteerAgent {
            var pos = agent.position;

            var delta = target - pos;
            var dir = delta.normalized;
            var distScale = delta.sqrMagnitude / (agent.radius * agent.radius);
            distScale = Mathf.Clamp01(distScale);


            // lerp for smooth-ish rotation
            if (agent.direction != Vector2.zero) {
                var m = momentum * distScale;
                dir = Vector2.Lerp(dir, agent.direction.normalized, m);
            }


            dir =
                dir.normalized * 2 +
                Seperation(agent, nearby, 1.2f) * 3 * distScale +
                Seperation(agent, nearby, 1) * 5;

            return dir.normalized;
        }


        public static Vector2 Basic<T>(Vector2 target, T agent, List<T> nearby) where T : ISteerAgent {
            var pos = agent.position;

            var dir = (target - pos);
            if (dir == Vector2.zero) {
                return Seperate(agent, nearby);
            }
            // lerp for smooth-ish rotation
            if (agent.direction != Vector2.zero) {
                dir = Vector2.Lerp(
                    dir.normalized,
                    agent.direction.normalized + Random.insideUnitCircle * 0.01f, // add random just incase directions are completely opposite
                    .6f);
            }

            dir =
                dir.normalized * 3 +
                Seperation(agent, nearby, 2f) * 4+
                Seperation(agent, nearby, .8f) * 4;

            return dir.normalized;
        }


        /// <typeparam name="T"></typeparam>
        /// <param name="agent"></param>
        /// <param name="nearby"></param>
        /// <param name="sepScale"></param>
        /// <returns></returns>
        static Vector2 Seperation<T>(T agent, List<T> nearby, float sepScale) where T : ISteerAgent {
            Vector2 result = new Vector2();
            foreach (var other in nearby) {
                if (EqualityComparer<T>.Default.Equals(other, agent)) {
                    continue;
                }
                var dir = other.position - agent.position;

                var dist = dir.magnitude;
                var scale = dist / ((agent.radius + other.radius) * sepScale);

                //too far
                if (scale > 1) {
                    continue;
                }

                if (Mathf.Approximately(scale, 0)) {
                    // on same spot -> rand direction
                    dir = Random.insideUnitCircle;
                }
                dir = dir.normalized;
                scale = Mathf.Lerp(1, 0, scale * scale);
                // correction
                var cv = dir * scale;
                result -= cv;
            }
            return result;
        }
    }


}
