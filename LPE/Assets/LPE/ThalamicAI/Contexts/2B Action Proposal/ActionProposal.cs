

namespace LPE.AI.LPE {
    public abstract class ActionProposal  {
        internal ActionProposalComposition report = new ();
        
        internal abstract void Return();
        public abstract bool TryGetRepresentative<T>(out T result);
        public abstract bool TryGetParam<T>(out T result);



    }

    internal class ActionProposal<TCallback, TParam> : ActionProposal {
        static ObjectPool<ActionProposal<TCallback, TParam>> _pool = new (() => new());
        public static ActionProposal<TCallback, TParam> Get(TCallback callback, TParam param) {
            var result = _pool.Get();
            result.callback = callback;
            result.param = param;   
            return result;
        }

        internal override void Return() {
            report.Return();
            _pool.Return(this);
        }


        ActionProposal() { }



        internal TParam param { get; private set; }
        internal TCallback callback { get; private set; }



        public override bool TryGetRepresentative<T>(out T result) {

            if (callback is T t) {
                result = t;
                return true;
            }
            result = default;
            return false;
        }
        public override bool TryGetParam<T>(out T result) {
            if (param is T t) {
                result = t;
                return true;
            }
            result = default;
            return false;
        }
    }

}
