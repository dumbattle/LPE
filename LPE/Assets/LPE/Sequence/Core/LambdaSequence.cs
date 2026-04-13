using System;
/// <summary>
/// this sequence is not pooled, so it should be used for 1 time actions only
/// </summary>
public class LambdaSequence : ISequence {
    Action lambda;
    bool didAction = false;

    public LambdaSequence(Action lambda) {
        this.lambda = lambda;
    }


    public void Next() {
        if (Done()) {
            return;
        }
        lambda();
        didAction = true;
    }
    public bool Done() {
        return didAction;
    }
    public void OnComplete() { }
}
