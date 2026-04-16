# Spatial Partitioning

Namespace: `LPE.SpacePartition`

---

## Overview

Two spatial data structures for efficient broad-phase queries (e.g. "which objects are near this position?"):

| Class | Strategy | Best for |
|-------|----------|----------|
| `Grid2D<T>` | Uniform grid | Many small, similarly-sized items |
| `GridHierarchy<T>` | Multi-level grid | Mixed-size items or large spatial range |

Both extend `Partition2D<T>` and share the same API.

---

## Abstract Base: `Partition2D<T>`

```csharp
void Add(T item, Vector2 min, Vector2 max)
void Remove(T item)
void UpdateItem(T item, Vector2 min, Vector2 max)
void QueryItems(Vector2 min, Vector2 max, HashSet<T> result)
void QueryItems(Vector2 min, Vector2 max, List<T> result)   // virtual, defaults to HashSet then copy
void CleanUp()                                              // virtual no-op by default

// Overloads accepting tuple AABB:
void Add(T item, (Vector2 min, Vector2 max) aabb)
void UpdateItem(T item, (Vector2 min, Vector2 max) aabb)
void QueryItems((Vector2 min, Vector2 max) aabb, HashSet<T> result)
```

---

## `Grid2D<T>` (uniform grid)

**Constructor:**
```csharp
new Grid2D<T>(Vector2 min, Vector2 max, Vector2Int resolution)
// min/max: world-space bounds of the entire grid
// resolution: number of cells in X and Y (e.g. new Vector2Int(20, 20))
```

**Behavior:**
- Each item's AABB is mapped to the cells it overlaps.
- `Remove()` and `UpdateItem()` use the stored AABB from `Add()` — no need to pass it again for removes.
- Queries return all unique items whose cells overlap the query AABB. Items can appear in multiple cells but are de-duplicated in the HashSet output.

**Usage:**
```csharp
var grid = new Grid2D<Enemy>(new Vector2(-50, -50), new Vector2(50, 50), new Vector2Int(20, 20));

// Add
grid.Add(enemy, enemy.bounds.min, enemy.bounds.max);

// Query nearby enemies
var nearby = new HashSet<Enemy>();
grid.QueryItems(queryMin, queryMax, nearby);

// Update position
grid.UpdateItem(enemy, newMin, newMax);

// Remove
grid.Remove(enemy);
```

---

## `GridHierarchy<T>` (multi-level grid)

A coarse/fine two-level structure. Internally partitions space at multiple scales, placing each item at the appropriate level based on its size.

```csharp
new GridHierarchy<T>(/* parameters vary — check constructor */)
```

API is the same as `Grid2D<T>` (inherits `Partition2D<T>`). Use when items vary widely in size or when the spatial range is very large relative to item sizes.

---

## Tips

- **`HashSet` vs `List` output**: Prefer `HashSet` to avoid manual de-duplication. Use `List` only if you need ordered or indexed results.
- **`UpdateItem`** is cheaper than `Remove` + `Add` because it only touches changed cells.
- **`CleanUp()`**: Call periodically if items are frequently removed but you suspect stale data.
- Grid2D does not auto-resize — choose bounds and resolution at construction time.
- Items outside the grid bounds are clamped to the nearest cell.
