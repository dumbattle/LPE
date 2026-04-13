using System;

namespace LPE.AI.LPE {
    public class DesireContext : BaseContext {
        internal AttentionLevel attentionLevel;
        internal DesireComposition desires = new();

        internal DesireContext() { }

        internal void Reset(AttentionLevel attentionLevel) {
            this.attentionLevel = attentionLevel;
            desires.Clear();
        }

        public bool TryGetData<T>(ViewElement<T> element, out T result) {
            if (element.attentionLevel < attentionLevel) {
                result = default;
                return false;
            }

            result = element.data;
            return true;
        }

        public ViewCollection<T>.DataEnumerable EnumerateData<T>(ViewCollection<T> collection) {
            return new ViewCollection<T>.DataEnumerable(collection.elements, attentionLevel);
        }



        public void EmitDesire<T>(DesireStrength strength, string source) where T : ActionSpec, new() {
            //ctx.EmitDesire<IdleActionSpec>();

            desires.AddDesire<T>(strength, source);
        }

        public void EmitDesire<T, U>(U param, DesireStrength strength, string source) where T : ActionSpec<U>, new() {
            // ctx.EmitDesire<DamageActionSpec, EntityID>(new EntityID(1232));
            // Slightly verbose with redundant generic declaration, but no better alternative

            desires.AddDesire<T, U>(param, strength, source);

        }
    }

}
