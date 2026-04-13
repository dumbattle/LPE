using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOnGridMain : MonoBehaviour {
    public Transform start;
    public Transform end;
    public int mode;
    public int i = 0;


    Vector2Int StartPos() {
        return new Vector2Int(Mathf.FloorToInt(start.position.x), Mathf.FloorToInt(start.position.y));
    }
    Vector2Int EndPos() {
        return new Vector2Int(Mathf.FloorToInt(end.position.x), Mathf.FloorToInt(end.position.y));
    }

    private void OnDrawGizmos() {
        var line = LPE.Math.Geometry.GetLine(start.position, end.position, null, mode);
        i++;
        i %= line.Count;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(start.position, end.position);

        foreach (var t in line) {
            Gizmos.DrawWireCube(t + Vector2.one / 2, Vector3.one);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(StartPos() + Vector2.one / 2, Vector3.one);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(EndPos() + Vector2.one / 2, Vector3.one);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(line[i] + Vector2.one / 2, Vector3.one);

    }



}
