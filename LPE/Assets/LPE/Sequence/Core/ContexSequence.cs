public abstract class ContexSequence : ISequence {
    ISequence inner;
    bool frame1;
    bool done;

    protected void Init(ISequence inner) {
        this.inner = inner;
        frame1 = true;
        done = false;
    }

    public virtual void Next() {
        if (done) {
            return;
        }

        if (frame1) {
            BeforeStart();
            frame1 = false;
        }

        inner.Next();

        if (inner.Done()) {
            done = true;
            AfterEnd();
        }
    }

    public virtual bool Done() {
        return inner.Done();
    }

    public virtual void OnComplete() {
        inner.OnComplete();
    }

    protected abstract void BeforeStart();
    protected abstract void AfterEnd();
}
