using UnityEngine;
using System.Collections.Generic;
using System;


namespace LPE.Shape {
    public class RectangleShape : ConvexPolygonShape {
        public RectangleShape(float width, float height) : base(GetBaseVertices(width, height)) { }

        static Vector2[] _v = new Vector2[4];
        static Vector2[] GetBaseVertices(float w, float h) {
            _v[0] = new Vector2(-w / 2, h / 2);
            _v[1] = new Vector2(w / 2, h / 2);
            _v[2] = new Vector2(w / 2, -h / 2);
            _v[3] = new Vector2(-w / 2, -h / 2);
            return _v;
        }
    }
}