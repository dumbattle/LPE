using System;
using System.Collections.Generic;

namespace LPE.AI.LPE {
    internal class DesireComposition {
        internal Dictionary<Type, DesireStorage> type2storage = new();


        internal void Clear() {

            foreach(var (type, storage) in type2storage) {
                storage.Return();
            }

            type2storage.Clear();
        }



        public void AddDesire<T>(DesireStrength strength, string source) where T : ActionSpec, new() {
            var storage = GetStorage<T>();
            storage.AddDesire(null, source, strength);
        }

        public void AddDesire<T, U>(U param, DesireStrength strength, string source) where T : ActionSpec<U>, new() {
            var storage = GetStorage<U, T>();
            storage.AddDesire(param, source, strength);
        }


        DesireStorage<Unit> GetStorage<TSpec>() where TSpec : ActionSpec, new() {
            var type = typeof(TSpec);

            if (type2storage.TryGetValue(type, out var s)) {
                return (DesireStorage<Unit>)s;
            }

            var storage = DesireStorage<Unit>.Get(Singleton<TSpec>.Get());


            type2storage[type] = storage ;

            return storage;
        }

        DesireStorage<TParam> GetStorage<TParam, TSpec>() where TSpec : ActionSpec<TParam>, new() {
            var type = typeof(TSpec);

            if (type2storage.TryGetValue(type, out var s)) {
                return (DesireStorage<TParam>)s;
            }

            var storage = DesireStorage<TParam>.Get(Singleton<TSpec>.Get());

            type2storage[type] = storage;

            return storage;
        }

    }

}
