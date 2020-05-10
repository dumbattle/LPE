using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LPE2D {
    public class ShapeManager {
        
        LinkedList<Astroid> _shapes = new LinkedList<Astroid>();
        public static QuadTreePartion<Astroid> partionRoot;

        public ShapeManager(Vector2 mapSize) {
            partionRoot = new QuadTreePartion<Astroid>();
            partionRoot.Initialize(mapSize / -2, mapSize / 2, 9,5);
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.white;
            partionRoot.OnDrawGizmos();

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