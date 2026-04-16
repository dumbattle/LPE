# Thalamic AI System

Namespace: `LPE.AI.LPE`

---

## Overview

The **Lensed Perception Engine** (LPE) AI is a 4-stage decision system inspired by thalamic filtering in the brain. Each stage is a clean, separate concern:

| Stage | Class | What it does |
|-------|-------|--------------|
| 1. Attention | `AttentionContext` | Filter the world view — mark elements as focus / peripheral / ignored |
| 2. Desires | `DesireContext` | Emit goals with a strength level |
| 3. Action Proposals | `ActionProposalContext` | Propose candidate actions and declare which desires they satisfy |
| 4. Selection | `SelectionContext` | Score proposals against desires; pick the best |

The caller invokes `Run(view)` and receives the winning `ActionProposal`, then executes it.

---

## Main Class: `LensedPerceptionEngine<TView>`

Abstract. `TView` is your application-specific world snapshot (e.g. a struct or class holding entity collections, scores, etc.).

### Methods to implement (abstract)

```csharp
void Attention(TView view, AttentionContext ctx)
void EmitDesires(TView view, DesireContext ctx)
void ProposeActions(TView view, ActionProposalContext ctx)
```

### Methods to call

```csharp
ActionProposal Run(TView view)          // Run all 4 stages, return best proposal
void DebugRun(TView view)               // Same but with debug output
```

### Selection algorithm

`SelectBest()` iterates desire strengths from `primary` down to `idle`. The first strength level that has at least one proposal with a non-zero score wins. Within that level, the highest-scoring proposal is returned. If no proposal satisfies any desire, the proposal with the highest idle score is returned.

---

## World View

### `ViewElement<T>`
Wraps a data item with an `AttentionLevel`. Created and managed by `ViewCollection<T>`.

```csharp
// Internal fields:
T data
AttentionLevel attentionLevel  // default = peripheral
```

### `ViewCollection<T>`
A list of `ViewElement<T>` for one category of world data (e.g. all enemies).

```csharp
collection.SetElements(List<T> items)   // Reset with new data each frame

// Enumerators that skip 'ignored' elements:
collection.EnumerateData()              // yields T
collection.EnumerateFull()              // yields (ViewElement<T>, T)
```

### `AttentionLevel` (enum)
```csharp
focus      = 2   // Actively attended to
peripheral = 1   // Default — visible but not focused
ignored    = 0   // Filtered out; skipped by all enumerators
```

---

## Stage 1: Attention

```csharp
void Attention(TView view, AttentionContext ctx)
{
    // Example: focus on entities with high health
    foreach (var (element, entity) in view.entities.EnumerateFull())
    {
        if (entity.health > 50)
            ctx.SetAttentionLevel(element, AttentionLevel.focus);
        else
            ctx.SetAttentionLevel(element, AttentionLevel.ignored);
    }
    // For non-collection ViewElements:
    ctx.SetAttentionLevel(view.opponentScore, AttentionLevel.focus);
}
```

`AttentionContext` methods:
```csharp
void SetAttentionLevel<T>(ViewElement<T> element, AttentionLevel level)
void SetAttentionLevel<T>(T data, AttentionLevel level)  // matches by reference
```

---

## Stage 2: Desires

```csharp
void EmitDesires(TView view, DesireContext ctx)
{
    // Parameterless desire — "I want to idle"
    ctx.EmitDesire<IdleActionSpec>(DesireStrength.idle, source: "default");

    // Parameterized desire — "I want to damage entity X"
    foreach (var entity in ctx.EnumerateData(view.entities))
        ctx.EmitDesire<DamageActionSpec, EntityID>(entity.id, DesireStrength.primary, "low-hp");
}
```

`DesireContext` methods:
```csharp
bool TryGetData<T>(ViewElement<T> element, out T result)  // returns false if ignored
ViewCollection<T>.DataEnumerable EnumerateData<T>(ViewCollection<T> collection)
void EmitDesire<TSpec>(DesireStrength strength, string source)
void EmitDesire<TSpec, TParam>(TParam param, DesireStrength strength, string source)
```

### `DesireStrength` (enum)
```csharp
primary    = 4   // Must-do goal
secondary  = 3
incidental = 2
idle       = 1   // Fallback
none       = 0
```

