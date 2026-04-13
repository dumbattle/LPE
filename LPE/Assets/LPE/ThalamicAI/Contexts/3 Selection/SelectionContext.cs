using System.Collections.Generic;

namespace LPE.AI.LPE {
    public class SelectionContext : BaseContext {

        List<ActionProposal> proposals;
        DesireComposition desires = new();


        internal SelectionContext() { }


        internal void Reset(List<ActionProposal> proposals, DesireComposition desires) {
            this.proposals = proposals;
            this.desires = desires;
        }




        public T GetData<T>(ViewElement<T> element) {
            return element.data;
        }

        public ViewCollection<T>.DataEnumerable EnumerateData<T>(ViewCollection<T> collection) {
            return new ViewCollection<T>.DataEnumerable(collection.elements, AttentionLevel.ignored);
        }


        public void GetProposals(List<ActionProposal> results) {
            results.AddRange(proposals);
        }


        public void GetSatisfactions(ActionProposal proposal, List<IActionSpec> results){
            foreach (var (type, storage) in proposal.report.type2storage) {
                results.Add(storage.spec);
            }
        }


        public float GetScores(ActionProposal proposal, DesireStrength desireStrength, Dictionary<IActionSpec, float> results = null) {
            results?.Clear();
            var r = proposal.report;
            var totalScore = 0.0f;


            foreach (var (type, storage) in r.type2storage) {
                var partialDesire = storage.GetMatching(desires);
                if (partialDesire == null) {
                    continue;
                }
                var spec = partialDesire.spec;

                var score = spec.ComputePartialScore(partialDesire, storage, desireStrength);
                results?.Add(spec, score);
                totalScore += score;
            }


            return totalScore;
        }


    }
}
