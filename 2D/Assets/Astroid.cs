using System.Collections.Generic;
using UnityEngine;
using LPE2D;

public class Astroid : IShape2D {
    CircleShape _rect;
    CircleShape _neighborCirc;
    Vector2 prevDirection = Vector2.zero;

    public Shape2D shape => _rect;
    public event System.Action OnShapeUpdate {
        add => shape.OnShapeUpdate += value;
        remove => shape.OnShapeUpdate -= value;
    }

    float angularSpeed;

    List<IShape2D> al = new List<IShape2D>();
    Vector2 push;
    int pushCount = 0;

    public Astroid() {
        angularSpeed = Random.Range(0f, 10f);
        _rect = new CircleShape(2);
        _rect.position = new Vector2(Random.Range(-Main.size, Main.size), Random.Range(-Main.size, Main.size));

        _neighborCirc = new CircleShape(4f);
    }

    public void Update() {
        bool startedColliding = ShapeManager.partionRoot.IsColliding(this);

        Vector2 prevPos = _rect.position;
        Vector2 direction = (Vector2)Main.Target.transform.position - shape.position;

        bool inRange = direction.sqrMagnitude < 15 * 15;

        if (inRange) {
            direction = Vector2.zero;
        }
        else {
            if (pushCount > 0) {
                direction += push / pushCount;

                push = Vector2.zero;
                pushCount = 0;
            }
        }


        direction.Normalize();

        if (!inRange) {
            direction += Seperation();
            direction = Vector2.Lerp(prevDirection, direction, .5f);
        }

        direction.Normalize();
        Move(direction);

        if (pushCount > 0) {
            var p = push;
            push = Vector2.zero;
            pushCount = 0;
            Move(p);
        }

        if (!startedColliding && ShapeManager.partionRoot.IsColliding(this)) {
            _rect.position = prevPos;
        }
        else {
            _rect.UpdateShape();

        }
        prevDirection = _rect.position - prevPos;
        if (prevDirection.sqrMagnitude > 100) {
            prevDirection = Vector2.zero;
        }
    }

    private Vector2 Seperation() {
        _neighborCirc.position = _rect.position;
        Vector2 result = Vector2.zero;
        ShapeManager.partionRoot.GetOverlap(_neighborCirc, al);

        if (al.Count > 1) {

            foreach (var o in al) {
                Vector2 cv = (_neighborCirc.CheckCollisionWithCorrection(o));

                result += cv;
            }
        }

        return result;
    }

    void Move(Vector2 direction) {
        Vector2 prev = _rect.position;
        _rect.position += direction.normalized;
        al.Clear();
        ShapeManager.partionRoot.GetOverlap(this, al);

        if (al.Count > 1) { // dont include self
            foreach (var o in al) {
                if (o is Astroid a) {
                    Vector2 cv = _rect.CheckCollisionWithCorrection(o);
                    a.push -= cv;
                    a.pushCount++;

                    push += cv;
                    pushCount++;
                }
            }
        }

        _rect.position = new Vector2(
            (_rect.position.x + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size,
            (_rect.position.y + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size);

    }
}

//public class Astroid : IShape2D {
//    CircleShape _rect;
//    CircleShape _neighborCirc;
//    Vector2 momentum = Vector2.zero;

//    public Shape2D shape => _rect;
//    public event System.Action OnShapeUpdate {
//        add => shape.OnShapeUpdate += value;
//        remove => shape.OnShapeUpdate -= value;
//    }

//    float angularSpeed;

//    List<Astroid> al = new List<Astroid>();
//    Vector2 push;
//    int pushCount = 0;

//    public Astroid() {
//        angularSpeed = Random.Range(0f, 10f);
//        _rect = new CircleShape(2);
//        _rect.position = new Vector2(Random.Range(-Main.size, Main.size), Random.Range(-Main.size, Main.size));

//        _neighborCirc = new CircleShape(4f);
//    }

//    public void Update() {
//        bool startedColliding = ShapeManager.partionRoot.IsColliding(this);
//        Vector2 prevPos = _rect.position;

//        Vector2 direction = (Vector2)Main.Target.transform.position - shape.position + momentum * .5f;
//        bool inRange = direction.sqrMagnitude < 10 * 10;
//        if (inRange) {
//            direction = Vector2.zero;

//        }
//        else {

//            if (pushCount > 0) {
//                direction += push / pushCount;

//                push = Vector2.zero;
//                pushCount = 0;
//            }
//        }

//        direction = direction.normalized;

//        _neighborCirc.position = _rect.position;

//        if (!inRange) {
//            var sep = Vector2.zero;
//            al.Clear();
//            sep = Seperation(sep);
//            direction += sep;
//        }


//        Move(direction.normalized);

//        if (pushCount > 0) {
//            var p = push;
//            push = Vector2.zero;
//            pushCount = 0;
//            Move(p);
//        }

//        if (al.Count > 0 && !startedColliding) {
//            _rect.position = prevPos;
//        }
//        momentum = _rect.position - prevPos;
//    }

//    private Vector2 Seperation(Vector2 c) {
//        ShapeManager.partionRoot.GetOverlap(_neighborCirc, al);

//        if (al.Count > 1) {

//            foreach (var o in al) {
//                Vector2 cv = (_neighborCirc.CheckCollisionWithCorrection(o));

//                c += cv;
//            }
//        }

//        return c;
//    }

//    void Move(Vector2 direction) {
//        Vector2 prev = _rect.position;
//        _rect.position += direction.normalized;
//        al.Clear();
//        ShapeManager.partionRoot.GetOverlap(this, al);

//        if (al.Count > 0) {
//            foreach (var o in al) {
//                Vector2 cv = _rect.CheckCollisionWithCorrection(o);
//                o.push -= cv;
//                o.pushCount++;

//                push += cv;
//                pushCount++;
//            }
//        }

//        _rect.position = new Vector2(
//            (_rect.position.x + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size,
//            (_rect.position.y + Main.size * 3 + 1) % (Main.size * 2 + 1) - Main.size);

//        _rect.UpdateShape();
//    }
//}