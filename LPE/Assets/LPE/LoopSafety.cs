using UnityEngine;
namespace LPE {
    public struct LoopSafety {
        const int THROW = 1;
        const int LOG = 2;
        public bool hitLimit;
        int mode;
        public int i;
        int count;
        string msg;


        public LoopSafety(int count) {
            this.count = count;
            mode = LOG;
            i = 0;
            msg = "Safety reached";
            hitLimit = false;
        }

        public void SetException(string msg = null) {
            this.msg = msg;
            mode = THROW;
        }

        public void SetLog(string msg = null) {
            this.msg = msg;
            mode = LOG;
        }

        public bool Inc() {
            i++;

            if (i >= count) {
                if (mode == THROW) {
                    throw new System.Exception(msg);
                }

                if (msg != null) {
                    Debug.Log(msg);
                }
                hitLimit = true;
                return false;
            }
            return true;
        }

    }
}