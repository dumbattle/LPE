using LPE;
using System.Collections.Generic;



public class SequenceQueue : ISequence {
    //************************************************************************************************
    // Pool
    //************************************************************************************************

    static ObjectPool<SequenceQueue> _pool = new ObjectPool<SequenceQueue>(() => new SequenceQueue());
    public static SequenceQueue GetNew() {
        var result = _pool.Get();
        result._entries.Clear(); // shouldn't be neccessary, but who knows
        result.currentSeq = 0;
        return result;
    }

    SequenceQueue() { }


    //************************************************************************************************
    // Implementation
    //************************************************************************************************

    List<Entry> _entries = new List<Entry>();

    int currentSeq;

    public void AddSequence(ISequence seq, bool playNextImmediate = false) {
        var e = new Entry(seq, playNextImmediate);
        _entries.Add(e);
    }

    //************************************************************************************************
    // ISequence
    //************************************************************************************************

    public void Next() {
        while (true) {
            if (Done()) {
                break;
            }

            // update
            _entries[currentSeq].seq.Next();

            // if not done, exit
            if (!_entries[currentSeq].seq.Done()) {
                break;
            }

            // move next
            currentSeq++;

            // finished last
            if (Done()) {
                break;
            }


            // play immediate -> go again
            if(_entries[currentSeq].playImmediate) {
                continue;
            }

            // wait for next frame
            break;

        }
    }

    public bool Done() {
        return currentSeq >= _entries.Count;
    }

    public void OnComplete() {
        foreach (var e in _entries) {
            e.seq.OnComplete();
        }

        _entries.Clear();
        _pool.Return(this);
    }

    //************************************************************************************************
    // Helpers
    //************************************************************************************************


    struct Entry {
        public ISequence seq;
        public bool playImmediate;

        public Entry(ISequence seq, bool playImmediate) {
            this.seq = seq;
            this.playImmediate = playImmediate;
        }
    }
}
