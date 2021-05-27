using UnityEngine;


namespace Example.Shooter2D {
    public class SpawnManager {
        GameObject creep;
        float timer = 0;
        public SpawnManager(GameObject creep) {
            this.creep = creep;
            creep.SetActive(false);
        }
        public void Update() {
            timer += .11f;
            if (timer > 1) {
                timer -= 1;
                var obj = GameObject.Instantiate(creep);
                obj.transform.localScale = new Vector3(.4f, .4f, 1);
                obj.transform.position = GetSpawnPos();
                obj.SetActive(true);
                var u = new BasicCreep(obj, .2f);

                EX_2DShooterMain.um.AddUnit(u);
            }
        }

        Vector2 GetSpawnPos() {
            if (Random.value < .5f) {
                var x = Random.Range(1, Params.Width - 1);
                var y = Random.value < .5f ? 1 : Params.Height - 1;
                return new Vector2(x, y) + Params.Offset;
            }
            else {
                
                var x = Random.value < .5f ? 1 : Params.Width - 1;
                var y = Random.Range(1, Params.Height - 1);
                return new Vector2(x, y) + Params.Offset;
            }
        }
    }

}