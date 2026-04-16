# Steering Behaviors

Namespace: `LPE.Steering`

---

## Overview

A small set of steering calculations for autonomous movement. All methods are pure functions that return a `Vector2` direction or force — they don't move anything directly.

---

## `ISteerAgent` (interface)

Your agent class must implement this:

```csharp
Vector2 position  { get; }
Vector2 direction { get; }   // current facing/velocity direction (normalized)
float   radius    { get; }   // agent radius for separation
```

---

## `Steering` (static class)

All methods are generic: `T` must implement `ISteerAgent`.

### `Separate<T>(T agent, List<T> nearby) → Vector2`
Pure separation force. Pushes the agent away from all nearby agents, weighted by inverse-square distance. Result should be added to your movement vector.

### `Basic<T>(Vector2 target, T agent, List<T> nearby) → Vector2`
Seek the target position + separation from nearby agents.

```csharp
// Returns normalized direction toward target, with separation applied
Vector2 dir = Steering.Basic(targetPos, myAgent, nearbyAgents);
```

### `Better<T>(Vector2 target, T agent, List<T> nearby, float momentum) → Vector2`
Seek + separation + **momentum smoothing**. Blends the desired direction with the agent's current direction based on `momentum` (0 = no smoothing, 1 = no turning).

```csharp
Vector2 dir = Steering.Better(targetPos, myAgent, nearbyAgents, momentum: 0.3f);
```

- Higher `momentum` → smoother turning (more inertia)
- Also applies a small random jitter to prevent agents from getting stuck in symmetric configurations

### `Seperation<T>(T agent, List<T> nearby, float sepScale) → Vector2`
Internal separation logic, exposed for custom combinations.

- `sepScale` — multiplier on the separation distance threshold
- Returns sum of inverse-square repulsion vectors from all nearby agents

---

## Usage Pattern

```csharp
// In Update():
var nearby = new List<MyAgent>();
partition.QueryItems(agentAABB, nearby);
nearby.Remove(this);  // exclude self

Vector2 moveDir = Steering.Better(targetPosition, this, nearby, momentum: 0.25f);
transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
```

---

## Notes

- All methods return a direction/force vector — **you** apply it to position.
- `Basic` and `Better` normalize the final result. `Separate` and `Seperation` return unnormalized forces — scale them yourself.
- The separation uses **two scales** internally (close-range and medium-range) to handle both tight clusters and loose groups.
- Pair with `Grid2D<T>` or `GridHierarchy<T>` for the nearby query to avoid O(n²) checks.
