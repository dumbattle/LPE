/// <summary>
/// Sequence that will play for as long as the inner sequence is playing
/// </summary>
public abstract class PassiveSequence : ISequence {
    ISequence inner;

    protected void Init(ISequence inner) {
        this.inner = inner;
    }

    //*******************************************************************************************
    // ISequence
    //*******************************************************************************************

    public virtual void Next() {
        PrePassive();
        inner.Next();
        PostPassive();
    }

    public virtual bool Done() {
        return inner.Done();
    }

    public virtual void OnComplete() {
        inner.OnComplete();
    }

    //*******************************************************************************************
    // Abstract
    //*******************************************************************************************


    protected abstract void PrePassive();
    protected abstract void PostPassive();
}
