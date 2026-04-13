using System.Collections.Generic;

namespace LPE.AI.LPE {
    internal abstract class DesireStorage {
        public IActionSpec_internal spec;
        public abstract void Return();
        public abstract string DebugString();
    }


    internal class DesireStorage<TParam> : DesireStorage {
        static ObjectPool<DesireStorage<TParam>> _pool = new ObjectPool<DesireStorage<TParam>>(() => new());

        public static DesireStorage<TParam> Get(IActionSpec_internal spec) {
            var result = _pool.Get();
            result.entries.Clear();
            result.spec = spec;
            return result;
        }

        public override void Return() {
            entries.Clear();
            _pool.Return(this);
        }

        DesireStorage() { }


        internal List<Entry> entries = new List<Entry>();



        public void AddDesire(TParam param, string source, DesireStrength strength) {
            entries.Add(new Entry() {param = param, source = source, strength = strength });
        }

        public override string DebugString() {
            var sb = new System.Text.StringBuilder();
            foreach (var e in entries) {
                sb.AppendLine($"    [{e.strength}] {e.param} ({e.source})");
            }
            return sb.ToString();
        }

        internal struct Entry {
            public TParam param;
            public string source;
            public DesireStrength strength;
        }
    }
}
