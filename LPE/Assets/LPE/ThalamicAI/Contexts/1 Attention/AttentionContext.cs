
using System.Collections.Generic;

namespace LPE.AI.LPE {
    public class AttentionContext : BaseContext {
        HashSet<IViewElement> modified = new();
        internal AttentionContext() { }

        public T GetData<T>(ViewElement<T> element) {
            return element.data;
        }

        public void SetAttentionLevel<TData>(ViewElement<TData> element, AttentionLevel lvl) {
            element.attentionLevel = lvl;
            modified.Add(element);
        }

        public ViewCollection<T>.FullEnumerable EnumerateElements<T>(ViewCollection<T> collection) {
            return new ViewCollection<T>.FullEnumerable(collection.elements, AttentionLevel.ignored);
        }

        internal void ResetModifiedAttentions() {
            foreach (var e in modified) {
                e.attentionLevel = AttentionLevel.peripheral;
            }
            modified.Clear();
        }
    }
}
