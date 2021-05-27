using UnityEngine;
using System.Collections.Generic;
using LPE.Steering;
using LPE;

namespace Example.Shooter2D {
    public abstract class Unit : ISteerAgent {
        protected static List<Unit> nearby = new List<Unit>();
        public bool IsAlive { get; protected set; }

        public abstract Vector2 pos { get; }

        public abstract Vector2 dir { get; }

        public abstract float size { get; }

        public abstract (Vector2 min, Vector2 max) AABB();

        public abstract void Update();

        public virtual void DrawGizmos() { }
        public virtual void Remove() { }
    }


    public class BasicCreep : Unit {
        public override Vector2 pos => obj.transform.position;
        public override Vector2 dir => _dir;
        public override float size => radius;
        List<Vector2> path;

        Vector2 _dir;
        float radius;
        GameObject obj;
        int frameCount = 0;


        public BasicCreep(GameObject obj, float r) {
            this.obj = obj;
            radius = r;
            path = new List<Vector2>();
        }
        public override (Vector2 min, Vector2 max) AABB() {
            return (pos - new Vector2(radius, radius), pos + new Vector2(radius, radius));
        }

        public override void Update() {
            frameCount++;
            var target = EX_2DShooterMain.um.player.pos;
            var dd = (target - pos).sqrMagnitude;

            if (dd < (EX_2DShooterMain.um.player.size + size + .1f) * (EX_2DShooterMain.um.player.size + size + .1f)) {
                // near
                return;
            }
            // get path
            // no need to recalculate path every frame
            if (frameCount >= 10|| path.Count < 2) {
                path.Clear();
                path = EX_2DShooterMain.env.GetFirstInPath(pos, target, size, path);
                frameCount = 0;
            }
            target = path[1];

            // steer
            nearby.Clear();
            EX_2DShooterMain.um.Query(
                pos - new Vector2(radius * 2, radius * 2), 
                pos + new Vector2(radius * 2, radius * 2), 
                nearby);

            if (nearby.Contains(EX_2DShooterMain.um.player)) {
                nearby.Remove(EX_2DShooterMain.um.player);
            }
            _dir = Steering.Basic(target, this, nearby);
            obj.transform.position += _dir.SetZ(0) * .03f;


            // walls
            var cv = EX_2DShooterMain.env.GetCorrection(pos, radius);
            obj.transform.position += cv.SetZ(0);



            EX_2DShooterMain.um.UpdateUnit(this);

        }
        
        
        public override void Remove() {
            GameObject.Destroy(obj);
            obj.SetActive(false);
            EX_2DShooterMain.um.Remove(this);
        }
        public override void DrawGizmos() {
            if (path == null) {
                return;
            }
            Gizmos.color = Color.black;

            for (int i = 0; i < path.Count - 1; i++) {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }

    }

}