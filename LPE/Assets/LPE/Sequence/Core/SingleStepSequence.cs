public abstract class SingleStepSequence : ISequence {
    public void Next() {
        Call();
    }

    public bool Done() {
        return true;
    }
    public abstract void Call();
    public abstract void OnComplete();

    public void ExecuteImmediate() {
        Next();
        OnComplete();
    }
}