---

## Stage 3: Action Proposals

```csharp
void ProposeActions(TView view, ActionProposalContext ctx)
{
    // Parameterless action
    ctx.BeginProposal(MyCallback.Idle)
       .ReportSatisfies<IdleActionSpec>();

    // Parameterized action
    foreach (var entity in ctx.EnumerateData(view.enemies))
    {
        ctx.BeginProposal(MyCallback.Attack, entity.id)
           .ReportSatisfies<DamageActionSpec, EntityID>(entity.id);
    }
}
```

`ActionProposalContext` methods:
```csharp
ActionProposalBuilder<TRep> BeginProposal<TRep>(TRep representative)
ActionProposalBuilder<TRep, TParam> BeginProposal<TRep, TParam>(TRep rep, TParam param)
```

### `ActionProposalBuilder` (struct, fluent)
```csharp
.ReportSatisfies<TSpec>()                  // Parameterless spec
.ReportSatisfies<TSpec, TParam>(TParam p)  // Parameterized spec
```

Each `BeginProposal` call creates one `ActionProposal` stored internally. The builder is a struct; no need to store it. Just chain `.ReportSatisfies` calls.

---

## Stage 4: Selection (automatic)

`SelectBest()` is called internally by `Run()`. The returned `ActionProposal` carries the representative and parameter.

### Reading the result

```csharp
ActionProposal result = ai.Run(view);

if (result.TryGetRepresentative<MyCallback>(out var callback))
    // use callback

if (result.TryGetParam<EntityID>(out var target))
    // use target
```

---

## Action Specs

Action specs are the vocabulary of desires and proposals. They are types, not instances.

### `ActionSpec` (abstract, parameterless)
```csharp
class IdleActionSpec : ActionSpec { }
```

Scoring override (optional):
```csharp
float ScoreFunction(int numProposals, int numDesires)  // default: 1.0
```

### `ActionSpec<TParam>` (abstract, parameterized)
```csharp
class DamageActionSpec : ActionSpec<EntityID> { }
```

Scoring override (optional):
```csharp
float ScoreFunction(IReadOnlyList<TParam> proposals, IReadOnlyList<TParam> desires)
float Score(TParam proposal, TParam desire)  // default: 1 if equal
```

The default scoring counts matching (proposal, desire) pairs. Override `Score()` to use fuzzy/partial matching.

---

## Full Example Skeleton

```csharp
// 1. Define world view
class MyView {
    public ViewCollection<Enemy> enemies = new();
    public ViewElement<int> threatLevel = new();
}

// 2. Define action specs
class AttackSpec : ActionSpec<Enemy> { }
class FleeSpec : ActionSpec { }

// 3. Implement the engine
class MyAI : LensedPerceptionEngine<MyView> {
    protected override void Attention(MyView view, AttentionContext ctx) {
        foreach (var (elem, enemy) in view.enemies.EnumerateFull())
            ctx.SetAttentionLevel(elem, enemy.isAlive ? AttentionLevel.focus : AttentionLevel.ignored);
    }

    protected override void EmitDesires(MyView view, DesireContext ctx) {
        foreach (var enemy in ctx.EnumerateData(view.enemies))
            ctx.EmitDesire<AttackSpec, Enemy>(enemy, DesireStrength.primary, "combat");
        if (view.threatLevel.data > 80)
            ctx.EmitDesire<FleeSpec>(DesireStrength.primary, "flee");
    }

    protected override void ProposeActions(MyView view, ActionProposalContext ctx) {
        foreach (var enemy in ctx.EnumerateData(view.enemies))
            ctx.BeginProposal(Action.Attack, enemy).ReportSatisfies<AttackSpec, Enemy>(enemy);
        ctx.BeginProposal(Action.Flee).ReportSatisfies<FleeSpec>();
    }
}

// 4. Each frame
var result = myAI.Run(view);
// Execute result...
```

---

## Notes

- All internal collections (`DesireComposition`, `ActionProposalComposition`, etc.) are **pooled** — no allocations per frame.
- `ViewCollection<T>.SetElements()` must be called each tick to refresh world data.
- The `source` string in `EmitDesire` is for debugging only; it does not affect scoring.
- Attention levels persist across proposals/desires in the same tick — set them in `Attention()` once.
