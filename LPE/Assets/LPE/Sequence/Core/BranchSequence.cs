
public abstract class BranchSequence : ISequence {
    ISequence trueSeq;
    ISequence falseSeq;
    bool evaled;
    bool branch;

    protected void Init(ISequence trueSeq, ISequence falseSeq) {
        this.trueSeq = trueSeq;
        this.falseSeq = falseSeq;
        evaled = false;
    }

    public void Next() {
        if (!evaled) {
            evaled = true;
            branch = CheckBranch();
        }

        var s = branch ? trueSeq : falseSeq;
        s.Next();
    }

    public bool Done() {
        if (!evaled) {
            return false;
        }
        var s = branch ? trueSeq : falseSeq;

        return s.Done();
    }

    public virtual void OnComplete() {
        trueSeq?.OnComplete();
        falseSeq?.OnComplete();

        trueSeq = null;
        falseSeq = null;
    }

    protected abstract bool CheckBranch();
}