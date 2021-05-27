using System.Collections;
using UnityEngine;
using LPE;
using LPE.Steering;


namespace Example.Shooter2D {
    public static class Params {
        public const int Width = 50;
        public const int Height = 50;
        public const int SpawnBuffer = 3;

        public static Vector2 Offset => -new Vector2(Width, Height) / 2;
    }
    public class EX_2DShooterMain : MonoBehaviour {
        public GameObject wallSprite;
        public GameObject bgSprite;
        public GameObject playerObj;
        public GameObject creepObj;
        public GameObject bulletObj;


       public static Enviroment env;
        public static UnitManager um;
        public static SpawnManager sm;
        public static ProjectileManager pm;
        void Start() {
            env = new Enviroment(wallSprite, bgSprite);
            um = new UnitManager(playerObj);
            sm = new SpawnManager(creepObj);
            pm = new ProjectileManager(bulletObj);
        }

        void Update() {
            um.Update();
            sm.Update();
            pm.Update();
            Camera.main.transform.position = um.player.pos.SetZ(-10);
        }

        private void OnDrawGizmos() {
            env?.DrawGizmos();
            um?.DrawGizmos();
        }
    }

}