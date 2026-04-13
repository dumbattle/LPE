using UnityEngine;


namespace LPE.Shape {
    class CastedShape : Shape2D {
        Shape2D source;
        Vector2 dir;

        public void Set(Shape2D source, Vector2 dir) {
            this.source = source;
            this.dir = dir;
        }

        //----------------------------------------------------------------------------------------------------
        // Transform
        //----------------------------------------------------------------------------------------------------

        public override Vector2 position {
            get => source.position; 
            protected set { source.SetPosition(value, false); }
        }

        public override float rotation {
            get => base.rotation;
            protected set { source.SetRotation(value, false); }
        }
        public override float scale {
            get => base.scale; 
            protected set { source.SetScale(value, false); }
        }

        //*****************************************************************************************************
        // override
        //*****************************************************************************************************

        public override Vector2 Project(Vector2 line) {
            Vector2 shadow = source.Project(line);
            float projectedDir = Projection(dir, line);
            if (projectedDir > 0) {
                shadow.y += projectedDir;
            }
            else {
                shadow.x += projectedDir;
            }
            return shadow;
        }

        public override (Vector2 min, Vector2 max) AABB() {
            return Math.Geometry.ExpandAABB(source.AABB(), dir);
        }

        public override void Update() { }
    }
}
