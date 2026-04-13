using LPE.AI.LPE;
using NUnit.Framework;
using System.Collections.Generic;

public class LPEAIViewTest {
    private const int EXPECTED_INT = 10;
    private const bool EXPECTED_BOOL = false;
    private static readonly List<int> EXPECTED_INT_LIST = new() { 1, 2, 3, 4, 5, 6 };

    public class World {
        public int int1 = EXPECTED_INT;
        public bool bool1 = EXPECTED_BOOL;
        public List<int> intList = new(EXPECTED_INT_LIST);
    }

    public class TestView : LPEView<World> {
        public ViewElement<int> int1 = new();
        public ViewElement<bool> bool1 = new();
        public ViewCollection<int> intCollection1 = new();

        protected override void Capture(SnapshotContext ctx, World world) {
            ctx.SetData(int1, world.int1);
            ctx.SetData(bool1, world.bool1);
            ctx.SetData(intCollection1, world.intList);
        }
    }

    [Test]
    public void TestSnapshotElements() {
        var world = new World();
        var view = new TestView();

        view.Capture(world);

        new TestSnapshotElementsEngine().Run(view);
    }

    [Test]
    public void TestSnapshotCollection_Count() {
        // collection should contain exactly as many elements as the source list
        var world = new World();
        var view = new TestView();
        view.Capture(world);

        var engine = new CollectAllEngine();
        engine.Run(view);
        Assert.AreEqual(EXPECTED_INT_LIST.Count, engine.seen.Count);
    }

    [Test]
    public void TestSnapshotCollection_Values() {
        // values captured into the collection must match the source list in order
        var world = new World();
        var view = new TestView();
        view.Capture(world);

        var engine = new CollectAllEngine();
        engine.Run(view);
        CollectionAssert.AreEqual(EXPECTED_INT_LIST, engine.seen);
    }

    [Test]
    public void TestSnapshotUpdatesOnRecapture() {
        // calling Capture again with new world data must overwrite the previous snapshot
        var world = new World();
        var view = new TestView();
        view.Capture(world);

        world.int1 = 99;
        world.bool1 = true;
        view.Capture(world);

        var engine = new ScalarAssertEngine(99, true);
        engine.Run(view);
    }

    [Test]
    public void TestSnapshotCollectionUpdatesOnRecapture() {
        // collection re-capture must replace previous elements entirely
        var world = new World();
        var view = new TestView();
        view.Capture(world);

        world.intList = new List<int> { 10, 20, 30 };
        view.Capture(world);

        var engine = new CollectAllEngine();
        engine.Run(view);
        CollectionAssert.AreEqual(world.intList, engine.seen);
    }

    [Test]
    public void TestSnapshotCollectionShrink() {
        // shrinking the source list must not leave stale elements behind
        var world = new World();
        var view = new TestView();
        view.Capture(world);                        // 6 elements

        world.intList = new List<int> { 7, 8 };
        view.Capture(world);                        // 2 elements

        var engine = new CollectAllEngine();
        engine.Run(view);
        Assert.AreEqual(2, engine.seen.Count);
        CollectionAssert.AreEqual(world.intList, engine.seen);
    }

    [Test]
    public void TestSnapshotCollectionGrow() {
        // growing the source list must include all new elements
        var world = new World();
        var view = new TestView();

        world.intList = new List<int> { 1 };
        view.Capture(world);

        world.intList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        view.Capture(world);

        var engine = new CollectAllEngine();
        engine.Run(view);
        Assert.AreEqual(8, engine.seen.Count);
        CollectionAssert.AreEqual(world.intList, engine.seen);
    }

    [Test]
    public void TestSnapshotAttentionResetBetweenRuns() {
        // attention set during one Run must not bleed into the next Run
        var world = new World();
        var view = new TestView();
        view.Capture(world);

        var engine = new SetAttentionThenCheckEngine();
        engine.Run(view);  // sets int1 to focus
        engine.Run(view);  // on second run, int1 should start back at peripheral
    }


    // ─────────────────────────────────────────────────
    // Engines
    // ─────────────────────────────────────────────────

    // collects all visible ints from intCollection1 (attention is ignored — sees everything)
    private class CollectAllEngine : EmptyEngine {
        public List<int> seen = new();

        protected override void Attention(TestView view, AttentionContext ctx) {
            seen.Clear();
            foreach (var (element, data) in ctx.EnumerateElements(view.intCollection1)) {
                seen.Add(data);
            }
        }
    }

    private class ScalarAssertEngine : EmptyEngine {
        readonly int expectedInt;
        readonly bool expectedBool;

        public ScalarAssertEngine(int expectedInt, bool expectedBool) {
            this.expectedInt = expectedInt;
            this.expectedBool = expectedBool;
        }

        protected override void Attention(TestView view, AttentionContext ctx) {
            Assert.AreEqual(expectedInt,  ctx.GetData(view.int1));
            Assert.AreEqual(expectedBool, ctx.GetData(view.bool1));
        }
    }

    private class SetAttentionThenCheckEngine : EmptyEngine {
        int runCount;

        protected override void Attention(TestView view, AttentionContext ctx) {
            runCount++;
            if (runCount == 1) {
                ctx.SetAttentionLevel(view.int1, AttentionLevel.focus);
            }
        }

        protected override void EmitDesires(TestView view, DesireContext ctx) {
            if (runCount == 1) {
                Assert.IsTrue(ctx.TryGetData(view.int1, out var _));
            }
            else {
                Assert.IsFalse(ctx.TryGetData(view.int1, out var _));
            }
        }
    }

    private class TestSnapshotElementsEngine : EmptyEngine {
        protected override void Attention(TestView view, AttentionContext ctx) {
            Assert.AreEqual(EXPECTED_INT, ctx.GetData(view.int1));
            Assert.AreEqual(EXPECTED_BOOL, ctx.GetData(view.bool1));

            var actual = new List<int>();
            foreach (var (ele, data) in ctx.EnumerateElements(view.intCollection1)) {
                actual.Add(data);
            }

            CollectionAssert.AreEqual(EXPECTED_INT_LIST, actual);
        }
    }

    private class EmptyEngine : LensedPerceptionEngine<TestView> {
        protected override void Attention(TestView view, AttentionContext ctx) { }
        protected override void EmitDesires(TestView view, DesireContext ctx) { }
        protected override void ProposeActions(TestView view, ActionProposalContext ctx) { }
    }
}