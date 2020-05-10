using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE2D;
using LPE;

public class Main : MonoBehaviour {
    ShapeManager sm;

    RectangleShape r2;
    RectangleShape movingR;
    int size = 25;

    void Start() {
        sm = new ShapeManager(new Vector2(size * 2 + 1, size * 2 + 1));

        for (int x = -size; x <= size; x++) {
            for (int y = -size; y <= size; y++) {
                RectangleShape r = new RectangleShape(.4f, .4f);
                r.position = new Vector2(x, y);
                sm.AddShape(r);
            }
        }
        r2 = new RectangleShape(transform.localScale.x, transform.localScale.y);

        r2.rotation = Vector2.Angle(transform.right, Vector2.right);
        r2.position = transform.position;
        movingR = new RectangleShape(2, 4);

        sm.AddShape(r2);
        sm.AddShape(movingR);
    }

    private void Update() {
        r2.rotation = Vector2.Angle(transform.right, Vector2.right);
        r2.position = transform.position;
        sm.CleanUpMapTree();

        movingR.position += new Vector2(.12312f, .231423f);
        movingR.rotation += 1.123123f;

        movingR.position = new Vector2((movingR.position.x + size) % (size * 2 + 1) - size, (movingR.position.y + size) % (size * 2 + 1) - size);
    }
    void OnDrawGizmos() {
        sm?.OnDrawGizmos();
    }
}
