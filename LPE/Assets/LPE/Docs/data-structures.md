# Core Data Structures

Namespace: `LPE`

---

## `EntityID` (struct)

Unique identifier for game entities.

```csharp
new EntityID(int id)

bool IsValid()              // false if id == -1
static EntityID INVALID     // sentinel value

// Implements IEquatable<EntityID>
// Operator overloads: ==, !=
```

---

## `FreeList<T>` (class)

An array-backed list where removed slots are marked free and reused by subsequent adds. Indices are stable until an item is removed.

```csharp
int  Add(T item)            // Returns the index
void RemoveAt(int index)    // Marks slot as free
void Clear()
T    this[int index]        // Indexed access
```

**When to use**: When you need stable indices to items and frequent add/remove cycles.

---

## `FreeLinkedList<T>` (class)

A doubly linked list with pooled node slots (removed nodes are reused).

```csharp
Node AddFirst(T value)
Node AddLast(T value)

// Node struct:
T     value
Node? Next
Node? Previous

// Iteration
foreach (T item in list) { ... }
```

**When to use**: Frequent insert/remove at arbitrary positions with iteration.

---

## `ObjectPool<T>` (class)

Generic object pool to avoid GC allocations.

```csharp
new ObjectPool<T>(Func<T> factory)

T   Get()                   // Get or create instance
void Return(T item)          // Return to pool (detects double-returns)

// Scoped usage (auto-returns on Dispose):
using var scoped = pool.GetScoped();
T item = scoped.item;

// Diagnostics
int createdCount
int availableCount
int inUseCount
```

Warns in the console if the pool grows beyond an internal threshold (potential leak).

---

## `PriorityQueue<T>` (class)

Max-priority binary heap. Higher priority = dequeued first.

```csharp
void  Add(T value, float priority)
T     Get()                  // Remove and return highest priority item
T     Peek()                 // Highest priority item without removing
float PeekPriority()
void  Clear()

bool  isEmpty { get; }
int   size    { get; }
```

---

## `ArbitraryDataStorage<TKey>` (class)

Type-safe heterogeneous key→value store. Multiple types can be stored under the same key.

```csharp
void Set<T>(TKey key, T value)
bool TryGet<T>(TKey key, out T value)     // False if key missing or type mismatch
T    GetOrDefault<T>(TKey key, T defaultVal)
void Remove(TKey key)
void Clear()
```

**When to use**: Attaching arbitrary data to entities without a fixed component structure. Similar to a component bag.

---

## `Direction4` (class)

Cardinal direction abstraction.

```csharp
// Static instances
Direction4.up
Direction4.down
Direction4.left
Direction4.right

// Conversion
Vector2Int ToVector2Int()
static Direction4 FromVector2Int(Vector2Int v)
static Direction4 Reverse(Direction4 d)

// Enum interop
static Direction4 FromEnum(Direction4.Enum e)
// implicit conversion to/from Direction4.Enum

// Inner enum
enum Enum { up, right, down, left, none }
```

---

## Notes

- `FreeList<T>` indices should not be stored long-term unless you know the item hasn't been removed.
- `ObjectPool<T>` expects the pooled type to be reusable/resettable — clear state in the type before returning.
- `PriorityQueue<T>` is a **max** heap (highest priority out first). For min-heap behavior, negate priorities.
- `ArbitraryDataStorage<TKey>` uses internal pooling for its generic entries — low allocation overhead.
