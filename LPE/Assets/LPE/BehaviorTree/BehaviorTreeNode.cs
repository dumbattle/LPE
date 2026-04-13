using System.Collections.Generic;

namespace LPE.AI {
}
namespace LPE {

        public enum BTEvalResult {
        none,
        fail,
        running,
        success
    }


    public partial class BTContext<TContext> {
        public TContext context { get; private set; }


        public void Eval(BehaviorTreeNode<TContext> node) {
            while (backtrackCount > 0) {
                backtrackCount--;
                path.Pop();
            }

            path.Push(node);
            result = BTEvalResult.none;
            node.InternalEval(this);
            if (result == BTEvalResult.none) {
                throw new System.Exception($"'{node.GetType().Name}' - Node Evaluation did not report status");
            }

        }

        // Must call one of these

        public void ReportSuccess() {
            Report(BTEvalResult.success);
        }
        public void ReportFail() {
            Report(BTEvalResult.fail);
        }
        public void ReportRunning() {
            Report(BTEvalResult.running);
        }


        void Report(BTEvalResult result) {
            if (this.result != BTEvalResult.none) {
                throw new System.Exception($"'{path.Peek().GetType().Name}' - Node Evaluation reported multiple times");
            }
            backtrackCount++;
            this.result = result;
        }
    }

    public partial class BTContext<TContext> {
        internal BTEvalResult result { get; private set; }

        internal Stack<BehaviorTreeNode<TContext>> path = new();
        internal int backtrackCount = 0;

        internal void Reset(TContext ctx) {
            context = ctx;
            result = BTEvalResult.none;
            path.Clear();
            backtrackCount = 0;
        }
    }


    public class BehaviorTree<TContext> {
        BehaviorTreeNode<TContext> root;

        BTContext<TContext> context = new BTContext<TContext>();


        public BehaviorTree(BehaviorTreeNode<TContext> root) {
            this.root = root;
        }

        public void Tick(TContext ctx) {
            context.Reset(ctx);
            context.Eval(root);

            UnityEngine.Debug.Log(context.path.ContentString());
        }
    }



    public abstract class BehaviorTreeNode<TContext> {
        internal void InternalEval(BTContext<TContext> ctx) {
            Eval(ctx);
        }
        protected abstract void Eval(BTContext<TContext> ctx);


        // Mutates existing trees
        // Should be fine if we forbid reusing subtrees
        public static BTNode_Sequence<TContext> operator +(BehaviorTreeNode<TContext> l, BehaviorTreeNode<TContext> r) {
            var sl = l as BTNode_Sequence<TContext>;


            if (sl != null) {
                sl.AddChild(r);
                return sl;
            }

            var result = new BTNode_Sequence<TContext>();

            result.AddChild(l);
            result.AddChild(r);
            return result;
        }


        // probably going too far...
        public static BTNode_Sequence<TContext> operator >(BehaviorTreeNode<TContext> l, BehaviorTreeNode<TContext> r) {
            return l + r;
        }
        public static BTNode_Sequence<TContext> operator <(BehaviorTreeNode<TContext> l, BehaviorTreeNode<TContext> r) {
            return r + l;
        }

        public static BTNode_Selector<TContext> operator |(BehaviorTreeNode<TContext> l, BehaviorTreeNode<TContext> r) {
            var sl = l as BTNode_Selector<TContext>;


            if (sl != null) {
                sl.AddChild(r);
                return sl;
            }

            var result = new BTNode_Selector<TContext>();

            result.AddChild(l);
            result.AddChild(r);
            return result;
        }
    }


    public class BTNode_Selector<TContext> : BehaviorTreeNode<TContext> {
        List<BehaviorTreeNode<TContext>> children = new List<BehaviorTreeNode<TContext>>();

        protected override void Eval(BTContext<TContext> ctx) {

            foreach (var n in children) {
                ctx.Eval(n);
                if (ctx.result == BTEvalResult.success) {
                    return;
                }

                if (ctx.result == BTEvalResult.running) {
                    return;
                }


            }

            ctx.ReportFail();
        }
        public void AddChild(BehaviorTreeNode<TContext> child) {
            children.Add(child);
        }
    
    }

    public class BTNode_Sequence<TContext> : BehaviorTreeNode<TContext> {
        List<BehaviorTreeNode<TContext>> children = new();

        protected override void Eval(BTContext<TContext> ctx) {

            foreach (var n in children) {
                ctx.Eval(n);
      
                if (ctx.result == BTEvalResult.fail) {
                    return;
                }

                if (ctx.result == BTEvalResult.running) {
                    return;
                }
            }
            ctx.ReportSuccess();
        }

