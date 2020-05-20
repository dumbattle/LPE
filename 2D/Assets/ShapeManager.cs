using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LPE2D {
    public class ShapeManager {
        IShape2D obstacle;
        LinkedList<Astroid> _shapes = new LinkedList<Astroid>();
        public static LooseQuadTreePartion<IShape2D> partionRoot;

        public ShapeManager(Vector2 mapSize) {
            partionRoot = new QuadTreePartion<IShape2D>();
            partionRoot.Initialize(mapSize / -2, mapSize / 2, 7, 5);
            obstacle = new RectangleShape(43f, 43f) {
                position = Vector2.zero
            };
            ShapeManager.partionRoot.AddShape(obstacle);
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.white;
            partionRoot.OnDrawGizmos();

            Gizmos.color = Color.green;
            obstacle.shape.OnDrawGizmos();


            foreach (var s in _shapes) {
                Gizmos.color = partionRoot.IsColliding(s) ? Color.red : Color.green;
                s.shape.OnDrawGizmos();
            }
        }

        public void AddShape(Astroid s) {
            _shapes.AddLast(s);
            partionRoot.AddShape(s);
            s.OnShapeUpdate += () => { ShapeChanged(s); };
        }
        void ShapeChanged(Astroid s) {
            partionRoot.UpdateShape(s);
        }
        public void CleanUpMapTree() {
            partionRoot.CleanUp();
        }
    }
}