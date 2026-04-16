# Geometry & Math Utilities

Namespace: `LPE.Math`

Class: `Geometry` (static, partial — split across multiple files)

---

## Ray & Segment Tests

```csharp
// Does a ray from origin 'o' in direction 'd' intersect segment [a, b]?
bool IsRaySegment(Vector2 o, Vector2 d, Vector2 a, Vector2 b)

// Is point 'p' on segment [a, b]?
bool OnSegment(Vector2 p, Vector2 a, Vector2 b)

// Rasterize a line from 's' to 'e' into grid cells
// mode: controls inclusion rules (exact meaning depends on implementation)
List<Vector2Int> GetLine(Vector2 s, Vector2 e, List<Vector2Int> results, int mode)
```

---

## Circle Casting

```csharp
// Swept circle collision: how far can circle 1 move in 'dir' before hitting circle 2?
// Returns distance, or float.MaxValue if no collision
float CircleCast_Circle(Vector2 c1, float r1, Vector2 c2, float r2, Vector2 dir)
```

Uses a quadratic solver internally.

---

## Quadratic Solver

```csharp
// Solve ax² + bx + c = 0
// Returns (root1, root2) — may be NaN if no real roots
(double, double) SolveQuadratic(double a, double b, double c)
```

---

## Transformations

```csharp
// Rotate a single point around origin by 'degrees'
Vector2 Rotate(Vector2 point, float degrees)

// Rotate array of points in-place
void Rotate(Vector2[] points, float degrees)
```

---

## Segment Utility

```csharp
// Shorten segment [a, b] by 'amnt' from each end
(Vector2 a, Vector2 b) ShortenSegment(Vector2 a, Vector2 b, float amnt)
```

---

## AABB Operations

```csharp
// AABB of a circle
(Vector2 min, Vector2 max) CircleAABB(Vector2 pos, float r)

// Expand an AABB to include all points swept in direction 'dir'
// (useful for motion AABBs / broad-phase sweep)
(Vector2 min, Vector2 max) ExpandAABB((Vector2 min, Vector2 max) src, Vector2 dir)
```

---

## Intersection Tests

```csharp
// Segment vs segment
bool IsIntersecting(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)

// Circle vs circle
// touch: if true, tangent counts as intersecting
bool IsIntersecting(Vector2 c1, float r1, Vector2 c2, float r2, bool touch)

// AABB vs AABB
bool AABBIntersection(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)

// Ray vs AABB (optimized with precomputed reciprocal direction)
bool RayAABBIntersection((Vector2 min, Vector2 max) aabb, Vector2 rayStart, Vector2 invRayDir)
```

For `RayAABBIntersection`, compute `invRayDir` once:
```csharp
Vector2 invDir = new Vector2(1f / dir.x, 1f / dir.y);
bool hit = Geometry.RayAABBIntersection(aabb, rayStart, invDir);
```

---

## Notes

- All methods are static — call as `Geometry.MethodName(...)`.
- `GetLine` fills the provided `List<Vector2Int>` and returns it (no allocation if you pass your own list).
- AABB tuples are `(Vector2 min, Vector2 max)` — not Unity `Bounds`.
- `SolveQuadratic` returns `(NaN, NaN)` when the discriminant is negative (no real roots).
- `RayAABBIntersection` handles the `invRayDir` division-by-zero edge cases (axis-aligned rays) correctly.
