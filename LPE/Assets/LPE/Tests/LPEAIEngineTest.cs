using NUnit.Framework;
using System.Collections.Generic;
using LPE.AI.LPE;


public class LPEAIEngineTest {
    private const int EXPECTED_INT = 10;
    private const bool EXPECTED_BOOL = false;
    private static readonly List<int> EXPECTED_INT_LIST = new() { 1, 2, 3, 4, 5, 6 };

    public class TestView {
        public ViewElement<int> int1 = new ViewElement<int>(EXPECTED_INT);
        public ViewElement<bool> bool1 = new ViewElement<bool>(EXPECTED_BOOL);

        public ViewCollection<int> intCollection1 = new ViewCollection<int>(EXPECTED_INT_LIST);
    }


    // ─────────────────────────────────────────────────
    // Attention & Visibility
    // ─────────────────────────────────────────────────

    [Test]
    public void TestVisibility() {
        new AttentionFilterEngine().Run(new TestView());
    }

    [Test]
    public void TestCollectionLoopCount() {
        // {1,2,3,4,5,6} — only the 3 odd values should be visible after attention
        var engine = new CollectionCountEngine();
        engine.Run(new TestView());
        Assert.AreEqual(3, engine.seenInDesires);
    }


    // ─────────────────────────────────────────────────
    // Scoring & Selection
    // ─────────────────────────────────────────────────

    [Test]
    public void TestProposalWinsWhenMatchingDesire() {
        var winner = new MatchingDesireEngine().Run(new TestView());
        Assert.IsNotNull(winner);
        Assert.IsTrue(winner.TryGetRepresentative(out string rep));
        Assert.AreEqual("hit", rep);
    }

    [Test]
    public void TestMultipleDesiresStackScore() {
        var winner = new StackedDesireEngine().Run(new TestView());
        Assert.IsNotNull(winner);
        Assert.IsTrue(winner.TryGetRepresentative(out string rep));
        Assert.AreEqual("double", rep);
    }

    [Test]
    public void TestIncidentalDesireDoesNotScoreAsPrimary() {
        // a desire emitted at incidental strength must not contribute to primary scoring
        var engine = new StrengthFilterEngine();
        engine.Run(new TestView());
        Assert.AreEqual(0f, engine.primaryScore, "incidental desire should not count toward primary score");
        Assert.AreEqual(1f, engine.incidentalScore, "incidental desire should count toward incidental score");
    }

    [Test]
    public void TestNullReturnedWhenNoProposals() {
        var result = new EmptyEngine().Run(new TestView());
        Assert.IsNull(result);
    }
    [Test]
    public void TestPrimaryDesireWins() {
        var winner = new StrengthPriorityEngine().Run(new TestView());
        Assert.IsNotNull(winner);
        Assert.IsTrue(winner.TryGetRepresentative(out string rep));
        Assert.AreEqual("A", rep);
    }

    [Test]
    public void TestTieBreakFallThroughWins() {
        var winner = new TieBreakEngine().Run(new TestView());
        Assert.IsNotNull(winner);
        Assert.IsTrue(winner.TryGetRepresentative(out string rep));
        Assert.AreEqual("B", rep);
    }


    // ─────────────────────────────────────────────────
    // Engines
    // ─────────────────────────────────────────────────

    class AttentionFilterEngine : LensedPerceptionEngine<TestView> {
        List<int> expected = new();

        protected override void Attention(TestView view, AttentionContext ctx) {
            ctx.SetAttentionLevel(view.int1, AttentionLevel.ignored);
            ctx.SetAttentionLevel(view.bool1, AttentionLevel.focus);

            foreach (var (element, value) in ctx.EnumerateElements(view.intCollection1)) {
                if (value % 2 == 1){
                    ctx.SetAttentionLevel(element, AttentionLevel.focus);
                    expected.Add(value);
                }
            }
        }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            Assert.IsFalse(ctx.TryGetData(view.int1, out int _));
            Assert.IsTrue(ctx.TryGetData(view.bool1, out bool bool1));
            Assert.AreEqual(false, bool1);

            var actual = new List<int>();
            foreach (var item in ctx.EnumerateData(view.intCollection1)) {
                actual.Add(item);
            }

            CollectionAssert.AreEqual(expected, actual);
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            Assert.IsFalse(ctx.TryGetData(view.int1, out int _));
            Assert.IsTrue(ctx.TryGetData(view.bool1, out bool bool1));
            Assert.AreEqual(false, bool1);

            var actual = new List<int>();
            foreach (var item in ctx.EnumerateData(view.intCollection1)) {
                actual.Add(item);
            }

