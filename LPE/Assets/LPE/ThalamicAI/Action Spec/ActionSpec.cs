using System.Collections.Generic;
using UnityEngine;

namespace LPE.AI.LPE {
    public abstract class ActionSpec<TParam> : IActionSpec_internal {
        float IActionSpec_internal.ComputePartialScore(DesireStorage _desires, ActionProposalStorage _proposal, DesireStrength strength) {
            var desires = (DesireStorage<TParam>)_desires;
            var proposal = (ActionProposalStorage<TParam>)_proposal;

            var dList = ObjectPool.GetList<TParam>();


            foreach (var e in desires.entries) {
                if (e.strength != strength) {
                    continue;
                }

                dList.Add(e.param);
            }



            var result = ScoreFunction(proposal.entries, dList);
            ObjectPool.ReturnList(dList);
            return result;
        }

        protected virtual float ScoreFunction(IReadOnlyList<TParam> proposals, IReadOnlyList<TParam> desires) {
            float result = 0;

            foreach (var p in proposals) {
                foreach (var d in desires) {
                    result += Score(p, d);
                }
            }

            return result;
        }

        protected virtual float Score(TParam proposal, TParam desire) {
            return EqualityComparer<TParam>.Default.Equals(proposal, desire) ? 1 : 0;
        }
    }


    public abstract class ActionSpec : IActionSpec_internal {
        float IActionSpec_internal.ComputePartialScore(DesireStorage _desires, ActionProposalStorage _proposal, DesireStrength strength) {
            var desires = (DesireStorage<Unit>)_desires;
            var proposal = (ActionProposalStorage<Unit>)_proposal;


            int dCount = 0;

            foreach (var e in desires.entries) {
                if (e.strength != strength) {
                    continue;
                }
                dCount += 1;
            }


            return ScoreFunction(proposal.entries.Count, dCount);
        }

        protected virtual float ScoreFunction(int NumProposals, int numDesires) {
            return NumProposals * numDesires;
        }

    }
}
