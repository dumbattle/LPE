using LPE;

public class PauseFramesSequence : ISequence {
    public static PauseFramesSequence Get(int frames) {
        var result = ObjectPool.Get<PauseFramesSequence>();
        result.remaining = frames;
        return result;
    }


    int remaining;


    public void Next() {
        remaining--;
    }

    public bool Done() {
        return remaining <= 0;
    }

    public void OnComplete() {
        ObjectPool.Return(this);
    }
}
