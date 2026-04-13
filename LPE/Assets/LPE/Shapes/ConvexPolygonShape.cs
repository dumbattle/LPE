using System.Collections.Generic;
using UnityEngine;


namespace LPE.Shape {
    /// <summary>
    /// Does not enforce convexness 
    /// </summary>
    public class ConvexPolygonShape : Shape2D {
        Vector2[] _srcVertices;
        Vector2[] _vertices;
        //----------------------------------------------------------------------------------------------------
        // cache for private use
        //----------------------------------------------------------------------------------------------------

        Vector2[] _collisionAxes = new Vector2[4];
        (Vector2 min, Vector2 max) _AABB;

        //*****************************************************************************************************
        // Constructors
        //*****************************************************************************************************

        public ConvexPolygonShape(params Vector2[] vertices) {
            _srcVertices = (Vector2[])vertices.Clone();
            _vertices = (Vector2[])vertices.Clone();
            _collisionAxes = new Vector2[vertices.Length];

            Update();
        }
        public ConvexPolygonShape(List<Vector2> vertices) {
            _srcVertices = vertices.ToArray();
            _vertices = vertices.ToArray();
            _collisionAxes = new Vector2[vertices.Count];

            Update();
        }

        public static ConvexPolygonShape Regular(int n) {
            Vector2[] vertices = new Vector2[n];
            float deg = 360f / n;

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = Math.Geometry.Rotate(new Vector2(0, .5f), deg * i);
            }

            return new ConvexPolygonShape(vertices);
        }
        //*****************************************************************************************************
        // Updates
        //*****************************************************************************************************

        public override void Update() {
            float cosRot = Mathf.Cos(Mathf.Deg2Rad * _rotation);
            float sinRot = Mathf.Sin(Mathf.Deg2Rad * _rotation);

            // vertices
            for (int i = 0; i < _srcVertices.Length; i++) {
                Vector2 src = _srcVertices[i];

                _vertices[i] = new Vector2(
                     (src.x * cosRot - src.y * sinRot) * _scale + position.x,
                     (src.y * cosRot + src.x * sinRot) * _scale + position.y
                );
            }

            // bounding box
            float minX = _vertices[0].x;
            float minY = _vertices[0].y;

            float maxX = _vertices[0].x;
            float maxY = _vertices[0].y;

            foreach (var v in _vertices) {
                minX = Mathf.Min(minX, v.x);
                minY = Mathf.Min(minY, v.y);
                maxX = Mathf.Max(maxX, v.x);
                maxY = Mathf.Max(maxY, v.y);
            }

            _AABB = (
                new Vector2(minX, minY),
                new Vector2(maxX, maxY)
            );

            // Collision Axes
            for (int i = 1; i < _vertices.Length; i++) {
                Vector2 v1 = _vertices[i - 1];
                Vector2 v2 = _vertices[i];
                _collisionAxes[i] = new Vector2(v2.y - v1.y, v1.x - v2.x).normalized;
            }

            _collisionAxes[0] = new Vector2(
                _vertices[_vertices.Length - 1].y - _vertices[0].y,
                _vertices[0].x - _vertices[_vertices.Length - 1].x).normalized;
        }

        //*****************************************************************************************************
        // Abstract Implementations
        //*****************************************************************************************************

        public override (Vector2 min, Vector2 max) AABB() {
            return _AABB;
        }

        public override Vector2[] CollisionAxes() {
            return _collisionAxes;
        }

        public override Vector2 Project(Vector2 line) {
            float x = Projection(_vertices[0], line); ;
            float y = x;

            for (int i = 1; i < _vertices.Length; i++) {
                float p = Projection(_vertices[i], line);
                x = Mathf.Min(x, p);
                y = Mathf.Max(y, p);
            }

            return new Vector2(x, y);
        }

        public override Vector2[] Vertices() {
            return _vertices;
        }

        public override void OnDrawGizmos() {
            for (int i = 0; i < _vertices.Length - 1; i++) {
                Gizmos.DrawLine(_vertices[i], _vertices[i + 1]);
            }
            Gizmos.DrawLine(_vertices[0], _vertices[_vertices.Length - 1]);
        }
    }
}