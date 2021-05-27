using System.Collections.Generic;
using UnityEngine;
using LPE.Math;


namespace LPE.Steering {
    public static class Steering {
        public static Vector2 Basic<T>(Vector2 target, T agent, IEnumerable<T> nearby) where T : ISteerAgent {
            var pos = agent.pos;
            var prev = pos;

            var dir = (target - pos);

            // lerp for smooth-ish rotation
            if (agent.dir != Vector2.zero) {
                dir = Vector2.Lerp(dir.normalized, agent.dir.normalized, .6f);
            }

            dir =
                dir.normalized +
                Seperation(agent, nearby, 2f) * 4 +
                Seperation(agent, nearby, .8f) * 4;
      
            return dir.normalized;
        }



        static Vector2 Seperation<T>(T agent, IEnumerable<T> nearby, float sepScale) where T : ISteerAgent {
            if (nearby is List<T> l) {
                return Seperation(agent, l, sepScale);
            }
            Vector2 result = new Vector2();
            foreach (var other in nearby) {
                if (EqualityComparer<T>.Default.Equals(other, agent)) {
                    continue;
                }
                var dir = other.pos - agent.pos;
                var sep = Mathf.Max(agent.size, other.size) * sepScale;
                float minRad = sep + Mathf.Min(agent.size, other.size);



                var dist = dir.magnitude;
                var scale = (dist) / minRad;
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

        // List<T> doesn't create garbage
        static Vector2 Seperation<T>(T agent, List<T> nearby, float sepScale) where T : ISteerAgent {
            Vector2 result = new Vector2();
            foreach (var other in nearby) {
                if (EqualityComparer<T>.Default.Equals(other, agent)) {
                    continue;
                }
                var dir = other.pos - agent.pos;
                var sep = Mathf.Max(agent.size, other.size) * sepScale;
                float minRad = sep + Mathf.Min(agent.size, other.size);



                var dist = dir.magnitude;
                var scale = (dist) / minRad;
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



    public interface ISteerAgent {
        Vector2 pos { get; }
        Vector2 dir { get; }
        float size { get; }
    }


}
