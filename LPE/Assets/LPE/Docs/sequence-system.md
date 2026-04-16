# Sequence System

Namespace: `LPE` (inferred — no explicit namespace on most sequence files)

---

## Overview

The sequence system is a coroutine-like framework for composing multi-step actions without Unity coroutines. It is frame-based: each call to `Next()` advances by one step (often one frame). The caller loop pattern is:

```csharp
sequence.Next();
while (!sequence.Done()) {
    // wait a frame
    sequence.Next();
}
sequence.OnComplete();
```

`ISequence` is the root interface. All compositions (queue, parallel, branch, etc.) implement it.

---

## `ISequence` (interface)

```csharp
void Next()          // Advance one step
bool Done()          // True when finished
void OnComplete()    // Cleanup — call exactly once after Done() returns true
```

**Contract**: Always call `Next()` at least once before checking `Done()`.

---

## Base Types

### `SingleStepSequence` (abstract)
Completes in exactly one `Next()` call.

```csharp
abstract void Call()         // The work to do
abstract void OnComplete()   // Cleanup

void ExecuteImmediate()      // Convenience: calls Next() then OnComplete()
```

### `LambdaSequence`
Wraps any `Action` as a single-step sequence.

```csharp
new LambdaSequence(() => DoSomething())
```

Not pooled. Use for one-off actions.

---

## Timing

### `PauseFramesSequence`
Wait for N frames.

```csharp
ISequence pause = PauseFramesSequence.Get(30);  // wait 30 frames
```

Pooled — always use `Get()`, not `new`.

---

## Composition

### `SequenceQueue` — Serial (one after another)

```csharp
var queue = SequenceQueue.Get();   // pooled factory
queue.AddSequence(seqA, playNextImmediate: false);
queue.AddSequence(seqB, playNextImmediate: false);
// seqA runs, then seqB
```

`playNextImmediate: true` means the next sequence starts in the same frame that the current one finishes (no 1-frame gap).

### `ParallelSequence` — Run simultaneously

```csharp
ISequence parallel = ParallelSequence.Get(seqA, seqB);
ISequence parallel3 = ParallelSequence.Get(seqA, seqB, seqC);
```

All sequences advance each frame. Done when ALL are done.

### `BranchSequence` (abstract) — Conditional branching

```csharp
class MyBranch : BranchSequence {
    protected override bool CheckBranch() => someCondition;
    // true  → runs trueBranch sequence
    // false → runs falseBranch sequence
}
```

The branch is evaluated once on the first `Next()`. After that it follows one path.

---

## Lifecycle Wrappers

### `ContexSequence` (abstract) — Setup/teardown around an inner sequence

```csharp
class MyContext : ContexSequence {
    protected override ISequence Inner { get; }   // set your inner sequence
    protected override void BeforeStart() { /* runs on first Next() */ }
    protected override void AfterEnd()   { /* runs when inner is done */ }
}
```

### `PassiveSequence` (abstract) — Side effects alongside another sequence

Runs in lockstep with an inner sequence. Done when inner is done.

```csharp
class MySfx : PassiveSequence {
    protected override ISequence Inner { get; }
    protected override void PrePassive()  { /* before each inner step */ }
    protected override void PostPassive() { /* after each inner step */ }
}
```

### `ReusedSequence` — Prevent double `OnComplete()`

Wraps a sequence so `OnComplete()` is only called once, even if the wrapper is referenced in multiple places.

```csharp
ISequence safe = someSeq.Reuse();   // extension method
```

Pooled.

---

## Extension Methods (`SequenceUtility`)

```csharp
ISequence a, b;

a.WithParallel(b)                          // → ParallelSequence(a, b)
a.Reuse()                                  // → ReusedSequence(a)
a.Then(b, playNextImmediate: false)        // → SequenceQueue with a then b
```

---

## Usage Pattern

```csharp
// Build
var seq = SequenceQueue.Get();
seq.AddSequence(new MoveSequence(target), false);
seq.AddSequence(PauseFramesSequence.Get(10), false);
seq.AddSequence(new AttackSequence(), false);

// Drive (typically in Update())
seq.Next();
if (seq.Done()) {
    seq.OnComplete();
    seq = null;
}
```

Or with parallel:
```csharp
var seq = ParallelSequence.Get(moveAnim, moveSfx);
```

---

## Notes

- Most composition types are **pooled** — use static `Get()` factories.
- `LambdaSequence` and custom subclasses are not pooled.
- `SequenceQueue` is the most commonly used composition; use `playNextImmediate: true` when you don't want a 1-frame delay between steps.
- `OnComplete()` must be called by the driving code after `Done()` returns true — the sequence does not self-cleanup.
