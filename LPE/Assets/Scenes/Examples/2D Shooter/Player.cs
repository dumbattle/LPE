using UnityEngine;
using LPE;


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
        }

    }

}