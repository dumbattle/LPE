public static class SequenceUtility {
    public static ISequence WithParallel(this ISequence a, ISequence b) {
        return ParallelSequence.Get(a, b);
    }

    /// <inheritdoc cref="ReusedSequence"/>
    public static ISequence Reuse(this ISequence s) {
        return ReusedSequence.Get(s);
    }


    public static ISequence Then(this ISequence first, ISequence second, bool playNextImmediate = false) {
        if (first is SequenceQueue q) {
            q.AddSequence(second, playNextImmediate);
            return q;
        }
        else {
            q = SequenceQueue.GetNew();
            q.AddSequence(first);
            q.AddSequence(second, playNextImmediate);
            return q;
        }
    }
}
