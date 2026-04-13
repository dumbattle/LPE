using System.Collections.Generic;

namespace LPE.AI.LPE {
    public class ActionProposalContext : BaseContext {
        internal AttentionLevel attentionLevel;

        internal List<ActionProposal> proposals = new();


        internal ActionProposalContext() { }


        public void Reset(AttentionLevel attentionLevel) {
            this.attentionLevel = attentionLevel;

            foreach (var p in proposals) {
                p.Return();
            }

            proposals.Clear();
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

        public ActionProposalBuilder<TRep> BeginProposal<TRep>(TRep representative) {
            var proposal = ActionProposal<TRep, Unit>.Get(representative, default);
            proposals.Add(proposal);


            return new ActionProposalBuilder<TRep>(proposal);
        }
        public ActionProposalBuilder<TRep, TParam> BeginProposal<TRep, TParam>(TRep representative, TParam parameter) {
            var proposal = ActionProposal<TRep, TParam>.Get(representative, parameter);
            proposals.Add(proposal);


            return new ActionProposalBuilder<TRep, TParam>(proposal);
        }
    }
}
