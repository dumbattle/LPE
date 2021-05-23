using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE.Math;


namespace LPE.Steering {
    public static class Steering {
        public static Vector2 Basic(Vector2 target, SteerAgent agent, List<SteerAgent> nearby) {
            var pos = agent.pos;
            var prev = pos;

            var dir = (target - pos);
               

            // lerp for smooth-ish rotation
            if (agent.dir != Vector2.zero) {
                dir = Vector2.Lerp(dir.normalized, agent.dir.normalized, .6f);
            }

            dir =
                dir.normalized +
                Seperation(agent, nearby, 1.5f) * 4;
      
            return dir.normalized;
        }

  

        static Vector2 Seperation(SteerAgent agent, List<SteerAgent> nearby, float sepScale) {
            Vector2 result = new Vector2();
            foreach (var other in nearby) {
                if (other == agent) {
                    continue;
                }
                var dir = other.pos - agent.pos;
                var sep = agent.size * sepScale;
                float minRad = sep + other.size;

                //too far
                if (dir.sqrMagnitude > minRad * minRad) {
                    continue;
                }

                var dist = dir.magnitude;
                var scale = (dist - other.size) / minRad;

                if (Mathf.Approximately(dist, 0)) {
                    // on same spot -> rand direction
                    dir = Random.insideUnitCircle;

                }
                dir = dir.normalized;
                // correction
                var cv = dir * Mathf.Lerp(1, 0, scale);
                result -= cv;


            }
            return result;
        }
    }



    public class SteerAgent {
        public Vector2 pos;
        public Vector2 dir;
        public float size;
    }


}
