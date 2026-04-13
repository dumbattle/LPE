using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RayAABBIntersectionTestMain : MonoBehaviour {
    public Transform AABB;

    public Transform rayStart;
    public Transform rayEnd;


    (Vector2 min, Vector2 max) GetAABB() {
        return (AABB.position - AABB.lossyScale / 2, AABB.position + AABB.lossyScale / 2);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        var aabb = GetAABB();
        Gizmos.DrawCube((aabb.min + aabb.max) / 2, aabb.max - aabb.min);

        var dir = rayEnd.position - rayStart.position;


        Gizmos.color = LPE.Math.Geometry.RayAABBIntersection(aabb, rayStart.position, new Vector2(1f / dir.x, 1f / dir.y)) ?  Color.red : Color.green;
        Gizmos.DrawLine(rayStart.position, rayEnd.position);
    }

   
}
