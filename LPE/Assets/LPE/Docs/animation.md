# Animation System

Namespace: `LPE.Animation`

---

## Overview

A generic, sprite-based animation system built around three concerns:
1. **Definitions** — ScriptableObjects that describe frames, durations, and tags.
2. **Player** — Generic `AnimationPlayer<TTarget>` that advances frames over time.
3. **State machines** — Directional animation lookup tables that map (state, direction) → definition.

The system is generic: `TTarget` is typically `SpriteRenderer`, but can be anything.

---

## Animation Definitions

### `IAnimationDefinition<TTarget>` (interface)
The core contract for any animation.

```csharp
int numFrames { get; }
int GetFrameDuration(int frame)                  // Duration of frame in ticks
AnimationFrameTags GetFrameTags(int frame)       // Tags for frame
void DrawFrame(int frame, TTarget target)        // Apply frame to target
```

### `AnimationFrameTags` (enum, flags)
```csharp
step    = 1   // Foot-step frame (e.g. for SFX)
primary = 2   // Key/impact frame
```

### `AnimationFrameDefinition` (serializable class)
Used inside ScriptableObjects to define a single frame.

```csharp
Sprite sprite
int duration              // How many ticks this frame lasts
AnimationFrameTags tags
```

### `LpeAnimationDefinitionSO` (ScriptableObject)
Implements `IAnimationDefinition<SpriteRenderer>`.

```csharp
AnimationFrameDefinition[] frames
int TotalDuration()       // Sum of all frame durations
```

Create as a Unity asset. Assign frames in the Inspector.

---

## Animation Player

### `AnimationPlayer<TTarget>` (class)
Manages playback state. Not a MonoBehaviour — embed in your component.

```csharp
void Set(TTarget target, IAnimationDefinition<TTarget> anim, bool doLoop)
void Update()             // Advance by one tick; call in MonoBehaviour.Update()
bool IsDone()             // True when non-looping animation finished
AnimationFrameTags GetTags(bool firstFrameOnly = false)  // Current frame's tags
```

**Usage:**
```csharp
// In your MonoBehaviour
AnimationPlayer<SpriteRenderer> player = new();

void StartWalkAnim() {
    player.Set(spriteRenderer, walkAnimSO, doLoop: true);
}

void Update() {
    player.Update();
    if (player.GetTags().HasFlag(AnimationFrameTags.step))
        PlayFootstep();
}
```

### `AnimationPlayerBehaviour` (MonoBehaviour)
Pre-built MonoBehaviour wrapper around `AnimationPlayer<SpriteRenderer>`.

```csharp
// Fields (Inspector)
SpriteRenderer sr
LpeAnimationDefinitionSO anim
bool autoStart

// Methods
void SetAnimation(IAnimationDefinition<SpriteRenderer> anim)
void StartAnimation(IAnimationDefinition<SpriteRenderer> anim, bool loop)
```

`Update()` is called automatically. Use `SetAnimation` to swap mid-play.

---

## Directional Animations

For characters that face different directions.

### `DirectionalAnimationSO` (abstract ScriptableObject)
```csharp
IAnimationDefinition<SpriteRenderer> GetAnim(Vector2 direction)
```

Subclasses implement how direction maps to a specific clip.

### `DirectionalAnimationSO_2Dir` (ScriptableObject)
Two-direction variant (e.g. left/right, or up/down).

```csharp
LpeAnimationDefinitionSO right   // Inspector field
LpeAnimationDefinitionSO left    // Inspector field
```

`GetAnim(direction)` returns `right` or `left` based on the sign of the direction vector's relevant axis.

---

## Directional Animation State Machines

For characters with multiple states (idle, walk, attack) each with directional variants.

### `IDirectionalAnimationStateMachine<TState>` (interface)
```csharp
IAnimationDefinition<SpriteRenderer> GetAnim(TState state, Vector2 direction)
bool TryGetAnim(TState state, Vector2 direction, out IAnimationDefinition<SpriteRenderer> result)
```

### `DirectionalAnimationStateMachineSO<TState>` (abstract ScriptableObject)
Implements `IDirectionalAnimationStateMachine<TState>`.

```csharp
// Internal: array of (TState state, DirectionalAnimationSO anim) entries
```

Subclass this with a concrete `TState` (enum works well) and fill entries in the Inspector.

**Usage:**
```csharp
class CharacterAnimSM : DirectionalAnimationStateMachineSO<CharacterState> { }

// In code:
var anim = stateMachine.GetAnim(CharacterState.Walk, moveDirection);
player.Set(sr, anim, doLoop: true);
```

---

## Notes

- `AnimationPlayer<TTarget>` advances by 1 each `Update()` call — tie this to your game's frame rate or a fixed tick.
- `IsDone()` is only meaningful for non-looping animations.
- All definitions are ScriptableObjects: create them as Unity assets, assign sprites in the Inspector.
- `GetTags(firstFrameOnly: true)` only checks the first frame's tags — useful for "did this animation just start a new cycle?"
