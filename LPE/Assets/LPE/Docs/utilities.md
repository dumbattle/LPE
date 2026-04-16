# Utilities & Helpers

Namespace: `LPE`

---

## `LoopSafety` (struct)

Guards against infinite loops in simulation code.

```csharp
var ls = new LoopSafety(maxIterations: 1000);

// Configure behavior on limit hit (call before loop):
ls.SetException("Loop exceeded limit in MyMethod");   // throw exception
ls.SetLog("Loop exceeded limit in MyMethod");          // log warning only

while (someCondition) {
    if (!ls.Next()) break;   // returns false when limit reached
    // ... loop body ...
}

bool hitLimit = ls.hitLimit;
int  i        = ls.i;         // current iteration count
```

---

## `ProfileHelper` (static)

Lightweight Unity Profiler integration using named `CustomSampler` instances.

```csharp
// Manual:
ProfileHelper.Start("MySystem.Update");
DoWork();
ProfileHelper.End("MySystem.Update");

// Scoped (auto-ends on Dispose):
using (ProfileHelper.Sample("MySystem.Update")) {
    DoWork();
}
```

Samplers are lazily created and cached by name. Visible in the Unity Profiler window.

---

## `RandomUtility` (static)

Uniform random selection from a small set.

```csharp
T result = RandomUtility.Uniform(a, b);         // one of 2
T result = RandomUtility.Uniform(a, b, c);      // one of 3
T result = RandomUtility.Uniform(a, b, c, d);   // one of 4
```

---

## `Utility` (static)

```csharp
Utility.Switch(ref a, ref b);   // Swap two values of any type
```

---

## `Singleton<T>` (static)

Simple lazy singleton.

```csharp
T instance = Singleton<T>.Get();
```

`T` must have a parameterless constructor. Instance is created on first access and cached.

---

## `LazyLoadResource<T>` (class)

Lazy Unity `Resources.Load<T>` wrapper. Loads on first use.

```csharp
var res = new LazyLoadResource<Texture2D>("path/in/Resources");
Texture2D tex = res;   // implicit conversion triggers load
```

---

## `LPEButtonBehaviour` (MonoBehaviour)

UI button with single-frame event detection. Attach to a `GameObject` with a `Collider2D` or UI element.

```csharp
// Inspector / setup:
void SetClickListener(Action callback)    // Fires one frame after click
void SetDownListener(Action callback)     // Fires on press-down frame

// Poll directly:
bool Clicked { get; }   // True for exactly 1 frame after click
bool Down    { get; }   // True for exactly 1 frame on press

// Implements IPointerDownHandler, IPointerClickHandler
// Resets flags in LateUpdate()
```

---

## `LpeExtensions` (static, extension methods)

### Collections
```csharp
bool IsInRange<T>(this T[] arr, int i)
bool IsInRange<T>(this List<T> arr, int i)
string ContentString<T>(this IEnumerable<T> list, Func<T, string> formatter)
```

### Numeric
```csharp
float Max(this float f, float maxValue)   // Clamp upper bound
int   Max(this int f,   int maxValue)
int   Min(this int f,   int minValue)
```

### Vector
```csharp
Vector3 SetZ(this Vector2 v, float z)   // Create Vector3 from Vector2 + z
```

### Bounds / Rect
```csharp
bool IsWithinBounds(this Vector2Int p, Vector2Int min, Vector2Int max, bool maxInclusive)
bool IntersectsXY(this BoundsInt a, BoundsInt b, bool allowTangent)
Bounds ToBounds(this BoundsInt boundsInt)
bool Contains(this BoundsInt bounds, Vector2 point)
bool Contains(this BoundsInt bounds, Vector2Int point)
```

---

## `Direction4` (class)

Cardinal direction utility.

```csharp
Direction4.up / .down / .left / .right   // Static instances

Vector2Int ToVector2Int()
static Direction4 FromVector2Int(Vector2Int v)
static Direction4 Reverse(Direction4 d)

// Enum interop (Direction4.Enum: up, right, down, left, none)
static Direction4 FromEnum(Direction4.Enum e)
// Implicit conversions to/from Direction4.Enum
```
