using NUnit.Framework;
using UnityEngine;
public class Geometry {
    // Helper alias
    private static bool Intersects(
        Vector2Int a1, Vector2Int a2,
        Vector2Int b1, Vector2Int b2,
        bool requireProperCrossing) {
        return LPE.Math.Geometry.IsIntersecting(a1, a2, b1, b2, requireProperCrossing);
    }

    [Test]
    public void ProperCrossing_ShouldIntersect_Strict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(4, 4);
        var b1 = new Vector2Int(0, 4);
        var b2 = new Vector2Int(4, 0);

        Assert.IsTrue(Intersects(a1, a2, b1, b2, true));
    }

    [Test]
    public void ProperCrossing_ShouldIntersect_NonStrict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(4, 4);
        var b1 = new Vector2Int(0, 4);
        var b2 = new Vector2Int(4, 0);

        Assert.IsTrue(Intersects(a1, a2, b1, b2, false));
    }

    [Test]
    public void TIntersection_ShouldNotIntersect_Strict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(4, 0);
        var b1 = new Vector2Int(2, 0);
        var b2 = new Vector2Int(2, 3);

        Assert.IsFalse(Intersects(a1, a2, b1, b2, true));
    }

    [Test]
    public void TIntersection_ShouldIntersect_NonStrict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(4, 0);
        var b1 = new Vector2Int(2, 0);
        var b2 = new Vector2Int(2, 3);

        Assert.IsTrue(Intersects(a1, a2, b1, b2, false));
    }

    [Test]
    public void EndpointTouch_ShouldNotIntersect_Strict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(2, 0);
        var b1 = new Vector2Int(2, 0);
        var b2 = new Vector2Int(4, 0);

        Assert.IsFalse(Intersects(a1, a2, b1, b2, true));
    }

    [Test]
    public void EndpointTouch_ShouldIntersect_NonStrict() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(2, 0);
        var b1 = new Vector2Int(2, 0);
        var b2 = new Vector2Int(4, 0);

        Assert.IsTrue(Intersects(a1, a2, b1, b2, false));
    }

    [Test]
    public void ParallelSeparated_ShouldNotIntersect() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(4, 0);
        var b1 = new Vector2Int(0, 2);
        var b2 = new Vector2Int(4, 2);

        Assert.IsFalse(Intersects(a1, a2, b1, b2, true));
        Assert.IsFalse(Intersects(a1, a2, b1, b2, false));
    }

    [Test]
    public void ClearlySeparate_ShouldNotIntersect() {
        var a1 = new Vector2Int(0, 0);
        var a2 = new Vector2Int(1, 1);
        var b1 = new Vector2Int(3, 3);
        var b2 = new Vector2Int(4, 4);

        Assert.IsFalse(Intersects(a1, a2, b1, b2, true));
        Assert.IsFalse(Intersects(a1, a2, b1, b2, false));
    }
}
