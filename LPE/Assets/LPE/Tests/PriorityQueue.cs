using LPE;
using NUnit.Framework;
using System.Collections.Generic;

public class PriorityQueue {
    [Test]
    public void Test_Random() {
        const int NUM_TRIALS = 10;
        const int SAMPLE_SIZE = 128;

        var pq = new PriorityQueue<int>();

        for (int _ = 0; _ < NUM_TRIALS; _++) {

            int[] prio = new int[SAMPLE_SIZE];

            for (int i = 0; i < SAMPLE_SIZE; i++) {
                prio[i] = i;

                if (i > 0) {
                    int randInd = UnityEngine.Random.Range(0, i);
                    (prio[randInd], prio[i]) = (prio[i], prio[randInd]);
                }
            }

            Dictionary<int, int> prio2index = new Dictionary<int, int>();

            for (int i = 0; i < SAMPLE_SIZE; i++) {
                prio2index.Add(prio[i], i);
                pq.Add(i, prio[i]);
            }

            for (int i = 0; i < SAMPLE_SIZE; i++) {
                Assert.AreEqual(prio2index[SAMPLE_SIZE - i - 1], pq.Get());
            }
            Assert.Zero(pq.size);
        }
    }


    [Test]
    public void Test_Interleaved_AddGet_Stress() {
        const int OPERATIONS = 1000;
        var pq = new PriorityQueue<int>();
        var oracle = new List<int>(); // We'll use this to verify results

        for (int i = 0; i < OPERATIONS; i++) {
            // Randomly choose to Add or Get (70% Add, 30% Get to ensure growth)
            float action = UnityEngine.Random.value;

            if (action < 0.7f || pq.isEmpty) {
                // Add a random priority
                int p = UnityEngine.Random.Range(0, 1000);
                pq.Add(p, p);
                oracle.Add(p);
                oracle.Sort(); // Keep sorted to simulate priority behavior
            }
            else {
                // Get the max
                float expectedMax = oracle[oracle.Count - 1];
                oracle.RemoveAt(oracle.Count - 1);

                int actualMax = pq.Get();
                Assert.AreEqual(expectedMax, (float)actualMax,
                    $"Failed during interleaved ops. Expected {expectedMax} but got {actualMax}");
            }

            // Integrity check: Count must match
            Assert.AreEqual(oracle.Count, pq.size, $"Size mismatch at op {i}");
        }
    }
}

