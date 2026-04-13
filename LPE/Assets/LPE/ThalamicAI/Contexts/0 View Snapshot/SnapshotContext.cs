
using System.Collections.Generic;

namespace LPE.AI.LPE {
    public class SnapshotContext {
        internal SnapshotContext() { }


        public void SetData<T>(ViewElement<T> element, T data) {
            element.data = data;
        }
        public void SetData<T>(ViewCollection<T> element, List<T> data) {
            element.SetElements(data);
        }
    }
}
