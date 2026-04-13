using System;
using System.Collections.Generic;
using UnityEngine;


namespace LPE.Shape {
    public abstract class Shape2D {
        static Vector2[] _dummy = new Vector2[0];

        //----------------------------------------------------------------------------------------------------
        // Transform
        //----------------------------------------------------------------------------------------------------

        public virtual Vector2 position { get; protected set; }

        protected float _rotation = 0;
        protected float _scale = 1;


        public virtual float rotation {
            get => _rotation;
            protected set {
                _rotation = value + 360f;
                _rotation = ((value % 360f) + 360f) % 360f;
            }
        }
        public virtual float scale {
            get => _scale;
            protected set {
                _scale = value;
            }
        }

        //*****************************************************************************************************
        // Abstract
        //*****************************************************************************************************

        public abstract Vector2 Project(Vector2 line);
        public abstract (Vector2 min, Vector2 max) AABB();
        public virtual Vector2[] Vertices() {
            return _dummy;
        }
        public virtual Vector2[] CollisionAxes() {
            return _dummy;
        }

        //*****************************************************************************************************
        // Updates
        //*****************************************************************************************************

        public abstract void Update();


        public void SetRotation(float degrees, bool update = true) {
            rotation = degrees;

            if (update) {
                Update();
            }
        }
        public void SetPosition(Vector2 pos, bool update = true) {
            position = pos;

            if (update) {
                Update();
            }
        }
        public void SetScale(float scale, bool update = true) {
            this.scale = scale;

            if (update) {
                Update();
            }
        }

        //*****************************************************************************************************
        // Collision
        //*****************************************************************************************************

        public bool CheckCollision(Shape2D other) {
            return Shape2DCollision.CheckCollision(this, other);
        }
        public Vector2 CheckCollisionWithCorrection(Shape2D other) {
            return Shape2DCollision.CheckCollisionWithCorrection(this, other);
        }
        
        public float CheckCollisionWithCorrection(Shape2D other, Vector2 dir) {
            return Shape2DCollision.CheckCollisionWithCorrection(this, other, dir);
        }

  
        //*****************************************************************************************************
        // Utility
        //*****************************************************************************************************

        public virtual void OnDrawGizmos() { }

        public static float Projection(Vector2 point, Vector2 line) {
            return (point.x * line.x + point.y * line.y) / Mathf.Sqrt(line.x * line.x + line.y * line.y);
        }
    }


}