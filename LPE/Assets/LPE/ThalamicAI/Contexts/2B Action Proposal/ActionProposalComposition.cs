using System.Collections.Generic;
using System;

namespace LPE.AI.LPE {
    internal class ActionProposalComposition {
        internal Dictionary<Type, ActionProposalStorage> type2storage = new();


        public void Return() {
            foreach (var (type, storage) in type2storage) {
                storage.Return();
            }
            type2storage.Clear();
        }



        public void AddProposal<T>() where T : ActionSpec, new() {
            var storage = GetStorage<T>();
            storage.entries.Add(default);
        }

        public void AddProposal<T, U>(U param) where T : ActionSpec<U>, new() {
            var storage = GetStorage<U, T>();
            storage.entries.Add(param);
        }


        ActionProposalStorage<Unit> GetStorage<TSpec>() where TSpec : ActionSpec, new() {
            var type = typeof(TSpec);

            if (type2storage.TryGetValue(type, out var entry)) {
                return (ActionProposalStorage<Unit>)entry;
            }

            var storage = ActionProposalStorage<Unit>.Get(Singleton<TSpec>.Get(), typeof(TSpec));

            type2storage[type] = storage;

            return storage;
        }

        ActionProposalStorage<TParam> GetStorage<TParam, TSpec>() where TSpec : ActionSpec<TParam>, new() {
            var type = typeof(TSpec);

            if (type2storage.TryGetValue(type, out var entry)) {
                return (ActionProposalStorage<TParam>)entry;
            }

            var storage = ActionProposalStorage<TParam>.Get(Singleton<TSpec>.Get(), typeof(TSpec));
            type2storage[type] = storage;

            return storage;
        }



    }
}
