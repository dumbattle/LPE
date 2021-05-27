using UnityEngine;
using LPE;
using System.Collections.Generic;
namespace Example.Shooter2D {
    public class Player : Unit {
        public override Vector2 pos => obj.transform.position;
        public override Vector2 dir => _dir;
        public override float size => radius;
        Vector2 _dir;
        GameObject obj;
        float radius;


        public Player (GameObject obj, float r) {
            this.obj = obj;
            radius = r;
            obj.transform.position = Vector2.zero;
        }
        public override (Vector2 min, Vector2 max) AABB() {
            return (pos - new Vector2(radius, radius), pos + new Vector2(radius, radius));
        }

        public override void Update() {
            var input = new Vector3(
                Input.GetAxisRaw("Horizontal"),    
                Input.GetAxisRaw("Vertical")    
            );
            var spd = .1f;

            // slow when touching creep
            nearby.Clear();
            EX_2DShooterMain.um.Query(
                AABB().min,
                AABB().max,
                nearby);

            spd *= 1 - nearby.Count / (nearby.Count + 3f);
            obj.transform.position += input.normalized * spd;

            // walls
            var cv = EX_2DShooterMain.env.GetCorrection(pos, radius);
            obj.transform.position += cv.SetZ(0);
            EX_2DShooterMain.um.UpdateUnit(this);

            // shoot
            if (Input.GetMouseButton(0)) {
                var p = EX_2DShooterMain.pm.Get();
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                p.Init(pos, (target - pos).normalized + Random.insideUnitCircle * .1f, .115f);
            }
            // special
            if (Input.GetMouseButtonDown(1)) {
                for (int i = 0; i < 100; i++) {

                    var p = EX_2DShooterMain.pm.Get();
                    p.Init(pos,  Random.insideUnitCircle, .2f * Random.value);
                }
            }
            var pj = EX_2DShooterMain.pm.Get();
            pj.Init(pos, Random.insideUnitCircle, .2f * Random.value);
        }

        Vector2 target;

        public override void DrawGizmos() {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(pos, target);
        }

    }

    public class ProjectileManager {
        ObjectPool<Projectile> _pool;

        FreeList<(Projectile p, int next)> fl = new FreeList<(Projectile p, int next)>();
        int first = -1;
        public ProjectileManager(GameObject bulletObj) {
            _pool = new ObjectPool<Projectile>(() => new Projectile(GameObject.Instantiate(bulletObj)));
        }

        public Projectile Get() {
            var result = _pool.Get();

            var id = fl.Add((result, first));
            first = id;

            return result;
        }

        public void Update() {
            int i = first;
            var s = new LoopSafety(10000);
            while (i >= 0 && s.Inc()) {
                var (p, n) = fl[i];
                p.Update();
                i = n;
            }
        }

        public void Remove(Projectile p) {
            int ip = -1;
            int i = first;
            var s = new LoopSafety(1000);

            while (i >= 0 && s.Inc()) {
                var (prj, n) = fl[i];
                if (prj == p) {
                    break;
                }

                ip = i;
                i = n;
            }

            if (ip >= 0) {
                fl[ip] = (fl[ip].p, fl[i].next);
                fl.RemoveAt(i);
            }
            else {
                first = fl[i].next;
                fl.RemoveAt(i);
            }

        }

    }
    public class Projectile {
        static List<Unit> _units = new List<Unit>();
        GameObject obj;
        Vector3 dir;
        float spd;
        int numhit = 0;
        int life;
        public Projectile(GameObject obj) {
            this.obj = obj;
        }

        public void Init(Vector2 pos, Vector2 dir, float spd) {
            this.dir = dir.normalized;
            this.spd = spd*Random.Range(.9f,1.1f);
            obj.transform.position = pos;
            obj.SetActive(true);
            numhit = (int)(10 * Random.Range(.7f, 1.3f));
            life = (int)(100 * Random.Range(.9f, 1.1f));
        }

        public void Update() {
            life--;
            obj.transform.position += dir * spd * (life / 100f);
            if (life <0) {
                EX_2DShooterMain.pm.Remove(this);
                obj.SetActive(false);
                return;
            }
            // hit wall
            var cv = EX_2DShooterMain.env.GetCorrection(obj.transform.position, obj.transform.lossyScale.x/2f);

            if (cv != Vector2.zero) {
                obj.transform.position += cv.SetZ(0);
                dir = Vector2.Reflect(dir, cv.normalized);
                spd /= 2;
            }


            // hit creep

            _units.Clear();
            EX_2DShooterMain.um.Query(LPE.Math.Geometry.CircleAABB(obj.transform.position, obj.transform.lossyScale.x / 2f), _units);
            foreach (var u in _units) {
                if (u == EX_2DShooterMain.um.player) {
                    continue;
                }
                if ((u.pos - (Vector2)obj.transform.position).sqrMagnitude <= (u.size + obj.transform.lossyScale.x / 2f) * (u.size + obj.transform.lossyScale.x / 2f)) {
                    if (Random.value < .01f * numhit / 10f) {
                        u.Remove();
                    }
                    numhit--;
                    numhit = Mathf.Max(numhit, 1);
                }
            }
        }
    }
}