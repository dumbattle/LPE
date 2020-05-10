using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LPE2D {
    public class ShapeManager {
        LinkedList<Shape2D> _shapes = new LinkedList<Shape2D>();
        QuadTreePartion partionRoot;

        public ShapeManager(Vector2 mapSize) {
            partionRoot = new QuadTreePartion(mapSize / -2, mapSize / 2, 8, 3);
        }

        public void OnDrawGizmos() {
            foreach (var s in _shapes) {
                Gizmos.color = partionRoot.IsColliding(s) ? Color.red : Color.green;
                s.OnDrawGizmos();
            }
            Gizmos.color = Color.white;
            partionRoot.OnDrawGizmos();
        }

        public void AddShape(Shape2D s) {
            _shapes.AddLast(s);
            partionRoot.AddShape(s);
            s.OnChange += ShapeChanged;
        }
        void ShapeChanged(Shape2D s) {
            partionRoot.RemoveShape(s);
            partionRoot.AddShape(s);
        }
        public void CleanUpMapTree() {
            partionRoot.CleanUp();
        }
    }
}