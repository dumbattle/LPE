using System.Collections.Generic;
using UnityEngine;

namespace LPE.AI.LPE {

    /// <summary>
    /// 2 Core Ideas:
    ///
    /// 1) Statefullness in achieved by restricting world view.
    /// If I am aggressive, I see low health Enemies.
    /// If I am timid, I see nearby enemies
    ///
    /// 2) Action Selection is a competition (Different parts of brain wants different things. PFC decides)
    /// Each action proposes 1 or more executions(execution parameters + granular effects (exdamage enetity entity 1)
    /// Agent computes a list of desires (ex. Damage Entity 1, Damage Entity 2, Flee Entity 3)
    /// Which actions Scores tthe best? we select that one.
    /// Both proposals and desires only consider what the agent sees in the restricted world view
    ///
    /// </summary>
    public abstract class LensedPerceptionEngine<TView> {

        AttentionContext attnCtx = new AttentionContext();
        DesireContext desireCtx = new DesireContext();
        ActionProposalContext actionProposalCtx = new();
        SelectionContext selectionCtx = new();


        public ActionProposal Run(TView view) {
            desireCtx.Reset(AttentionLevel.focus);
            actionProposalCtx.Reset(AttentionLevel.focus);
            selectionCtx.Reset(actionProposalCtx.proposals, desireCtx.desires);

            Attention(view, attnCtx);
            EmitDesires(view, desireCtx);
            ProposeActions(view, actionProposalCtx);
            var best = SelectBest(view, selectionCtx);

            attnCtx.ResetModifiedAttentions();
            return best;
        }
        protected abstract void Attention(TView view, AttentionContext ctx);
        protected abstract void EmitDesires(TView view, DesireContext ctx);
        protected abstract void ProposeActions(TView view, ActionProposalContext ctx);


        public void DebugRun(TView view) {
            desireCtx.Reset(AttentionLevel.focus);
            actionProposalCtx.Reset(AttentionLevel.focus);
            selectionCtx.Reset(actionProposalCtx.proposals, desireCtx.desires);

            Attention(view, attnCtx);

            EmitDesires(view, desireCtx);
            Debug.Log(DebugStringDesires());

            ProposeActions(view, actionProposalCtx);
            Debug.Log(DebugStringProposals());

            var best = SelectBest(view, selectionCtx);
            Debug.Log(DebugStringSelection(best));

            attnCtx.ResetModifiedAttentions();
        }

        string DebugStringDesires() {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Desires ===");
            foreach (var (type, storage) in desireCtx.desires.type2storage) {
                sb.AppendLine($"  {type.Name} ({storage.spec.GetType().Name}):");
                sb.Append(storage.DebugString());
            }
            return sb.ToString();
        }

        string DebugStringProposals() {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Proposals ===");
            foreach (var p in actionProposalCtx.proposals) {
                p.TryGetRepresentative<object>(out var rep);
                p.TryGetParam<object>(out var param);
                sb.AppendLine($"  \"{rep}\" [{param}]");
                foreach (var (type, storage) in p.report.type2storage) {
                    sb.AppendLine($"    satisfies: {type.Name}");
                    sb.Append(storage.DebugString());
                }
            }
            return sb.ToString();
        }

        string DebugStringSelection(ActionProposal best) {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Selection ===");
            foreach (var p in actionProposalCtx.proposals) {
                p.TryGetRepresentative<object>(out var rep);
                p.TryGetParam<object>(out var param);
                float score = selectionCtx.GetScores(p, DesireStrength.primary);
                sb.AppendLine($"  \"{rep}\" [{param}] -> {score}");
            }
            if (best != null) {
                best.TryGetRepresentative<object>(out var rep);
                best.TryGetParam<object>(out var param);
                sb.AppendLine($"  Best: \"{rep}\" [{param}]");
            } else {
                sb.AppendLine("  Best: (none)");
            }
            return sb.ToString();
        }

        protected virtual ActionProposal SelectBest(TView a, SelectionContext selectionContext) {
            List<ActionProposal> proposals = ObjectPool.GetList<ActionProposal>();
            List<ActionProposal> winners = ObjectPool.GetList<ActionProposal>();

            selectionContext.GetProposals(proposals);


            foreach (var d in DesireStrengthUtility.DesiresDescending) {
                float bestScore = float.MinValue;
                foreach (var p in proposals) {
                    float score = selectionContext.GetScores(p, d);

                    if (score > bestScore) {
                        bestScore = score;
                        winners.Clear();
                        winners.Add(p);
                    }
                    else if (score == bestScore) {
                        winners.Add(p);
                    }
                }

                if (winners.Count == 1) {
                    break;
                }
                proposals.Clear();
                (proposals, winners) = (winners, proposals);
            }


            var result = winners.Count > 0 
                ? winners[UnityEngine.Random.Range(0, winners.Count)]
                : null;

            ObjectPool.ReturnList(proposals);
            ObjectPool.ReturnList(winners);

            return result;
        }


    }

    //*************************************************************************
    #region Sample Context Usage
    //*************************************************************************

