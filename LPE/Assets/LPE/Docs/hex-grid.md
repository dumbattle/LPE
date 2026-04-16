# Hexagonal Grid System

Namespace: `LPE.Hex`

---

## Overview

Cube-coordinate hexagonal grid math. Three layers:
1. `HexCoord` — math on individual hex positions
2. `HexGrid` / `HexRectGrid` — enumeration helpers for circular and rectangular grids
3. `HexMap<T>` — data storage indexed by hex coordinate

---

## `HexCoord` (struct)

Cube coordinates: `(x, y, z)` where `z = -x - y` always holds.

```csharp
// Construction
new HexCoord(int x, int y)             // z is inferred
(int x, int y)                         // implicit tuple conversion
HexCoord.Round(float x, float y)       // round float cube coords to nearest hex
```

### Properties
```csharp
int x { get; }
int y { get; }
int z { get; }           // always -x - y
int Arm { get; }         // which of the 6 directions from origin (0–5)
```

### Distance
```csharp
int  ManhattanDist(HexCoord other)   // hex-space distance (max of abs differences)
int  ManhattanDist()                 // distance from origin
float SqrDist(HexCoord other)        // Euclidean squared
float Dist(HexCoord other)           // Euclidean
```

### Neighbors
```csharp
HexCoord[] GetNeighbors()            // Returns the 6 adjacent hexes
static HexCoord[] Directions         // The 6 unit direction offsets
```

### World-space conversion
```csharp
Vector2 ToCartesian(Orientation orientation, float hexRadius)
static HexCoord FromCartesian(Vector2 pos, float hexRadius, Orientation orientation)
```

### Operators
```csharp
HexCoord + HexCoord
HexCoord - HexCoord
HexCoord * int             // scale
==  !=
implicit (int x, int y)    // tuple → HexCoord
```

### Constants
```csharp
static float ROOT3 = 1.7320508f   // √3, useful for hex-to-world math
```

---

## `Orientation` (enum)

```csharp
vertical    // "pointy-top" hexes
horizontal  // "flat-top" hexes
```

---

## `HexGrid` (struct) — Circular grid

Enumerates all hexes within a given radius of a center.

```csharp
new HexGrid(int radius)
new HexGrid(int radius, HexCoord center)

int radius
int area           // Total hex count
int circumference  // Count on the outermost ring
HexCoord center
```

```csharp
// Enumerate all hexes
foreach (HexCoord h in grid) { ... }

// Checks
bool IsInRange(HexCoord hex)

// Utilities
HexCoord RandomCoord()
void ParallelForeach(Action<HexCoord> action)   // Multi-threaded iteration
```

---

## `HexRectGrid` (struct) — Rectangular grid

Enumerates hexes arranged in a rectangle.

```csharp
float width
float height
float spacing
Orientation orientation

Vector2 CenterOffset()         // Offset to center the grid at origin
IEnumerator<HexCoord> GetEnumerator()
```

---

## `HexMap<T>` (class) — Data storage

Stores values at hex positions for a fixed-radius circular grid.

```csharp
new HexMap<T>(int radius)

T this[HexCoord h] { get; set; }   // Indexed access
int Size            { get; }        // Total cells
int Radius          { get; }

bool IsInRange(HexCoord hex)
IEnumerable<T>                      // Iterate all values
```

Uses an optimized internal index function — O(1) access without a dictionary.

---

## Usage Example

```csharp
var map = new HexMap<TileData>(5);   // 5-hex radius

var grid = new HexGrid(5);
foreach (HexCoord h in grid) {
    map[h] = new TileData();

    // Get world position
    Vector2 worldPos = h.ToCartesian(Orientation.vertical, hexRadius: 1f);

    // Get neighbors
    foreach (HexCoord neighbor in h.GetNeighbors()) {
        if (map.IsInRange(neighbor))
            Process(map[neighbor]);
    }
}

// Find hex under mouse
HexCoord clicked = HexCoord.FromCartesian(mouseWorldPos, 1f, Orientation.vertical);
```

---

## Notes

- `HexCoord` is a struct — cheap to copy.
- The `z` component of cube coordinates is always redundant (`z = -x - y`) but used internally for distance math.
- `ManhattanDist` in hex space equals `max(|dx|, |dy|, |dz|)` (or equivalently `(|dx|+|dy|+|dz|)/2`).
- `HexMap<T>` only supports fixed-radius circular grids. For arbitrary shapes, use a `Dictionary<HexCoord, T>`.
