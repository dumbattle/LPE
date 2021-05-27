using System;
using System.Collections.Generic;
using UnityEngine;


namespace LPE.SpacePartition {
    public interface IShape2D {
        Shape2D shape { get; }
        event Action OnShapeUpdate;
    }



    public abstract class Shape2D : IShape2D {
        static Vector2[] _dummy = new Vector2[0];
        public event Action OnShapeUpdate;
        public Shape2D shape => this;

        private Vector2 _position;
        public Vector2 position { get => _position; set { _position = value; } }

        public void UpdateShape() {
            OnShapeUpdate?.Invoke();
        }

        public abstract Vector2 Project(Vector2 line);
        public abstract (Vector2 min, Vector2 max) AABB();
        public static float Projection(Vector2 point, Vector2 line) {
            return (point.x * line.x + point.y * line.y) / Mathf.Sqrt(line.x * line.x + line.y * line.y);
        }
        public virtual Vector2[] Vertices() {
            return _dummy;
        }
        public virtual Vector2[] CollisionAxes() {
            return _dummy;
        }


        public virtual void OnDrawGizmos() { }

        public bool CheckCollision(IShape2D other) {
            return CheckCollision(this, other);
        }
        public Vector2 CheckCollisionWithCorrection(IShape2D other) {
            return CheckCollisionWithCorrection(this, other);
        }
        public static bool CheckCollision(IShape2D is1, IShape2D is2) {
            Shape2D s1 = is1.shape;
            Shape2D s2 = is2.shape;

            CircleShape c1 = s1 as CircleShape;
            CircleShape c2 = s2 as CircleShape;

            if (c1 != null && c2 != null) {
                return CircleCollision(c1, c2);
            }

            foreach (var axis in s1.CollisionAxes()) {
                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);

                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return false;
                }
            }
            foreach (var axis in s2.CollisionAxes()) {
                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);

                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return false;
                }
            }

            if (c1 != null) {
                var vert = s2.Vertices();
                Vector2 closest = new Vector2(0, 0);
                float closestDist = -1;

                foreach (var v in vert) {
                    float dist = (v - c1._position).sqrMagnitude;
                    if (dist < closestDist || closestDist < 0) {
                        closestDist = dist;
                        closest = v - c1._position;
                    }
                }
                var axis = closest;

                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);

                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return false;
                }
            }
            if (c2 != null) {
                var vert = s1.Vertices();
                Vector2 closest = new Vector2(0, 0);
                float closestDist = -1;

                foreach (var v in vert) {
                    float dist = (v - c2._position).sqrMagnitude;
                    if (dist < closestDist || closestDist < 0) {
                        closestDist = dist;
                        closest = v - c2._position;
                    }
                }

                var axis = closest;

                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);

                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return false;
                }
            }

            return true;
        }
        public static Vector2 CheckCollisionWithCorrection(IShape2D is1, IShape2D is2) {
            Shape2D s1 = is1.shape;
            Shape2D s2 = is2.shape;

            CircleShape c1 = s1 as CircleShape;
            CircleShape c2 = s2 as CircleShape;
            if (c1 != null && c2 != null) {
                return CircleCollisionWithCorrection(c1, c2);
            }

            Vector2 correctionVector = Vector2.zero;
            float minDist = float.PositiveInfinity;

            foreach (var axis in s1.CollisionAxes()) {
                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);

                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return Vector2.zero;
                }
                else {
                    float rightDist = shadow1.y - shadow2.x;
                    float leftDist = shadow2.y - shadow1.x;

                    if (rightDist > leftDist) {
                        //go left
                        if (leftDist < minDist) {
                            correctionVector = -axis;
                            minDist = leftDist;
                        }
                    }
                    else {
                        // go right
                        if (rightDist < minDist) {
                            correctionVector = axis;
                            minDist = rightDist;
                        }
                    }
                }
            }
            foreach (var axis in s2.CollisionAxes()) {
                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);


                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return Vector2.zero;
                }
                else {
                    float rightDist = shadow1.y - shadow2.x;
                    float leftDist = shadow2.y - shadow1.x;

                    if (rightDist > leftDist) {
                        //go left
                        if (leftDist < minDist) {
                            correctionVector = -axis;
                            minDist = leftDist;
                        }
                    }
                    else {
                        // go right
                        if (rightDist < minDist) {
                            correctionVector = axis;
                            minDist = rightDist;
                        }
                    }
                }
            }

            if (c1 != null) {
                var vert = s2.Vertices();
                Vector2 closest = new Vector2(0, 0);
                float closestDist = -1;

                foreach (var v in vert) {
                    float dist = (v - c1._position).sqrMagnitude;
                    if (dist < closestDist || closestDist < 0) {
                        closestDist = dist;
                        closest = v - c1._position;
                    }
                }
                var axis = closest;

                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);


                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return Vector2.zero;
                }
                else {
                    float rightDist = shadow1.y - shadow2.x;
                    float leftDist = shadow2.y - shadow1.x;

                    if (rightDist > leftDist) {
                        //go left
                        if (leftDist < minDist) {
                            correctionVector = -axis;
                            minDist = leftDist;
                        }
                    }
                    else {
                        // go right
                        if (rightDist < minDist) {
                            correctionVector = axis;
                            minDist = rightDist;
                        }
                    }
                }
            }
            if (c2 != null) {
                var vert = s1.Vertices();
                Vector2 closest = new Vector2(0, 0);
                float closestDist = -1;

                foreach (var v in vert) {
                    float dist = (v - c2._position).sqrMagnitude;
                    if (dist < closestDist || closestDist < 0) {
                        closestDist = dist;
                        closest = v - c2._position;
                    }
                }

                var axis = closest;

                var shadow1 = s1.Project(axis);
                var shadow2 = s2.Project(axis);


                if (shadow1.x > shadow2.y || shadow1.y < shadow2.x) {
                    return Vector2.zero;
                }
                else {
                    float rightDist = shadow1.y - shadow2.x;
                    float leftDist = shadow2.y - shadow1.x;

                    if (rightDist > leftDist) {
                        //go left
                        if (leftDist < minDist) {
                            correctionVector = -axis;
                            minDist = leftDist;
                        }
                    }
                    else {
                        // go right
                        if (rightDist < minDist) {
                            correctionVector = axis;
                            minDist = rightDist;
                        }
                    }
                }
            }


            return correctionVector.normalized * -minDist;
        }


        static bool CircleCollision(CircleShape c1, CircleShape c2) {
            float minDist = c1.radius + c2.radius;

            float x = c1._position.x - c2._position.x;
            float y = c1._position.y - c2._position.y;

            return x * x + y * y < minDist * minDist;
        }
        static Vector2 CircleCollisionWithCorrection(CircleShape c1, CircleShape c2) {
            float minDist = c1.radius + c2.radius;

            float x = c1._position.x - c2._position.x;
            float y = c1._position.y - c2._position.y;

            float dist = Mathf.Sqrt(x * x + y * y);

            if (dist < minDist) {
                return (c1._position - c2._position).normalized * (minDist - dist);
            }

            return Vector2.zero;
        }
    }

    public class CircleShape : Shape2D {
        private float _radius;

        public float radius { get => _radius; set { _radius = value; } }

        public CircleShape(float radius) {
            this._radius = radius;
        }
        public override Vector2 Project(Vector2 line) {
            float center = Projection(position, line);
            return new Vector2(center - _radius, center + _radius);
        }
        public override (Vector2 min, Vector2 max) AABB() {
            return (
                new Vector2(position.x - radius, position.y - radius),
                new Vector2(position.x + radius, position.y + radius)
                );
        }
        public override void OnDrawGizmos() {
            Gizmos.DrawWireSphere(position, _radius);
        }

    }

    public class RectangleShape : Shape2D {
        private float _width;
        private float _height;
        float _rotation;

        Vector2[] collisionAxes = new Vector2[4];
        Vector2[] vertices = new Vector2[4];
        (Vector2 min, Vector2 max) _AABB;

        public float width { get => _width; set { _width = value; } }
        public float height { get => _height; set { _height = value; } }
        /// <summary>
        /// in degrees
        /// </summary>
        public float rotation {
            get => _rotation;
            set {
                _rotation = value + 360f;
                _rotation %= 360f;
            }
        }

        public RectangleShape(float width, float height) {
            this._width = width;
            this._height = height;
            OnShapeUpdate += UpdateVertices;
            UpdateVertices();
        }

        public override Vector2 Project(Vector2 line) {

            var p1 = Projection(vertices[0], line);
            var p2 = Projection(vertices[1], line);
            var p3 = Projection(vertices[2], line);
            var p4 = Projection(vertices[3], line);

            float x = Mathf.Min(Mathf.Min(p1, p2), Mathf.Min(p3, p4));
            float y = Mathf.Max(Mathf.Max(p1, p2), Mathf.Max(p3, p4));
            return new Vector2(x, y);
        }
        public override (Vector2 min, Vector2 max) AABB() {
            return _AABB;
        }
        public override Vector2[] Vertices() {
            return vertices;
        }
        void UpdateVertices() {
            Vector2 w = new Vector2(
                            Mathf.Cos(Mathf.Deg2Rad * rotation) * _width / 2,
                            Mathf.Sin(Mathf.Deg2Rad * rotation) * _width / 2);
            Vector2 h = new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)) * _height / 2,
                Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)) * _height / 2);
            var v1 = w + h + position;
            var v2 = h - w + position;
            var v3 = -h - w + position;
            var v4 = w - h + position;
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
            vertices[3] = v4;
            _AABB = (
                new Vector2(Mathf.Min(Mathf.Min(v1.x, v2.x), Mathf.Min(v3.x, v4.x)), Mathf.Min(Mathf.Min(v1.y, v2.y), Mathf.Min(v3.y, v4.y))),
                new Vector2(Mathf.Max(Mathf.Max(v1.x, v2.x), Mathf.Max(v3.x, v4.x)), Mathf.Max(Mathf.Max(v1.y, v2.y), Mathf.Max(v3.y, v4.y)))
            );
            collisionAxes[0] = new Vector2(v1.y - v2.y, v2.x - v1.x);
            collisionAxes[1] = new Vector2(v2.y - v3.y, v3.x - v2.x);
            collisionAxes[2] = new Vector2(v3.y - v4.y, v4.x - v3.x);
            collisionAxes[3] = new Vector2(v4.y - v1.y, v1.x - v4.x);
        }
        
        public override Vector2[] CollisionAxes() {
            return collisionAxes;
        }

        public override void OnDrawGizmos() {

            Gizmos.DrawLine(vertices[0], vertices[1]);
            Gizmos.DrawLine(vertices[1], vertices[2]);
            Gizmos.DrawLine(vertices[2], vertices[3]);
            Gizmos.DrawLine(vertices[3], vertices[0]);
        }
    }


}