using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPE.Shape;
using LPE;


public class ShapeCastMain : MonoBehaviour {
    public GameObject castStart;
    public GameObject castEnd;
    public GameObject target;

    Shape2D castShape;
    Shape2D targetShape;

    void Start() {
        castShape = ConvexPolygonShape.Regular(5);
        targetShape = ConvexPolygonShape.Regular(3);
        castShape = new CircleShape(1);
        targetShape = new CircleShape(1);
    }

    void Update() {
        castShape.SetPosition(castStart.transform.position, false);
        castShape.SetRotation(castStart.transform.rotation.eulerAngles.z, false);
        castShape.SetScale(castStart.transform.localScale.z, false);
        castShape.Update();
        targetShape.SetPosition(target.transform.position, false);
        targetShape.SetRotation(target.transform.rotation.eulerAngles.z, false);
        targetShape.SetScale(target.transform.localScale.z, false);
        targetShape.Update();
    }

    private void OnDrawGizmos() {
        if (castShape == null) {
            return;
        }

        Gizmos.color = Color.green;
        //DrawPoly(castShape.Vertices(), Vector2.zero);
        DrawCircle((CircleShape)castShape, Vector2.zero);
        Gizmos.color = Color.blue;
        //DrawPoly(castShape.Vertices(), castEnd.transform.position - castStart.transform.position);
        DrawCircle((CircleShape)castShape, castEnd.transform.position - castStart.transform.position);
        Gizmos.color = Color.white;
        //DrawPoly(targetShape.Vertices(), Vector2.zero);
        DrawCircle((CircleShape)targetShape, Vector2.zero);

        float cast = Shape2DCollision.ShapeCast(castShape, targetShape, castEnd.transform.position - castStart.transform.position);
        if (cast >= 0) {
            Gizmos.color = Color.red;
            //DrawPoly(castShape.Vertices(), (castEnd.transform.position - castStart.transform.position).normalized * cast);
            DrawCircle((CircleShape)castShape, (castEnd.transform.position - castStart.transform.position).normalized * cast);
        }
    }


    void DrawCircle(CircleShape c, Vector2 offset) {
        Gizmos.DrawWireSphere(c.position + offset, c.radius * c.scale);
    }

    void DrawPoly(Vector2[] vs, Vector2 offset) {
        for (int i = 0; i < vs.Length - 1; i++) {
            Gizmos.DrawLine(vs[i] + offset, vs[i + 1] + offset);
        }
        Gizmos.DrawLine(vs[0] + offset, vs[vs.Length - 1] + offset);
    }
}