    /*
    This system is designed to make implementing agent behaviors as easy as possible.
    The 'view' is decoupled from the engine, so you can add new fields as needed without breaking anything

    Each system will recieve a view object, and a context.
    The view object will be shared to prevent duplication
    */


    // allows querying entity data (such as health/position etc.)
    // so we don't have to populate view with complete entity data
    // This assumes a context can see all or none of an entity's state
    // If need more specificity:
    /*
    ViewCollection<Entity> entities;
    struct Entity {
        public ViewElement<int> hp;
        public ViewElement<Vector2> position;
    }
     */
    public class EntityQuery {
        //WorldData world;

        // assuming a context has access to an EntityID, it has access to its HP
        //int GetHP(EntityID id) {
        //    return world.entities[id].hp;
        //}
    }

    public class TEST_VIEW {

        public EntityQuery enntityQuery;

        public ViewElement<int> opponentScore;
        public ViewCollection<EntityID> entities;
    }


    /*
     * Demonstration of how a basic user implementations would look
     * Each phase will be a function
     * Engine will manage context creation and data flow
     * Details such as active rules, conflict resolution, etc. will be implemented internally
     * Implementation wills likely be compositional
     *   EnemyCloseAttention + EnemyLowAttention + AllyLowAttention => each applies there own filter
     *
     * This implementation explores the path of least work.
     */
    public class TEST : LensedPerceptionEngine<TEST_VIEW> {

        public class IdleActionSpec : ActionSpec { }
        public class DamageActionSpec : ActionSpec<EntityID> { }
        public class DisruptActionSpec : ActionSpec<EntityID> { }


        public void GameUpdate(TEST_VIEW world) {
            var action = Run(world);
            Execute(world, action);
        }

        // caller will manage. Engine only does action selection
        void Execute(TEST_VIEW env, ActionProposal winner) {
            winner.TryGetRepresentative(out string winnerMsg); // we ouly use strings as a rep, so this will always pass

            if (winnerMsg == "Attack") {
                if (winner.TryGetParam(out EntityID target)) {
                    // execute attack
                    // if the representative is a delegate, or ActionDefinition,
                    // we can call it instead and keep this phase clean
                    // winnerAction.Execute(target)
                }
            }
            else if (winnerMsg == "Idle") {
                // do idle
            }

        }


        protected override void Attention(TEST_VIEW env, AttentionContext attnCtx) {
            if (attnCtx.GetData(env.opponentScore) > 50) {
                attnCtx.SetAttentionLevel(env.opponentScore, AttentionLevel.focus);
            }
            else {
                attnCtx.SetAttentionLevel(env.opponentScore, AttentionLevel.ignored);
            }

            // Slightly unconventional loop, but straightforward (once you learn it)
            // All layers will use similar pattern, which keeps it consistant
            foreach (var (element, entityID) in attnCtx.EnumerateElements(env.entities)) {
                bool isFarAway = false;

                if (isFarAway) {
                    attnCtx.SetAttentionLevel(element, AttentionLevel.ignored);
                }
            }
        }

        protected override void EmitDesires(TEST_VIEW env, DesireContext desireCtx) {

            foreach (var entityID in desireCtx.EnumerateData(env.entities)) {

                bool isLowHealth = false;
                int tags = 0b1010101; // TODO - proper tagging system

                if (isLowHealth) {
                    desireCtx.EmitDesire<DamageActionSpec, EntityID>(entityID, DesireStrength.primary, "Low Health");
                }

                const int DPS_FLAG = 0b1;
                if ((tags & DPS_FLAG) == DPS_FLAG) {
                    desireCtx.EmitDesire<DamageActionSpec, EntityID>(entityID, DesireStrength.primary, "High DPS");
                }
            }
            desireCtx.EmitDesire<IdleActionSpec>(DesireStrength.idle, "Idle Fallback");
        }

        protected override void ProposeActions(TEST_VIEW env, ActionProposalContext proposalCtx) {

            float closestDist = float.MaxValue;
            EntityID closest = EntityID.INVALID;

            foreach (var entityID in proposalCtx.EnumerateData(env.entities)) {

                Vector2 agentPos = new Vector2();
                Vector2 otherPos = new Vector2();

                var dist = (agentPos - otherPos).sqrMagnitude;
                if (dist < closestDist) {
                    closestDist = dist;
                    closest = entityID;
                }

                bool otherLowHP = false;

                if (otherLowHP) {
                    proposalCtx.BeginProposal("Attack", entityID)  // using raw string is bad. at the very least an enum, or a delegate
                        .ReportSatisfies<DamageActionSpec, EntityID>(entityID)
                        .ReportSatisfies<DisruptActionSpec, EntityID>(entityID)
                    ;
                }
            }


            if (closest != EntityID.INVALID) {
                proposalCtx.BeginProposal("Attack", closest)
                    .ReportSatisfies<DamageActionSpec, EntityID>(closest)
                    .ReportSatisfies<DisruptActionSpec, EntityID>(closest)
                ;
            }

            proposalCtx.BeginProposal("Idle")
                .ReportSatisfies<IdleActionSpec>()
            ;
        }
    }


    #endregion
    //*************************************************************************



}