            CollectionAssert.AreEqual(expected, actual);
        }
    }


    class CollectionCountEngine : EmptyEngine {
        public int seenInDesires;

        protected override void Attention(TestView view, AttentionContext ctx) {
            foreach (var (element, data) in ctx.EnumerateElements(view.intCollection1)) {
                if (data % 2 == 1) {
                    ctx.SetAttentionLevel(element, AttentionLevel.focus);
                }
            }
        }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            seenInDesires = 0;
            foreach (var item in ctx.EnumerateData(view.intCollection1)) {
                seenInDesires++;
            }
        }
    }


    class MatchingDesireEngine : EmptyEngine {
        class TargetSpec : ActionSpec<int> { }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            ctx.EmitDesire<TargetSpec, int>(42, DesireStrength.primary, "test");
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            ctx.BeginProposal("miss").ReportSatisfies<TargetSpec, int>(99);
            ctx.BeginProposal("hit").ReportSatisfies<TargetSpec, int>(42);
        }
    }


    class StackedDesireEngine : EmptyEngine {
        class TargetSpec : ActionSpec<int> { }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            ctx.EmitDesire<TargetSpec, int>(42, DesireStrength.primary, "source A");
            ctx.EmitDesire<TargetSpec, int>(42, DesireStrength.primary, "source B");
            ctx.EmitDesire<TargetSpec, int>(1, DesireStrength.primary, "source B");
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            ctx.BeginProposal("double").ReportSatisfies<TargetSpec, int>(42);
            ctx.BeginProposal("one").ReportSatisfies<TargetSpec, int>(1);
            ctx.BeginProposal("none").ReportSatisfies<TargetSpec, int>(99);
        }
    }


    class StrengthFilterEngine : EmptyEngine {
        class TargetSpec : ActionSpec<int> { }

        public float primaryScore;
        public float incidentalScore;


        protected override void EmitDesires(TestView view, DesireContext ctx) {
            ctx.EmitDesire<TargetSpec, int>(42, DesireStrength.incidental, "test");
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            ctx.BeginProposal("A").ReportSatisfies<TargetSpec, int>(42);
        }

        protected override ActionProposal SelectBest(TestView view, SelectionContext ctx) {
            var proposals = new List<ActionProposal>();
            ctx.GetProposals(proposals);

            primaryScore = ctx.GetScores(proposals[0], DesireStrength.primary);
            incidentalScore = ctx.GetScores(proposals[0], DesireStrength.incidental);

            return base.SelectBest(view, ctx);
        }
    }


    class StrengthPriorityEngine : EmptyEngine {
        class TargetSpecA : ActionSpec { }
        class TargetSpecB : ActionSpec { }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            ctx.EmitDesire<TargetSpecA>(DesireStrength.primary, "A");
            ctx.EmitDesire<TargetSpecB>(DesireStrength.secondary, "B");
            ctx.EmitDesire<TargetSpecB>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecB>(DesireStrength.secondary, "D");
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            ctx.BeginProposal("A").ReportSatisfies<TargetSpecA>();
            ctx.BeginProposal("B").ReportSatisfies<TargetSpecB>();
        }
    }
    class TieBreakEngine : EmptyEngine {
        class TargetSpecA : ActionSpec { }
        class TargetSpecB : ActionSpec { }
        class TargetSpecC : ActionSpec { }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            ctx.EmitDesire<TargetSpecA>(DesireStrength.primary, "A");
            ctx.EmitDesire<TargetSpecB>(DesireStrength.primary, "B");


            ctx.EmitDesire<TargetSpecA>(DesireStrength.secondary, "A");
            ctx.EmitDesire<TargetSpecB>(DesireStrength.secondary, "BC");

            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");
            ctx.EmitDesire<TargetSpecC>(DesireStrength.secondary, "C");

            ctx.EmitDesire<TargetSpecB>(DesireStrength.incidental, "B");
        }

        protected override void ProposeActions(TestView view, ActionProposalContext ctx) {
            ctx.BeginProposal("A").ReportSatisfies<TargetSpecA>();
            ctx.BeginProposal("B").ReportSatisfies<TargetSpecB>();
            ctx.BeginProposal("C").ReportSatisfies<TargetSpecC>();
        }
    }


    class EmptyEngine : LensedPerceptionEngine<TestView> {
        protected override void Attention(TestView view, AttentionContext ctx) { }
        protected override void EmitDesires(TestView view, DesireContext ctx) { }
        protected override void ProposeActions(TestView view, ActionProposalContext ctx) { }
    }
}
