# Collision & Shapes (2D)

Namespace: `LPE.Shape`

---

## Overview

A 2D collision system based on the **Separating Axis Theorem (SAT)**. Three shape types are supported: circles, convex polygons, and rectangles. All shapes share a common transform model (position, rotation, scale) and a unified collision API.

---

## Shape Base: `Shape2D` (abstract)

All shapes extend `Shape2D`.

### Transform
```csharp
Vector2 position { get; set; }
float rotation   { get; set; }   // degrees, normalized to 0–360
float scale      { get; set; }
```

Setting any transform property automatically calls `Update()` to recompute cached vertices/axes.

### Overridable in subclasses
```csharp
abstract Vector2 Project(Vector2 line)               // SAT projection onto axis
abstract (Vector2 min, Vector2 max) AABB()           // Bounding box
abstract void Update()                               // Recompute cached data
virtual  Vector2[] Vertices()                        // Raw vertices
virtual  Vector2[] CollisionAxes()                   // SAT test axes
virtual  void OnDrawGizmos()                         // Debug visualization
```

### Collision Methods (on any Shape2D)
```csharp
bool CheckCollision(Shape2D other)
Vector2 CheckCollisionWithCorrection(Shape2D other)          // Returns MTV (push-out vector)
float   CheckCollisionWithCorrection(Shape2D other, Vector2 dir)  // MTV magnitude along dir
float   ShapeCast(Shape2D target, Vector2 dir)               // Sweep/cast distance (extension)
```

### Multi-shape correction
```csharp
// Extension method
Vector2 GetMultiCorrection(this Shape2D src, List<Shape2D> colliding, int phase1, int phase2)
```
Returns a combined separation vector against multiple overlapping shapes. `phase1`/`phase2` control iteration counts.

---

## Shape Types

### `CircleShape`
```csharp
new CircleShape(float radius)
float radius { get; set; }   // Setting this calls Update()
```

AABB and projection are computed analytically (no vertices needed).

### `ConvexPolygonShape`
```csharp
new ConvexPolygonShape(params Vector2[] vertices)
new ConvexPolygonShape(List<Vector2> vertices)
ConvexPolygonShape.Regular(int n)   // Regular n-gon (unit radius, centered at origin)
```

Vertices, axes, and AABB are cached and re-computed when position/rotation/scale change.

### `RectangleShape`
```csharp
new RectangleShape(float width, float height)
```
Inherits from `ConvexPolygonShape`. Convenience constructor for axis-aligned rectangles.

---

## Shape Casting

### `CastedShape`
A wrapper for sweep/cast queries. Represents the swept volume of a shape moving in a direction.

```csharp
var cast = new CastedShape();
cast.Set(sourceShape, direction);
```

Use with `ShapeCast()`:
```csharp
float dist = sourceShape.ShapeCast(targetShape, direction);
// dist: how far sourceShape can move in direction before hitting targetShape
// Returns float.MaxValue if no hit
```

---

## Static Collision API: `Shape2DCollision`

All collision logic is centralized here.

```csharp
bool    CheckCollision(Shape2D s1, Shape2D s2)
Vector2 CheckCollisionWithCorrection(Shape2D s1, Shape2D s2)     // MTV vector
float   CheckCollisionWithCorrection(Shape2D src, Shape2D target, Vector2 dir)  // MTV scalar
float   ShapeCast(this Shape2D cast, Shape2D target, Vector2 dir)
Vector2 GetMultiCorrection(this Shape2D src, List<Shape2D> colliding, int p1, int p2)
```

`Shape2D`'s instance methods delegate to this class.

---

## Algorithm Notes

- Uses **SAT** (Separating Axis Theorem) for polygon-polygon tests.
- Circles use analytic tests (distance vs. sum of radii) — faster than SAT.
- Circle vs. polygon uses a hybrid approach (point projection + SAT).
- `CheckCollisionWithCorrection` returns the **Minimum Translation Vector (MTV)** — the smallest displacement to separate overlapping shapes.
- `ShapeCast` uses binary-search-like projection to find the exact collision distance.

---

## Static Helper

```csharp
// On Shape2D (static)
float Projection(Vector2 point, Vector2 line)   // Project a point onto an axis
```

---

## Usage Example

```csharp
var player = new CircleShape(0.5f);
var wall   = new RectangleShape(2f, 0.2f);

player.position = new Vector2(1, 0);
wall.position   = new Vector2(1, 0.3f);

if (player.CheckCollision(wall)) {
    Vector2 correction = player.CheckCollisionWithCorrection(wall);
    player.position += correction;   // push player out of wall
}
```

---

## Notes

- Always call `Update()` (or set a transform property) before collision tests if you've moved a shape manually.
- `AABB()` returns `(Vector2 min, Vector2 max)` — not a Unity `Bounds`.
- `ShapeCast` returns `float.MaxValue` when there is no intersection along the cast direction.
- The `OnDrawGizmos()` methods on shapes can be called from a MonoBehaviour's `OnDrawGizmos()` for debug visualization.
