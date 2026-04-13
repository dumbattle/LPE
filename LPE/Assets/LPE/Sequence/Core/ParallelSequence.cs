using LPE;
using System.Collections.Generic;

public class ParallelSequence : ISequence {
    //************************************************************************************************
    // Pool
    //************************************************************************************************
    static ObjectPool<ParallelSequence> _pool = new ObjectPool<ParallelSequence>(() => new ParallelSequence());
    public static ParallelSequence Get(ISequence a, ISequence b) {
        var result = _pool.Get();
        result.entries.Add(new Entry(a));
        result.entries.Add(new Entry(b));
        return result;
    }

    public static ParallelSequence Get(ISequence a, ISequence b, ISequence c) {
        var result = _pool.Get();
        result.entries.Add(new Entry(a));
        result.entries.Add(new Entry(b));
        result.entries.Add(new Entry(c));
        return result;
    }

    ParallelSequence() { }

    //************************************************************************************************
    // Implementation
    //************************************************************************************************
    List<Entry> entries = new List<Entry>();

    public void Next() {
        for (int i = 0; i < entries.Count; i++) {
            Entry e = entries[i];
            if (!e.done) {
                e.seq.Next();
            }
            e.done = e.seq.Done();
            entries[i] = e;
        }

    }

    public bool Done() {
        foreach (var e in entries) {
            if (!e.done) {
                return false;
            }
        }
        return true;
    }

    public void OnComplete() {
        foreach (var e in entries) {
            e.seq.OnComplete();
        }
        entries.Clear();
        _pool.Return(this);
    }

    struct Entry {
        public ISequence seq;
        public bool done;

        public Entry(ISequence seq) {
            this.seq = seq;
            done = false;
        }
    }
}
