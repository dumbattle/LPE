using UnityEngine;


namespace LPE.Shape {
    public class CircleShape : Shape2D {
        private float _radius;

        public float radius { get => _radius; set { _radius = value; Update(); } }

        public CircleShape(float radius) {
            _radius = radius;
        }
        public override Vector2 Project(Vector2 line) {
            float center = Projection(position, line);
            float scaled_r = radius * scale;

            return new Vector2(center - scaled_r, center + scaled_r);
        }
        public override (Vector2 min, Vector2 max) AABB() {
            float scaled_r = radius * scale;

            return (
                new Vector2(position.x - scaled_r, position.y - scaled_r),
                new Vector2(position.x + scaled_r, position.y + scaled_r)
                );
        }
        public override void OnDrawGizmos() {
            Gizmos.DrawWireSphere(position, radius * scale);
        }

        public override void Update() { }
    }


}