        public void AddChild(BehaviorTreeNode<TContext> child) {
            children.Add(child);
        }
    }

    public abstract class BTNode_Condition<TContext> : BehaviorTreeNode<TContext> {
        protected sealed override void Eval(BTContext<TContext> ctx) {
            var done = CheckCondition(ctx.context);

            if (done) {
                ctx.ReportSuccess();
            }
            else {
                ctx.ReportFail();
            }
        }

        protected abstract bool CheckCondition(TContext ctx);
    }

    public abstract class BTNode_Action<TContext> : BehaviorTreeNode<TContext> {
        protected sealed override void Eval(BTContext<TContext> ctx) {
            ExecuteAction(ctx);
        }


        protected abstract void ExecuteAction(BTContext<TContext> ctx);
    }



    public struct BTBuilder<TContext> {
        List<BehaviorTreeNode<TContext>> nodes;

        BTNode_Selector<TContext> root;

        void Init() {
            if (root != null) {
                return;
            }
            root = new BTNode_Selector<TContext>(); 

            nodes = ObjectPool.GetList<BehaviorTreeNode<TContext>>();


            nodes.Add(root);

        }

        public BehaviorTree<TContext> Build() {
            ObjectPool.ReturnList(nodes);
            nodes = null;
            return new BehaviorTree<TContext>(root);
        }

        public int GetRoot() {
            Init();
            return 0;

        }

        public void AddChild(int parent, int child) {
            Init();
            if (nodes[parent] is BTNode_Selector<TContext> selector) {
                selector.AddChild(nodes[child]);
                return;
            }
            if (nodes[parent] is BTNode_Sequence<TContext> sequence) {
                sequence.AddChild(nodes[child]);
                return;
            }
            throw new System.InvalidOperationException($"{nodes[parent].GetType().Name} - cannot hold children");
        }

        public int AddChild(int parent, BehaviorTreeNode<TContext> child) {
            Init();
            nodes.Add(child);
            AddChild(parent, nodes.Count - 1);
            return nodes.Count - 1;
        }

        public int AddSequence(int parent) {
            Init();
            nodes.Add(new BTNode_Sequence<TContext>());
            AddChild(parent, nodes.Count - 1);
            return nodes.Count - 1;
        }

        public int AddSelector(int parent) {
            Init();
            nodes.Add(new BTNode_Selector<TContext>());
            AddChild(parent, nodes.Count - 1);
            return nodes.Count - 1;
        }

         
    }

    class TEST_CONDITION : BTNode_Condition<int> {
        protected override bool CheckCondition(int ctx) {
            return true;
        }
    }

    class TEST_ACTION : BTNode_Action<int> {
        protected override void ExecuteAction(BTContext<int> ctx) {
            ctx.ReportSuccess();
        }
    }

    public class TEST_BUILD {
        public static void V1() {
            var b = new BTBuilder<int>();

            var modeHappy = b.AddSequence(b.GetRoot());
            var modeSad = b.AddSequence(b.GetRoot());
            var modeAngry = b.AddSequence(b.GetRoot());

            // Happy
            b.AddChild(modeHappy, new TEST_CONDITION());
            b.AddChild(modeHappy, new TEST_CONDITION());
            b.AddChild(modeHappy, new TEST_CONDITION());
            b.AddChild(modeHappy, new TEST_ACTION());

            // Sad
            b.AddChild(modeSad, new TEST_CONDITION());
            b.AddChild(modeSad, new TEST_CONDITION());
            b.AddChild(modeSad, new TEST_ACTION());

            // Angry
            b.AddChild(modeAngry, new TEST_CONDITION());
            b.AddChild(modeAngry, new TEST_CONDITION());
            var angerSplit = b.AddSelector(modeAngry);
            {
                // path 1
                var path1 = b.AddSequence(angerSplit);
                b.AddChild(path1, new TEST_CONDITION());
                b.AddChild(path1, new TEST_CONDITION());
                b.AddChild(path1, new TEST_ACTION());

                // path 2
                b.AddChild(angerSplit, new TEST_ACTION());
            }
        }
        public static void V2() {

            var happy = new TEST_CONDITION()
                      + new TEST_CONDITION()
                      + new TEST_CONDITION()
                      + new TEST_ACTION();


            var sad   = new TEST_CONDITION() > new TEST_CONDITION() > new TEST_ACTION();


            // probably don't mix and match in same expression
            var angry = new TEST_CONDITION()
                      + new TEST_CONDITION()
                      + (
                              (new TEST_CONDITION() + new TEST_CONDITION() + new TEST_ACTION()) 
                            | (new TEST_ACTION()) 
                        );
            var root = happy | sad | angry;
        }
    }
}