using UnityEngine;
namespace LPE.Math {
    public static partial class Geometry {

        public static bool IsIntersecting(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2) {
                return false;
            }
            const float EPS = 1e-10f;
            bool result = false;


            float denom = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);

            if (denom > EPS || denom < -EPS) {
                float u_a = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denom;
                float u_b = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denom;
                float zero = EPS;
                float one = 1f - EPS;

                //Are intersecting if u_a and u_b are between 0 and 1
                if (u_a > zero && u_a < one && u_b > zero && u_b < one) {
                    result = true;
                }
            }

            return result;

        }


        /// <summary>
        /// Circle - Circle intersection
        /// touch => should touching be counted as intersecting
        /// </summary>
        public static bool IsIntersecting(Vector2 c1, float r1, Vector2 c2, float r2, bool touch = false) {
            if (touch) {
                return (c1 - c2).sqrMagnitude <= (r1 + r2) * (r1 + r2);
            }
            else {
                return (c1 - c2).sqrMagnitude < (r1 + r2) * (r1 + r2);
            }
        }


        public static bool AABBIntersection(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax) {
            return
                amin.x < bmax.x &&
                amax.x > bmin.x &&
                amin.y < bmax.y &&
                amax.y > bmin.y;
        }

        /// <summary>
        /// invRayDir = reciprical of ray direction
        /// </summary>
        public static bool RayAABBIntersection((Vector2 min, Vector2 max) aabb, Vector2 rayStart, Vector2 invRayDir) {
            Vector2 t0 = (aabb.min - rayStart) * invRayDir;
            Vector2 t1 = (aabb.max - rayStart) * invRayDir;
            Vector2 tmin = Vector2.Min(t0, t1);
            Vector2 tmax = Vector2.Max(t0, t1);
            return Mathf.Max(tmin.x, tmin.y) <= Mathf.Min(tmax.x, tmax.y);
        }

    }
}