using System;
using NUnit.Framework;
using LPE;
using LPE.Math;
public class ObjectPool {
        [Test]
    public void GetReturn_MaintainsCorrectCounts() {
        var pool = new ObjectPool<object>(() => new object());

        const string STEP_1 = "G";
        var a = pool.Get();
        AssertPoolSize(pool, 0, 1, STEP_1);

        const string STEP_2 = STEP_1 + "R";
        pool.Return(a);
        AssertPoolSize(pool, 1, 0, STEP_2);

        const string STEP_3 = STEP_2 + "G";
        var b = pool.Get();
        AssertPoolSize(pool, 0, 1, STEP_3);

        const string STEP_4 = STEP_3 + "G";
        var c = pool.Get();
        AssertPoolSize(pool, 0, 2, STEP_4);

        const string STEP_5 = STEP_3 + "R";
        pool.Return(c);
        AssertPoolSize(pool, 1, 1, STEP_5);
    }

    [Test]
    public void DoubleReturn_Throws() {
        var pool = new ObjectPool<object>(() => new object());

        var a = pool.Get();
        var b = pool.Get();
        var c = pool.Get();
        AssertPoolSize(pool, 0, 3, "Get 3");

        pool.Return(a);
        pool.Return(b);
        AssertPoolSize(pool, 2, 1, "return 2");

        Assert.Throws<InvalidOperationException>(() => pool.Return(b), "First double return");
        AssertPoolSize(pool, 2, 1, "After first double return");

        pool.Return(c);
        AssertPoolSize(pool, 3, 0, "return final");

        Assert.Throws<InvalidOperationException>(() => pool.Return(a), "Second double return");
        AssertPoolSize(pool, 3, 0, "After second double return");
    }

    [Test]
    public void ReturnUnowned_Throws() {
        var pool = new ObjectPool<object>(() => new object());
        var a = pool.Get();
        AssertPoolSize(pool, 0, 1, "Get 1");

        Assert.Throws<InvalidOperationException>(() => pool.Return(new object()), "invalid return");
        AssertPoolSize(pool, 0, 1, "After invalid return");

        pool.Return(a);
        AssertPoolSize(pool, 1, 0, "return valid");
    }


    void AssertPoolSize<T>(ObjectPool<T> pool, int available, int used, string msg) where T : class {
        msg = $"{msg}: Pool sizes do not match expected values\n";
        Assert.AreEqual(available, pool.availableCount, msg + $"Availability\nExpected: {available}\nGot: {pool.availableCount}");
        Assert.AreEqual(used, pool.inUseCount, msg + $"Used\nExpected: {used}\nGot: {pool.inUseCount}");
        Assert.AreEqual(available + used, pool.createdCount, msg + $"Total\nExpected: {available + used}\nGot: {pool.createdCount}");
    }
}
