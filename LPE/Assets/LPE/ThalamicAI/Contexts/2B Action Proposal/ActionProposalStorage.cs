using System;
using System.Collections.Generic;

namespace LPE.AI.LPE {
    internal abstract class ActionProposalStorage {
        public IActionSpec spec;
        protected Type specType;

        public abstract void Return();
        public abstract DesireStorage GetMatching(DesireComposition desires);
        public abstract string DebugString();
    }

    internal class ActionProposalStorage<TParam> : ActionProposalStorage {
        public List<TParam> entries = new List<TParam>();


        static ObjectPool<ActionProposalStorage<TParam>> _pool = new (() => new());

        ActionProposalStorage() { }


        public static ActionProposalStorage<TParam> Get(IActionSpec spec, Type specType) {
            var result = _pool.Get();
            result.entries.Clear();
            result.spec = spec;
            result.specType = specType;
            return result;
        }

        public override void Return() {
            entries.Clear();
            _pool.Return(this);
        }




        public override string DebugString() {
            var sb = new System.Text.StringBuilder();
            foreach (var e in entries) {
                sb.AppendLine($"      {e}");
            }
            return sb.ToString();
        }

        public override DesireStorage GetMatching(DesireComposition desires) {
            foreach (var (type, storage) in desires.type2storage) {
                if (type == specType) {
                    return storage;
                }
            }

            return null;
        }

    }

}
