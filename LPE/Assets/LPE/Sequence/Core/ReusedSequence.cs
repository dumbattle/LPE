using LPE;
/// <summary>
/// When a sequence is used multiple times, OnComplete() may be called multiple times. 
/// This wrapper stops thos excess calls.
/// A single instance of this class can be used multiple times safely, will call OnComplete on the inner class once.
/// </summary>
public class ReusedSequence : ISequence {
    public static ReusedSequence Get(ISequence inner) {
        var result = ObjectPool.Get<ReusedSequence>();
        result.inner = inner;
        result.active = true;
        return result;
    }

    bool active;
    ISequence inner;

    void ISequence.Next() {
        inner.Next();
    }

    bool ISequence.Done() {
        return inner.Done();
    }

    void ISequence.OnComplete() {
        if (active) {
            inner.OnComplete();
            active = false;
            inner = null;
            ObjectPool.Return(this);
        }
    }
}
